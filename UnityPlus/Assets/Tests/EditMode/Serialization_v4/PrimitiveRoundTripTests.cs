using NUnit.Framework;
using System;
using UnityEngine;

namespace UnityPlus.Serialization.Tests.V4
{
    public class PrimitiveRoundTripTests
    {
        [TearDown]
        public void Cleanup()
        {
            TypeDescriptorRegistry.Clear();
        }

        private T RoundTrip<T>( T original )
        {
            var data = SerializationUnit.Serialize<T>( original );
            return SerializationUnit.Deserialize<T>( data );
        }

        private void AssertRoundTrip<T>( T original )
        {
            T result = RoundTrip( original );
            Assert.That( result, Is.EqualTo( original ) );
        }

        // --- System Primitives ---

        [Test]
        public void System_Integers()
        {
            AssertRoundTrip<byte>( 255 );
            AssertRoundTrip<sbyte>( -127 );
            AssertRoundTrip<short>( -32000 );
            AssertRoundTrip<ushort>( 65000 );
            AssertRoundTrip<int>( -2000000000 );
            AssertRoundTrip<uint>( 4000000000 );
            AssertRoundTrip<long>( -9000000000000000000 );
            AssertRoundTrip<ulong>( 18000000000000000000 );
        }

        [Test]
        public void System_Floats()
        {
            AssertRoundTrip<float>( 123.456f );
            AssertRoundTrip<double>( 123.4567890123 );
            AssertRoundTrip<decimal>( 123.456m );
        }

        [Test]
        public void System_TextAndBool()
        {
            AssertRoundTrip<bool>( true );
            AssertRoundTrip<bool>( false );
            AssertRoundTrip<char>( 'Z' );
            AssertRoundTrip<string>( "Hello World \n \t \"" );
        }

        // --- Extended System Types ---

        [Test]
        public void System_Extended()
        {
            AssertRoundTrip<Guid>( Guid.NewGuid() );
            AssertRoundTrip<DateTime>( DateTime.UtcNow );
            AssertRoundTrip<DateTimeOffset>( DateTimeOffset.UtcNow );
            AssertRoundTrip<TimeSpan>( TimeSpan.FromMinutes( 123.5 ) );
        }

        [Test]
        public void System_ValueTuples()
        {
            AssertRoundTrip( (1, "A") );
            AssertRoundTrip( (1, 2.5f, "B") );
            AssertRoundTrip( (1, 2, 3, 4) );
        }

        // --- Unity Math ---

        [Test]
        public void Unity_Vectors()
        {
            AssertRoundTrip( new Vector2( 1.1f, 2.2f ) );
            AssertRoundTrip( new Vector3( 1.1f, 2.2f, 3.3f ) );
            AssertRoundTrip( new Vector4( 1.1f, 2.2f, 3.3f, 4.4f ) );
            AssertRoundTrip( new Vector2Int( 1, 2 ) );
            AssertRoundTrip( new Vector3Int( 1, 2, 3 ) );
        }

        [Test]
        public void Unity_Quaternion()
        {
            // Note: Precision issues can occur with floating point serialization, 
            // but Is.EqualTo handles Unity objects with a small epsilon usually.
            AssertRoundTrip( Quaternion.Euler( 30, 45, 60 ) );
        }

        [Test]
        public void Unity_Matrix4x4()
        {
            Matrix4x4 m = Matrix4x4.TRS( new Vector3( 1, 2, 3 ), Quaternion.Euler( 0, 90, 0 ), Vector3.one * 2 );
            AssertRoundTrip( m );
        }

        [Test]
        public void Unity_Colors()
        {
            AssertRoundTrip( new Color( 0.1f, 0.2f, 0.3f, 0.4f ) );
            AssertRoundTrip( new Color32( 10, 20, 30, 40 ) );
        }

        // --- Unity Geometry ---

        [Test]
        public void Unity_Geometry_RectBounds()
        {
            AssertRoundTrip( new Rect( 10, 10, 100, 50 ) );
            AssertRoundTrip( new RectInt( 10, 10, 100, 50 ) );
            AssertRoundTrip( new Bounds( Vector3.one, Vector3.one * 5 ) );
            AssertRoundTrip( new BoundsInt( Vector3Int.one, Vector3Int.one * 5 ) );
        }

        [Test]
        public void Unity_Geometry_RayPlane()
        {
            // Ray does not override Equals in some Unity versions, check manually
            var ray = new Ray( Vector3.zero, Vector3.up );
            var resRay = RoundTrip( ray );
            Assert.That( resRay.origin, Is.EqualTo( ray.origin ) );
            Assert.That( resRay.direction, Is.EqualTo( ray.direction ) );

            var plane = new Plane( Vector3.up, 10f );
            var resPlane = RoundTrip( plane );
            Assert.That( resPlane.normal, Is.EqualTo( plane.normal ) );
            Assert.That( resPlane.distance, Is.EqualTo( plane.distance ) );
        }

        // --- Unity Animation & Gradients ---

        [Test]
        public void Unity_Keyframe()
        {
            var kf = new Keyframe( 0.5f, 10f, 1f, 1f );
            kf.weightedMode = WeightedMode.Both;

            // Keyframe is a struct, overrides Equals
            AssertRoundTrip( kf );
        }

        [Test]
        public void Unity_AnimationCurve()
        {
            var curve = new AnimationCurve(
                new Keyframe( 0, 0 ),
                new Keyframe( 1, 1 )
            );
            curve.preWrapMode = WrapMode.PingPong;
            curve.postWrapMode = WrapMode.Loop;

            var result = RoundTrip( curve );

            Assert.That( result.keys.Length, Is.EqualTo( curve.keys.Length ) );
            Assert.That( result.preWrapMode, Is.EqualTo( curve.preWrapMode ) );
            Assert.That( result.postWrapMode, Is.EqualTo( curve.postWrapMode ) );
            Assert.That( result.keys[1].value, Is.EqualTo( curve.keys[1].value ) );
        }

        [Test]
        public void Unity_Gradient()
        {
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey( Color.red, 0 ), new GradientColorKey( Color.blue, 1 ) },
                new[] { new GradientAlphaKey( 1, 0 ), new GradientAlphaKey( 0, 1 ) }
            );
            grad.mode = GradientMode.Fixed;

            var result = RoundTrip( grad );

            Assert.That( result.mode, Is.EqualTo( grad.mode ) );
            Assert.That( result.colorKeys.Length, Is.EqualTo( grad.colorKeys.Length ) );
            Assert.That( result.alphaKeys.Length, Is.EqualTo( grad.alphaKeys.Length ) );

            Assert.That( result.colorKeys[0].color, Is.EqualTo( grad.colorKeys[0].color ) );
            Assert.That( result.alphaKeys[1].alpha, Is.EqualTo( grad.alphaKeys[1].alpha ) );
        }
    }
}