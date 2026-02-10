//using NUnit.Framework;
//using System;
//using UnityPlus.Serialization;

//namespace Neoserialization
//{
//    public class MappingTests_Tuples
//    {
//        [Test]
//        public void Mapping___Tuple_IntString___RoundTrip()
//        {
//            // Arrange
//            var initialValue = (-5, "hello");

//            // Act
//            var data = SerializationUnit.Serialize<object>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<object>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Tuple_ArrayArray___RoundTrip()
//        {
//            // Arrange
//            var initialValue = (new int[] { 1, 2, 3, 4, 5 }, new float[] { 1.1f, 2.2f, 3.3f, 4.4f, 5.5f });

//            // Act
//            var data = SerializationUnit.Serialize<ValueTuple<int[], float[]>>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<ValueTuple<int[], float[]>>( data );
            
//            // Assert
//            Assert.That( finalValue.Item1, Is.EquivalentTo( initialValue.Item1 ) );
//            Assert.That( finalValue.Item2, Is.EquivalentTo( initialValue.Item2 ) );
//        }

//        [Test]
//        public void Mapping___Tuple_3___RoundTrip()
//        {
//            // Arrange
//            var initialValue = (-5.1f, "hi", 'g');

//            // Act
//            var data = SerializationUnit.Serialize<object>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<object>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Tuple_7___RoundTrip()
//        {
//            // Arrange
//            var initialValue = (-5.0f, 5, 5, 5, 5, 5, 5);

//            // Act
//            var data = SerializationUnit.Serialize<object>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<object>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }
        
//        [Test]
//        public void Mapping___Tuple_TRest8___RoundTrip()
//        {
//            // Arrange
//            var initialValue = (-5.0f, 5, 5, 5, 5, 5, 5, 7);

//            // Act
//            var data = SerializationUnit.Serialize<object>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<object>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Tuple_TRest11___RoundTrip()
//        {
//            // Arrange
//            var initialValue = (-5, 5, 5, 5, 5, 5, 5, 5, 4, 5, 3);

//            // Act
//            var data = SerializationUnit.Serialize<object>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<object>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }
//    }
//}