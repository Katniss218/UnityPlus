using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using static Neoserialization.MappingRegistryTests;

namespace Neoserialization
{
    public class MappingTests
    {
        [Test]
        public void Mapping___Boolean___RoundTrip()
        {
            // Arrange
            var initialValue = true;

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<bool>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___Int32___RoundTrip()
        {
            // Arrange
            int initialValue = -5;

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<int>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___Float___RoundTrip()
        {
            // Arrange
            float initialValue = -5.2342f;

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<float>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___Char___RoundTrip()
        {
            // Arrange
            var initialValue = 'ę';

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<char>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___String___RoundTrip()
        {
            // Arrange
            var initialValue = "test \new \random ayhdsad231";

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<string>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___IntArray___RoundTrip()
        {
            // Arrange
            var initialValue = new int[] { 3, 4, 5, 9, 42, 218 };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<int[]>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___KeyValuePair_ValToVal___RoundTrip()
        {
            // Arrange
            var initialValue = new KeyValuePair<string, int>( "first", 5 );

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<KeyValuePair<string, int>>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___KeyValuePairArray_ValToVal___RoundTrip()
        {
            // Arrange
            var initialValue = new KeyValuePair<string, int>[]
            {
                new KeyValuePair<string, int>( "first", 5 ),
                new KeyValuePair<string, int>( "second", 42 ),
                new KeyValuePair<string, int>( "third", 218 )
            };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<KeyValuePair<string, int>[]>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___Dictionary_ValToVal___RoundTrip()
        {
            // Arrange
            var initialValue = new Dictionary<string, int>()
            {
                { "first", 5 },
                { "second", 42 },
                { "third", 218 }
            };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<Dictionary<string, int>>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___DictionaryOfObjects_ValToVal___RoundTrip()
        {
            // Arrange
            var initialValue = new Dictionary<BaseClass, BaseClass>()
            {
                { new BaseClass() { baseMember = 2 }, new BaseClass() { baseMember = 5 } }
            };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<Dictionary<BaseClass, BaseClass>>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }
    }
}