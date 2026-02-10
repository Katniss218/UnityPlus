//using NUnit.Framework;
//using System;
//using UnityPlus.Serialization;

//namespace Neoserialization
//{
//    public class MappingTests_Primitives
//    {
//        [Test]
//        public void Mapping___Boolean___RoundTrip()
//        {
//            // Arrange
//            var initialValue = true;

//            // Act
//            var data = SerializationUnit.Serialize<bool>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<bool>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Byte___RoundTrip()
//        {
//            // Arrange
//            byte initialValue = byte.MaxValue;

//            // Act
//            var data = SerializationUnit.Serialize<byte>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<byte>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___SByte___RoundTrip()
//        {
//            // Arrange
//            sbyte initialValue = sbyte.MinValue;

//            // Act
//            var data = SerializationUnit.Serialize<sbyte>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<sbyte>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Int16___RoundTrip()
//        {
//            // Arrange
//            short initialValue = short.MinValue;

//            // Act
//            var data = SerializationUnit.Serialize<short>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<short>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Int32___RoundTrip()
//        {
//            // Arrange
//            int initialValue = int.MinValue;

//            // Act
//            var data = SerializationUnit.Serialize<int>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<int>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Int64___RoundTrip()
//        {
//            // Arrange
//            long initialValue = long.MinValue;

//            // Act
//            var data = SerializationUnit.Serialize<long>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<long>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Float___RoundTrip()
//        {
//            // Arrange
//            float initialValue = float.Epsilon;

//            // Act
//            var data = SerializationUnit.Serialize<float>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<float>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Double___RoundTrip()
//        {
//            // Arrange
//            double initialValue = double.Epsilon;

//            // Act
//            var data = SerializationUnit.Serialize<double>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<double>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }
        
//        [Test]
//        public void Mapping___Decimal___RoundTrip()
//        {
//            // Arrange
//            decimal initialValue = decimal.MinValue;

//            // Act
//            var data = SerializationUnit.Serialize<decimal>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<decimal>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Char___RoundTrip()
//        {
//            // Arrange
//            var initialValue = 'ę';

//            // Act
//            var data = SerializationUnit.Serialize<char>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<char>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___String___RoundTrip()
//        {
//            // Arrange
//            var initialValue = "test \new \random ayhdsad231";

//            // Act
//            var data = SerializationUnit.Serialize<string>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<string>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___DateTime___RoundTrip()
//        {
//            // Arrange
//            var initialValue = DateTime.UtcNow;

//            // Act
//            var data = SerializationUnit.Serialize<DateTime>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<DateTime>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___DateTimeOffset___RoundTrip()
//        {
//            // Arrange
//            var initialValue = DateTimeOffset.UtcNow;

//            // Act
//            var data = SerializationUnit.Serialize<DateTimeOffset>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<DateTimeOffset>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___TimeSpan___RoundTrip()
//        {
//            // Arrange
//            var initialValue = TimeSpan.FromTicks( 342534252 );

//            // Act
//            var data = SerializationUnit.Serialize<TimeSpan>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<TimeSpan>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Enum___RoundTrip()
//        {
//            // Arrange
//            var initialValue = AnEnum.Second;

//            // Act
//            var data = SerializationUnit.Serialize<AnEnum>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<AnEnum>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        public static void TestMethod() { }

//        [Test]
//        public void Mapping___Delegate_Parameterless___RoundTrip()
//        {
//            // Arrange
//            Action initialValue = TestMethod;

//            // Act
//            var data = SerializationUnit.Serialize<Action>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<Action>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }
//    }
//}