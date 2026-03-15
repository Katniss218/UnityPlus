using NUnit.Framework;
using System;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;
using Ctx = UnityPlus.Serialization.Ctx;

namespace Neoserialization.V4
{
    public class ContextGenericsTests
    {
        [TearDown]
        public void Cleanup()
        {
            TypeDescriptorRegistry.Clear();
            AssetRegistry.Clear();
        }

        [Test]
        public void TypeDescriptorRegistry_DirectPath()
        {
            // Clear registry to ensure we scan real providers
            TypeDescriptorRegistry.Clear();

            // Request descriptor for Material in Asset context
            // Should return AssetDescriptor<Material> (from UnityAssetDescriptors.MaterialAsset)
            ContextKey assetCtx = ContextRegistry.GetID( typeof( Ctx.Asset ) );
            IDescriptor desc = TypeDescriptorRegistry.GetDescriptor( typeof( Material ), assetCtx );

            Assert.That( desc, Is.InstanceOf<AssetDescriptor<Material>>() );
        }

        [Test]
        public void TypeDescriptorRegistry_IndirectPath_Fallback()
        {
            // Clear registry to ensure we scan real providers
            TypeDescriptorRegistry.Clear();

            // Request descriptor for Material in Ref context (which has no specific provider)
            // Should fallback to DefaultProvider -> MemberwiseDescriptor<Material>
            ContextKey refCtx = ContextRegistry.GetID( typeof( Ctx.Ref ) );
            IDescriptor desc = TypeDescriptorRegistry.GetDescriptor( typeof( Material ), refCtx );

            Assert.That( desc, Is.InstanceOf<ReferenceDescriptor<Material>>() );
        }

        [Test]
        public void TypeDescriptorRegistry_TraversesInheritanceAndContextFallback()
        {
            // Clear registry to ensure we scan real providers
            TypeDescriptorRegistry.Clear();

            // Case 1: Texture2D in Asset Context
            // Texture2D inherits Texture.
            // UnityAssetDescriptors has [MapsInheritingFrom(typeof(Texture), Ctx.Asset)] -> TextureAsset()
            // Should return AssetDescriptor<Texture>
            ContextKey assetCtx = ContextRegistry.GetID( typeof( Ctx.Asset ) );
            IDescriptor descTex = TypeDescriptorRegistry.GetDescriptor( typeof( Texture2D ), assetCtx );
            Assert.That( descTex, Is.InstanceOf<AssetDescriptor<Texture2D>>() );

            IDescriptor descTexDef = TypeDescriptorRegistry.GetDescriptor( typeof( Shader ), assetCtx );
            Assert.That( descTexDef, Is.InstanceOf<AssetDescriptor<Shader>>() );

            // Case 3: BoxCollider in Asset Context
            // BoxCollider inherits Object -> object.
            // UnityPrimitiveDescriptors has generic ProvideAsset<T> for object in Ctx.Asset.
            // Should return AssetDescriptor<BoxCollider>
            IDescriptor descCol = TypeDescriptorRegistry.GetDescriptor( typeof( BoxCollider ), assetCtx );
            Assert.That( descCol, Is.InstanceOf<AssetDescriptor<BoxCollider>>() );

            ContextKey ctx = ContextRegistry.GetID( typeof( Ctx.Array<Ctx.Asset> ) );
            IDescriptor c = TypeDescriptorRegistry.GetDescriptor( typeof( Material[] ), ctx );
            Assert.That( c, Is.InstanceOf<IndexedCollectionDescriptor<Material[], Material>>() );
            Assert.That( ((IndexedCollectionDescriptor<Material[], Material>)c).ElementSelector, Is.InstanceOf<UniformSelector>() );
            Assert.That( ((IndexedCollectionDescriptor<Material[], Material>)c).ElementSelector.Select( default ), Is.EqualTo( assetCtx ) );
        }

