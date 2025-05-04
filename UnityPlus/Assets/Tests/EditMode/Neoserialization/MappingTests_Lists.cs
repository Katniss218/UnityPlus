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
    public class MappingTests_Lists
    {
        [Test]
        public void Mapping___NullList___RoundTrip()
        {
            // Arrange
            List<int> initialValue = null;

            // Act
            var data = SerializationUnit.Serialize<List<int>>( initialValue );
            var finalValue = SerializationUnit.Deserialize<List<int>>( data );

            // Assert
            Assert.That( finalValue, Is.Null );
        }

        [Test]
        public void Mapping___EmptyList___RoundTrip()
        {
            // Arrange
            List<int> initialValue = new();

            // Act
            var data = SerializationUnit.Serialize<List<int>>( initialValue );
            var finalValue = SerializationUnit.Deserialize<List<int>>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___IntList___RoundTrip()
        {
            // Arrange
            var initialValue = new List<int>() { 3, 4, 5, 9, 42, 218 };

            // Act
            var data = SerializationUnit.Serialize<List<int>>( initialValue );
            var finalValue = SerializationUnit.Deserialize<List<int>>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }
    }
}