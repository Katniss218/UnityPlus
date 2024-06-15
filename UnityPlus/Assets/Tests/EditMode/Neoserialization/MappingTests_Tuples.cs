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
    public class MappingTests_Tuples
    {
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

        [Test]
        public void Mapping___Tuple_3___RoundTrip()
        {
            // Arrange
            var initialValue = (-5.1f, "hi", 'g');

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<object>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___Tuple_7___RoundTrip()
        {
            // Arrange
            var initialValue = (-5, 5, 5, 5, 5, 5, 5);

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<object>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___Tuple_SplitIntoTRest___RoundTrip()
        {
            // Arrange
            var initialValue = (-5, 5, 5, 5, 5, 5, 5, 5, 4, 5, 3);

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<object>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }
    }
}