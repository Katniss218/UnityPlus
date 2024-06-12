using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace Neoserialization
{
    public class MappingTests_Vectors
    {
        [Test]
        public void Mapping___Vector2___RoundTrip()
        {
            // Arrange
            var initialValue = new Vector2( 5f, -342.525242342f );

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<Vector2>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___Vector3___RoundTrip()
        {
            // Arrange
            var initialValue = new Vector3( float.Epsilon, float.MinValue, 563f );

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<Vector3>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___Vector3Dbl___RoundTrip()
        {
            // Arrange
            var initialValue = new Vector3Dbl( double.MinValue, double.Epsilon, 563 );

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<Vector3Dbl>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___Tuple_IntString___RoundTrip()
        {
            // Arrange
            var initialValue = (-5, "hello");

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<ValueTuple<int, string>>( data );

            // Assert
            Assert.That( initialValue.Item1, Is.EqualTo( finalValue.Item1 ) );
            Assert.That( initialValue.Item2, Is.EqualTo( finalValue.Item2 ) );
        }

        [Test]
        public void Mapping___Tuple_ObjectObject___RoundTrip()
        {
            // Arrange
            var initialValue = (new int[] { 1, 2, 3, 4, 5 }, new float[] { 1.1f, 2.2f, 3.3f, 4.4f, 5.5f });

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<ValueTuple<int[], float[]>>( data );

            // Assert
            Assert.That( initialValue.Item1, Is.EquivalentTo( finalValue.Item1 ) );
            Assert.That( initialValue.Item2, Is.EquivalentTo( finalValue.Item2 ) );
        }
    }
}