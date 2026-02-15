using NUnit.Framework;
using System;

namespace UnityPlus.Serialization.Tests.V4
{
    public class StructAllocationTests
    {
        public struct LargeStruct
        {
            public int A, B, C, D, E, F, G, H;
            public float X, Y, Z;
            public double D1, D2;
        }

        private RefSetter<object, object> _setter;
        private object _boxedStruct;

        [SetUp]
        public void Setup()
        {
            // Prepare a boxed struct and the setter delegate
            _boxedStruct = new LargeStruct();
            var field = typeof( LargeStruct ).GetField( nameof( LargeStruct.A ) );
            _setter = AccessorUtils.CreateUntypedStructSetter( field );
        }

        [Test]
        public void SetStructField_AllocationTest()
        {
            // Warmup
            object val = 1;
            _setter( ref _boxedStruct, val );

            // Measure
            GC.Collect();
            long startBytes = GC.GetAllocatedBytesForCurrentThread();

            //int iterations = 100000000;
            int iterations = 1000000;
            for( int i = 0; i < iterations; i++ )
            {
                // This call should now be allocation free.
                // NOTE: 'val' is an int, which is boxed when passed as 'object'.
                // To test strictly the Setter logic, we reuse the same boxed integer 'val' 
                // or assume int boxing is negligible compared to LargeStruct boxing.
                // However, CreateUntypedStructSetter takes 'object value'. 
                // Passing '1' creates a box for the integer. 
                // We should pass a pre-boxed integer to measure strictly the struct manipulation.
                _setter( ref _boxedStruct, val );
            }

            long endBytes = GC.GetAllocatedBytesForCurrentThread();
            long diff = endBytes - startBytes;

            // Verify logic worked
            Assert.That( ((LargeStruct)_boxedStruct).A, Is.EqualTo( 1 ) );

            // Assert Zero Allocations
            // If the optimization works, 'diff' should be 0. 
            // If it failed (old "rebox" method), it would be: iterations * sizeof(LargeStruct + Header).
            // LargeStruct is ~64 bytes. 1000 iter = 64KB.
            Assert.That( diff, Is.EqualTo( 0 ), $"Detected {diff} bytes allocated during struct field setting. Optimization failed." );
        }

        [Test]
        public void StructRefEquality_Maintained()
        {
            object originalRef = _boxedStruct;
            object val = 42;

            _setter( ref _boxedStruct, val );

            // With Expression.Unbox optimization, the reference should not change because we modified the heap object in-place.
            // With the old "Unbox-Copy-Rebox" method, the reference would point to a new box.
            Assert.That( ReferenceEquals( _boxedStruct, originalRef ), Is.True, "The boxed object reference changed. In-place modification failed." );

            // Verify value updated
            Assert.That( ((LargeStruct)_boxedStruct).A, Is.EqualTo( 42 ) );
        }
    }
}