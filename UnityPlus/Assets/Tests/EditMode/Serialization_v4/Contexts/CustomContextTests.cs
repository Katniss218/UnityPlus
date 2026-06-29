using NUnit.Framework;
using UnityPlus.Serialization;
using Ctx = UnityPlus.Serialization.Ctx;

namespace Neoserialization.V4
{
    public class CustomContextTests
    {
        public interface IMyInterfaceContext<T> : IContext { }

        public class MyClassContext : IMyInterfaceContext<int> { }

        public interface IInterfaceContextAlias<T> : Ctx.Array<T> where T : IContext { }

        [TearDown]
        public void Cleanup()
        {
            //ContextRegistry.Clear();
            // ContextRegistry doesn't have a Clear() method exposed for tests currently, 
            // but IDs are unique per run so it shouldn't conflict unless we check absolute IDs.
        }

        [Test]
        public void ContextRegistry_CustomGenericMarker_GeneratesStableUniqueIDs()
        {
            ContextKey id1 = ContextRegistry.GetID( typeof( IMyInterfaceContext<int> ) );
            ContextKey id2 = ContextRegistry.GetID( typeof( IMyInterfaceContext<string> ) );
            ContextKey id3 = ContextRegistry.GetID( typeof( IMyInterfaceContext<int> ) );

            Assert.That( id1, Is.Not.EqualTo( ContextKey.Default ) );
            Assert.That( id1, Is.Not.EqualTo( id2 ), "Different generic args should produce different IDs" );
            Assert.That( id1, Is.EqualTo( id3 ), "Same generic args should produce same ID" );
        }

        [Test]
        public void ContextRegistry_ClassImplementingMarker_MapsToMarkerID()
        {
            ContextKey markerID = ContextRegistry.GetID( typeof( IMyInterfaceContext<int> ) );
            ContextKey objID = ContextRegistry.GetID( typeof( MyClassContext ) );

            Assert.That( objID, Is.EqualTo( markerID ) );
        }

        [Test]
        public void ContextRegistry_ReverseLookup_ReturnsCorrectType()
        {
            ContextKey id = ContextRegistry.GetID( typeof( IMyInterfaceContext<float> ) );

            System.Type recoveredType = id.Type;

            Assert.That( recoveredType, Is.EqualTo( typeof( IMyInterfaceContext<float> ) ) );
        }
    }
}