        [Test]
        public void ContextRegistry_ResolvesValueAsDefault()
        {
            // Test: typeof(Ctx.List<Ctx.Asset>)
            ContextKey contextId = ContextRegistry.GetID( typeof( Ctx.Value ) );

            Assert.That( contextId.ID, Is.EqualTo( 0 ) );
            Assert.That( contextId, Is.EqualTo( ContextKey.Default ) );
        }

        [Test]
        public void ContextRegistry_ResolvesGenericDictionary()
        {
            // Test: typeof(Ctx.Dict<Ctx.Default, Ctx.Asset>)
            // Should produce a valid Context ID that maps to (Key=Default, Val=Asset)

            Type contextType = typeof( Ctx.KeyValue<Ctx.Value, Ctx.Asset> );
            ContextKey contextId = ContextRegistry.GetID( contextType );

            Assert.That( contextId.ID, Is.Not.EqualTo( 0 ), "Generic Dictionary Context should not result in Default(0)" );

            var rules = ContextRegistry.GetContextArguments( contextId );
            ContextKey keyId = ContextRegistry.GetID( typeof( Ctx.Value ) );
            ContextKey valueId = ContextRegistry.GetID( typeof( Ctx.Asset ) );

            Assert.That( rules[0], Is.EqualTo( keyId ) );
            Assert.That( rules[1], Is.EqualTo( valueId ) );
        }

        [Test]
        public void ContextRegistry_ResolvesGenericList()
        {
            // Test: typeof(Ctx.List<Ctx.Asset>)
            Type contextType = typeof( Ctx.Array<Ctx.Asset> );
            ContextKey contextId = ContextRegistry.GetID( contextType );

            Assert.That( contextId.ID, Is.Not.EqualTo( 0 ) );

            ContextKey elemCtx = ContextRegistry.GetContextArguments( contextId )[0];
            ContextKey expectedAssetCtx = ContextRegistry.GetID( typeof( Ctx.Asset ) );

            Assert.That( elemCtx, Is.EqualTo( expectedAssetCtx ) );
        }

        [Test]
        public void ContextRegistry_ResolvesNestedGeneric()
        {
            // Test: typeof(Ctx.Dict<Ctx.Default, Ctx.List<Ctx.Asset>>)
            // Dictionary where Value is a List of Assets

            Type listContextType = typeof( Ctx.Array<Ctx.Asset> );
            Type dictContextType = typeof( Ctx.KeyValue<Ctx.Value, Ctx.Array<Ctx.Asset>> );

            ContextKey listId = ContextRegistry.GetID( listContextType );
            ContextKey dictId = ContextRegistry.GetID( dictContextType );

            var rules = ContextRegistry.GetContextArguments( dictId );

            Assert.That( rules[1], Is.EqualTo( listId ) ); // Value context should match the List context ID

            // Verify the list context itself resolves correctly
            ContextKey assetCtx = ContextRegistry.GetID( typeof( Ctx.Asset ) );
            var rules2 = ContextRegistry.GetContextArguments( rules[1] );
            Assert.That( rules2[0], Is.EqualTo( assetCtx ) );
        }

        [Test]
        public void ArrayDescriptor_HasCorrectElementSelector()
        {
            TypeDescriptorRegistry.Clear();

            ContextKey arrayAssetCtx = ContextRegistry.GetID( typeof( Ctx.Array<Ctx.Asset> ) );
            IDescriptor desc = TypeDescriptorRegistry.GetDescriptor( typeof( Material[] ), arrayAssetCtx );

            Assert.That( desc, Is.InstanceOf<IndexedCollectionDescriptor<Material[], Material>>() );
            var arrayDesc = (IndexedCollectionDescriptor<Material[], Material>)desc;

            Assert.That( arrayDesc.ElementSelector, Is.InstanceOf<UniformSelector>() );

            ContextKey elementCtx = ((UniformSelector)arrayDesc.ElementSelector).Select( default );
            ContextKey assetCtx = ContextRegistry.GetID( typeof( Ctx.Asset ) );

            Assert.That( elementCtx.ID, Is.EqualTo( assetCtx.ID ) );
        }
    }
}