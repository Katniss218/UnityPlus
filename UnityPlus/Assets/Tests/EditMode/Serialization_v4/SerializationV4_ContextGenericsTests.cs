using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.AssetManagement;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_ContextGenericsTests
    {
        [TearDown]
        public void Cleanup()
        {
            TypeDescriptorRegistry.Clear();
            AssetRegistry.Clear();
        }

        public class MockAsset : ScriptableObject
        {
            public int Value;
        }

        public class Container
        {
            public Dictionary<string, MockAsset> Assets = new Dictionary<string, MockAsset>();
        }

        [Test]
        public void ContextRegistry_ResolvesGenericDictionary()
        {
            // Test: typeof(Ctx.Dict<Ctx.Default, Ctx.Asset>)
            // Should produce a valid Context ID that maps to (Key=Default, Val=Asset)

            Type contextType = typeof( Ctx.KeyValue<Ctx.Value, Ctx.Asset> );
            ContextKey contextId = ContextRegistry.GetID( contextType );

            Assert.That( contextId.ID, Is.Not.EqualTo( 0 ), "Generic Dictionary Context should not result in Default(0)" );

            var rules = ContextRegistry.GetDictionaryElementContexts( contextId );
            ContextKey keyId = ContextRegistry.GetID( typeof( Ctx.Value ) );
            ContextKey valueId = ContextRegistry.GetID( typeof( Ctx.Asset ) );

            Assert.That( rules.keyCtx, Is.EqualTo( keyId ) );
            Assert.That( rules.valCtx, Is.EqualTo( valueId ) );
        }

        [Test]
        public void ContextRegistry_ResolvesGenericList()
        {
            // Test: typeof(Ctx.List<Ctx.Asset>)
            Type contextType = typeof( Ctx.Array<Ctx.Asset> );
            ContextKey contextId = ContextRegistry.GetID( contextType );

            Assert.That( contextId.ID, Is.Not.EqualTo( 0 ) );

            ContextKey elemCtx = ContextRegistry.GetCollectionElementContext( contextId );
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

            var rules = ContextRegistry.GetDictionaryElementContexts( dictId );

            Assert.That( rules.valCtx, Is.EqualTo( listId ) ); // Value context should match the List context ID

            // Verify the list context itself resolves correctly
            ContextKey assetCtx = ContextRegistry.GetID( typeof( Ctx.Asset ) );
            Assert.That( ContextRegistry.GetCollectionElementContext( rules.valCtx ), Is.EqualTo( assetCtx ) );
        }

        [Test]
        public void Serialization_Dictionary_Asset()
        {
            AssetRegistry.Clear();
            // Arrange: Object with Dictionary, configured via Fluent API using Generic Context Type
            var asset = ScriptableObject.CreateInstance<MockAsset>();
            asset.Value = 42;
            string assetId = "test::mock";

            AssetRegistry.Register( assetId, asset );

            var container = new Container();
            container.Assets.Add( "MyAsset", asset );

            TypeDescriptorRegistry.Register(
                new MemberwiseDescriptor<Container>()
                    // Use the generic type to specify context: Dict<Default, Asset>
                    .WithMember( "assets", typeof( Ctx.KeyValue<Ctx.Value, Ctx.Asset> ), c => c.Assets )
            );

            // Act: Serialize
            SerializedData data = SerializationUnit.Serialize( container );

            // Assert: Verify the value is serialized as an AssetRef (String), not a full object
            // Expected: { "assets": { "values": [ { "key": "MyAsset", "value": { "$assetref": "test::mock" } } ] } }
            // Actual: { "assets": { "$id": "..." } }

            var obj = (SerializedObject)data;
            SerializedArray assetsArr = SerializationHelpers.GetValueNode( obj["assets"] );

            SerializedObject entry = (SerializedObject)assetsArr[0];
            SerializedData valNode = entry["value"];

            AssetRegistry.Clear();
            Assert.That( valNode, Is.TypeOf<SerializedObject>(), "Asset should be serialized as object node (containing ref)" );
            Assert.That( ((SerializedObject)valNode).ContainsKey( KeyNames.ASSETREF ), Is.True, "Value should contain $assetref key" );
            Assert.That( (string)((SerializedObject)valNode)[KeyNames.ASSETREF], Is.EqualTo( assetId ) );
        }
    }
}