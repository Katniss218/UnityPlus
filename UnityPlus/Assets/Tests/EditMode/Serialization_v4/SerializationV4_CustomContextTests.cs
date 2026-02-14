
using NUnit.Framework;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Ctx;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_CustomContextTests
    {
        public interface IMyMarker<T> : IContext { }

        public class MyMarkedObject : IMyMarker<int> { }

        // Test interface inheriting from core context to verify alias vs distinct behavior
        public interface IMyListAlias<T> : Ctx.Array<T> where T : IContext { }

        [TearDown]
        public void Cleanup()
        {
            // ContextRegistry doesn't have a Clear() method exposed for tests currently, 
            // but IDs are unique per run so it shouldn't conflict unless we check absolute IDs.
        }

        [Test]
        public void ContextRegistry_CustomGenericMarker_GeneratesStableUniqueIDs()
        {
            ContextKey id1 = ContextRegistry.GetID( typeof( IMyMarker<int> ) );
            ContextKey id2 = ContextRegistry.GetID( typeof( IMyMarker<string> ) );
            ContextKey id3 = ContextRegistry.GetID( typeof( IMyMarker<int> ) );

            Assert.That( id1, Is.Not.EqualTo( ContextKey.Default ) );
            Assert.That( id1, Is.Not.EqualTo( id2 ), "Different generic args should produce different IDs" );
            Assert.That( id1, Is.EqualTo( id3 ), "Same generic args should produce same ID" );
        }

        [Test]
        public void ContextRegistry_ClassImplementingMarker_MapsToMarkerID()
        {
            ContextKey markerID = ContextRegistry.GetID( typeof( IMyMarker<int> ) );
            ContextKey objID = ContextRegistry.GetID( typeof( MyMarkedObject ) );

            Assert.That( objID, Is.EqualTo( markerID ) );
        }

        [Test]
        public void ContextRegistry_ReverseLookup_ReturnsCorrectType()
        {
            ContextKey id = ContextRegistry.GetID( typeof( IMyMarker<float> ) );

            System.Type recoveredType = id.Type;

            Assert.That( recoveredType, Is.EqualTo( typeof( IMyMarker<float> ) ) );
        }
    }
}