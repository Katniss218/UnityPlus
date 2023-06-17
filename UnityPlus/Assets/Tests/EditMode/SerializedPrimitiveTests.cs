using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Serialization.ComponentData;
using UnityEngine.Serialization.Json;
using UnityEngine.TestTools;

namespace Serialization
{
    public class SerializedValueTests
    {
        [Test]
        public void Equality___ValueType_Content___ByValue()
        {
            // Arrange
            SerializedPrimitive val1 = (SerializedPrimitive)null;
            SerializedPrimitive val2 = (SerializedPrimitive)null;

            SerializedPrimitive val3 = (SerializedPrimitive)true;
            SerializedPrimitive val4 = (SerializedPrimitive)false;

            SerializedPrimitive val5 = (SerializedPrimitive)5.125f;
            SerializedPrimitive val6 = (SerializedPrimitive)5.125f;

            // Act

            // Assert
            Assert.That( val1, Is.EqualTo( val2 ) );
            Assert.That( val3, Is.Not.EqualTo( val4 ) );
            Assert.That( val5, Is.EqualTo( val6 ) );
        }

        [Test]
        public void Equality___String_Content___ByValue()
        {
            // Arrange
            SerializedPrimitive val1 = (SerializedPrimitive)"hello";
            SerializedPrimitive val2 = (SerializedPrimitive)"hello";

            SerializedPrimitive val3 = (SerializedPrimitive)"Hi";
            SerializedPrimitive val4 = (SerializedPrimitive)"Hey";

            // Act

            // Assert
            Assert.That( val1, Is.EqualTo( val2 ) );
            Assert.That( val3, Is.Not.EqualTo( val4 ) );
        }

        //[Test]
        //public void Equality___Serialized_Content___ByReference()
        //{
#warning TODO - object and array need == and equals ops too now.
        //}

        [Test]
        public void RoundTrip___Boolean()
        {
            // Arrange
            bool original = true;
            bool roundtripped = (bool)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___Int8()
        {
            // Arrange
            sbyte original = sbyte.MinValue;
            sbyte roundtripped = (sbyte)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___UInt8()
        {
            // Arrange
            byte original = byte.MinValue;
            byte roundtripped = (byte)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }
        
        [Test]
        public void RoundTrip___Int16()
        {
            // Arrange
            short original = short.MinValue;
            short roundtripped = (short)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___UInt16()
        {
            // Arrange
            ushort original = ushort.MinValue;
            ushort roundtripped = (ushort)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }
        
        [Test]
        public void RoundTrip___Int32()
        {
            // Arrange
            int original = int.MinValue;
            int roundtripped = (int)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___UInt32()
        {
            // Arrange
            uint original = uint.MinValue;
            uint roundtripped = (uint)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___Int64()
        {
            // Arrange
            long original = long.MinValue;
            long roundtripped = (long)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }
        
        [Test]
        public void RoundTrip___UInt64()
        {
            // Arrange
            ulong original = ulong.MinValue;
            ulong roundtripped = (ulong)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___Single()
        {
            // Arrange
            float original = float.MinValue;
            float roundtripped = (float)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }
        
        [Test]
        public void RoundTrip___Double()
        {
            // Arrange
            double original = double.MinValue;
            double roundtripped = (double)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___Decimal()
        {
            // Arrange
            decimal original = decimal.MinValue;
            decimal roundtripped = (decimal)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }
        
        [Test]
        public void RoundTrip___String()
        {
            // Arrange
            string original = "Hello World! Hi! \r\n \u0020 123";
            string roundtripped = (string)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void Conversion___Int8_To_Int64()
        {
            // Arrange
            sbyte original = sbyte.MinValue;
            long roundtripped = (long)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void Conversion___Int16_To_Int64()
        {
            // Arrange
            short original = short.MinValue;
            long roundtripped = (long)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void Conversion___Int32_To_Int64()
        {
            // Arrange
            int original = int.MinValue;
            long roundtripped = (long)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void Conversion___UInt32_To_Int64()
        {
            // Arrange
            uint original = 218u;
            long expected = 218;
            long roundtripped = (long)(SerializedPrimitive)original;

            // Assert
            Assert.That( roundtripped, Is.EqualTo( expected ) );
        }

        [Test]
        public void Conversion___Single_To_Double()
        {
            // Arrange
            float original = float.MinValue;
            double roundtripped = (double)(SerializedPrimitive)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void Conversion___Int32_To_Float()
        {
            // Arrange
            int original = 32767;
            float expected = 32767.0f;
            float roundtripped = (float)(SerializedPrimitive)original;

            // Assert
            Assert.That( roundtripped, Is.EqualTo( expected ) );
        }
    }
}