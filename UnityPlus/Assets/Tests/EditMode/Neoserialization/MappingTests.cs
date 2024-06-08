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
        public void Mapping___DateTime___RoundTrip()
        {
            // Arrange
            var initialValue = DateTime.UtcNow;

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<DateTime>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___TimeSpan___RoundTrip()
        {
            // Arrange
            var initialValue = TimeSpan.FromTicks( 342534252 );

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<TimeSpan>( data );

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
        public void Mapping___KeyValuePair_OfObjects_ValToVal___RoundTrip()
        {
            // Arrange
            var initialValue = new KeyValuePair<BaseClass, BaseClass>( new BaseClass() { baseMember = 2 }, new DerivedClass() { baseMember = 5, derivedMember = "42" } );

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<KeyValuePair<BaseClass, BaseClass>>( data );

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

        [Test]
        public void Mapping___BaseClass___RoundTrip()
        {
            // Arrange
            var initialValue = new BaseClass() { baseMember = 2 };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___DerivedClass_Polymorphic___RoundTrip()
        {
            // Arrange
            BaseClass initialValue = new DerivedClass() { baseMember = 2, derivedMember = "42" };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

            // Assert
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }

        [Test]
        public void Mapping___Populate_Object()
        {
            // Arrange
            var initialValue = new DerivedClass() { baseMember = 2, derivedMember = "42" };
            var finalValue = new DerivedClass();

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            SerializationUnit.Populate<DerivedClass>( finalValue, data );

            // Assert
            Assert.That( finalValue, Is.SameAs( finalValue ) );
            Assert.That( initialValue, Is.EqualTo( finalValue ) );
        }
    }
}