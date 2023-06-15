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
            SerializedValue val1 = (SerializedValue)null;
            SerializedValue val2 = (SerializedValue)null;

            SerializedValue val3 = (SerializedValue)true;
            SerializedValue val4 = (SerializedValue)false;

            SerializedValue val5 = (SerializedValue)5.125f;
            SerializedValue val6 = (SerializedValue)5.125f;

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
            SerializedValue val1 = (SerializedValue)"hello";
            SerializedValue val2 = (SerializedValue)"hello";

            SerializedValue val3 = (SerializedValue)"Hi";
            SerializedValue val4 = (SerializedValue)"Hey";

            // Act

            // Assert
            Assert.That( val1, Is.EqualTo( val2 ) );
            Assert.That( val3, Is.Not.EqualTo( val4 ) );
        }

        [Test]
        public void Equality___Serialized_Content___ByReference()
        {
            // Arrange
            var r = new SerializedArray();
            SerializedValue val1 = (SerializedValue)r;
            SerializedValue val2 = (SerializedValue)r;

            SerializedValue val3 = (SerializedValue)new SerializedArray();
            SerializedValue val4 = (SerializedValue)new SerializedArray();

            // Act

            // Assert
            Assert.That( val1, Is.EqualTo( val2 ) );
            Assert.That( val3, Is.Not.EqualTo( val4 ) );
        }

        [Test]
        public void RoundTrip___Boolean()
        {
            // Arrange
            bool original = true;
            bool roundtripped = (bool)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___Int8()
        {
            // Arrange
            sbyte original = sbyte.MinValue;
            sbyte roundtripped = (sbyte)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___UInt8()
        {
            // Arrange
            byte original = byte.MinValue;
            byte roundtripped = (byte)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }
        
        [Test]
        public void RoundTrip___Int16()
        {
            // Arrange
            short original = short.MinValue;
            short roundtripped = (short)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___UInt16()
        {
            // Arrange
            ushort original = ushort.MinValue;
            ushort roundtripped = (ushort)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }
        
        [Test]
        public void RoundTrip___Int32()
        {
            // Arrange
            int original = int.MinValue;
            int roundtripped = (int)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___UInt32()
        {
            // Arrange
            uint original = uint.MinValue;
            uint roundtripped = (uint)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___Int64()
        {
            // Arrange
            long original = long.MinValue;
            long roundtripped = (long)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }
        
        [Test]
        public void RoundTrip___UInt64()
        {
            // Arrange
            ulong original = ulong.MinValue;
            ulong roundtripped = (ulong)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___Single()
        {
            // Arrange
            float original = float.MinValue;
            float roundtripped = (float)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }
        
        [Test]
        public void RoundTrip___Double()
        {
            // Arrange
            double original = double.MinValue;
            double roundtripped = (double)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void RoundTrip___Decimal()
        {
            // Arrange
            decimal original = decimal.MinValue;
            decimal roundtripped = (decimal)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }
        
        [Test]
        public void RoundTrip___String()
        {
            // Arrange
            string original = "Hello World! Hi! \r\n \u0020 123";
            string roundtripped = (string)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void Conversion___Int8_To_Int64()
        {
            // Arrange
            sbyte original = sbyte.MinValue;
            long roundtripped = (long)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void Conversion___Int16_To_Int64()
        {
            // Arrange
            short original = short.MinValue;
            long roundtripped = (long)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void Conversion___Int32_To_Int64()
        {
            // Arrange
            int original = int.MinValue;
            long roundtripped = (long)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void Conversion___UInt32_To_Int64()
        {
            // Arrange
            uint original = 218u;
            long expected = 218;
            long roundtripped = (long)(SerializedValue)original;

            // Assert
            Assert.That( roundtripped, Is.EqualTo( expected ) );
        }

        [Test]
        public void Conversion___Single_To_Double()
        {
            // Arrange
            float original = float.MinValue;
            double roundtripped = (double)(SerializedValue)original;

            // Assert
            Assert.That( original, Is.EqualTo( roundtripped ) );
        }

        [Test]
        public void Conversion___Int32_To_Float()
        {
            // Arrange
            int original = 32767;
            float expected = 32767.0f;
            float roundtripped = (float)(SerializedValue)original;

            // Assert
            Assert.That( roundtripped, Is.EqualTo( expected ) );
        }
    }
}