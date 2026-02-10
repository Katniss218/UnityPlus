//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityPlus.Serialization;

//namespace Neoserialization
//{
//    public class MappingTests_Vectors
//    {
//        [Test]
//        public void Mapping___Vector2___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new Vector2( 5f, -342.525242342f );

//            // Act
//            var data = SerializationUnit.Serialize<Vector2>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<Vector2>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Vector2Int___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new Vector2Int( 5, -342 );

//            // Act
//            var data = SerializationUnit.Serialize<Vector2Int>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<Vector2Int>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Vector3___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new Vector3( float.Epsilon, float.MinValue, 563f );

//            // Act
//            var data = SerializationUnit.Serialize<Vector3>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<Vector3>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Vector3Int___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new Vector3Int( 5, -342, 564 );

//            // Act
//            var data = SerializationUnit.Serialize<Vector3Int>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<Vector3Int>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Vector3Dbl___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new Vector3Dbl( double.MinValue, double.Epsilon, 563 );

//            // Act
//            var data = SerializationUnit.Serialize<Vector3Dbl>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<Vector3Dbl>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Vector4___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new Vector4( float.Epsilon, float.MinValue, 563f, -5.6f );

//            // Act
//            var data = SerializationUnit.Serialize<Vector4>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<Vector4>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Quaternion___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new Quaternion( 1f, 2f, 3f, 4f );

//            // Act
//            var data = SerializationUnit.Serialize<Quaternion>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<Quaternion>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___QuaternionDbl___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new QuaternionDbl( 1f, 2f, 3f, 4f );

//            // Act
//            var data = SerializationUnit.Serialize<QuaternionDbl>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<QuaternionDbl>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }
//    }
//}