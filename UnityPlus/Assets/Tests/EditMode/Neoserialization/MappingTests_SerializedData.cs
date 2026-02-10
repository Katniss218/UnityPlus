//using NUnit.Framework;
//using UnityPlus.Serialization;

//namespace Neoserialization
//{
//    public class MappingTests_SerializedData
//    {
//        [Test]
//        public void Mapping___SerializedPrimitive_Polymorphic___RoundTrip()
//        {
//            // Arrange
//            var initialValue = (SerializedPrimitive)5.4112f;

//            // Act
//            var data = SerializationUnit.Serialize<SerializedData>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<SerializedData>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }
        
//        [Test]
//        public void Mapping___SerializedPrimitive_Null_Polymorphic___RoundTrip()
//        {
//            // Arrange
//            var initialValue = (SerializedPrimitive)null;

//            // Act
//            var data = SerializationUnit.Serialize<SerializedData>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<SerializedData>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___SerializedArray_Empty_Polymorphic___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new SerializedArray()
//            {
//            };

//            // Act
//            var data = SerializationUnit.Serialize<SerializedData>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<SerializedData>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }
        
//        [Test]
//        public void Mapping___SerializedArray_Polymorphic___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new SerializedArray()
//            { 
//                (SerializedPrimitive)5.4112f,
//                (SerializedPrimitive)"hello world"
//            };

//            // Act
//            var data = SerializationUnit.Serialize<SerializedData>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<SerializedData>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___SerializedObject_Empty_Polymorphic___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new SerializedObject()
//            {
//            };

//            // Act
//            var data = SerializationUnit.Serialize<SerializedData>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<SerializedData>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___SerializedObject_Polymorphic___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new SerializedObject()
//            {
//                { "member1", (SerializedPrimitive)5.4112f },
//                { "member2", (SerializedPrimitive)"hello world" }
//            };

//            // Act
//            var data = SerializationUnit.Serialize<SerializedData>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<SerializedData>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }
//    }
//}