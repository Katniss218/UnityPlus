using NUnit.Framework;
using System.Text;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace Neoserialization
{
    public class MappingTests_Arrays
    {
        [Test]
        public void Mapping___NullArray___RoundTrip()
        {
            // Arrange
            int[] initialValue = null;

            // Act
            var data = SerializationUnit.Serialize<int[]>( initialValue );
            var finalValue = SerializationUnit.Deserialize<int[]>( data );

            // Assert
            Assert.That( finalValue, Is.Null );
        }

        [Test]
        public void Mapping___EmptyArray___RoundTrip()
        {
            // Arrange
            var initialValue = new int[] { };

            // Act
            var data = SerializationUnit.Serialize<int[]>( initialValue );
            var finalValue = SerializationUnit.Deserialize<int[]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___IntArray___RoundTrip()
        {
            // Arrange
            var initialValue = new int[] { 3, 4, 5, 9, 42, 218 };

            // Act
            var data = SerializationUnit.Serialize<int[]>( initialValue );
            var finalValue = SerializationUnit.Deserialize<int[]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___IntJaggedArray___RoundTrip()
        {
            // Arrange
            var initialValue = new int[][]
            {
                new int[] { 1 }, 
                new int[] { 2, 3 },
                new int[] { 4, 5, 6 }
            };

            // Act
            var data = SerializationUnit.Serialize<int[][]>( initialValue );
            var finalValue = SerializationUnit.Deserialize<int[][]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___ArrayWithNulls___RoundTrip()
        {
            // Arrange
            var initialValue = new BaseClass[] { new BaseClass(), null, new BaseClass(), null };

            // Act
            var data = SerializationUnit.Serialize<BaseClass[]>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass[]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___MixedObjectArray___RoundTrip()
        {
            // Arrange
            var initialValue = new object[] { 3, 4.1f, 5, new BaseClass(), "hello" };

            // Act
            var data = SerializationUnit.Serialize<object[]>( initialValue );
            var finalValue = SerializationUnit.Deserialize<object[]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }
    }
}