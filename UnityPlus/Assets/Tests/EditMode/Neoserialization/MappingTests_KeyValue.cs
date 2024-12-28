using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace Neoserialization
{
    public class MappingTests_KeyValue
    {
        [Test]
        public void Mapping___KeyValuePair_ValToVal___RoundTrip()
        {
            // Arrange
            var initialValue = new KeyValuePair<string, int>( "first", 5 );

            // Act
            var data = SerializationUnit.Serialize<KeyValuePair<string, int>>( initialValue );
            var finalValue = SerializationUnit.Deserialize<KeyValuePair<string, int>>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
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
            var data = SerializationUnit.Serialize<KeyValuePair<string, int>[]>( initialValue );
            var finalValue = SerializationUnit.Deserialize<KeyValuePair<string, int>[]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___KeyValuePair_OfObjects_ValToVal___RoundTrip()
        {
            // Arrange
            var initialValue = new KeyValuePair<BaseClass, BaseClass>( new BaseClass() { baseMember = 2 }, new DerivedClass() { baseMember = 5, derivedMember = "42" } );

            // Act
            var data = SerializationUnit.Serialize<KeyValuePair<BaseClass, BaseClass>>( initialValue );
            var finalValue = SerializationUnit.Deserialize<KeyValuePair<BaseClass, BaseClass>>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
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
            var data = SerializationUnit.Serialize<Dictionary<string, int>>( initialValue );
            var finalValue = SerializationUnit.Deserialize<Dictionary<string, int>>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
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
            var data = SerializationUnit.Serialize<Dictionary<BaseClass, BaseClass>>( initialValue );
            var finalValue = SerializationUnit.Deserialize<Dictionary<BaseClass, BaseClass>>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___DictionaryOfObjects_RefToVal___RoundTrip()
        {
            // Arrange
            var baseClass = new BaseClass();
            var initialValue = new Dictionary<BaseClass, BaseClass>()
            {
                { baseClass, new DerivedClass() }
            };
            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();
#warning TODO - tests to do external and internal refs.
            // Act
            var data = SerializationUnit.Serialize<Dictionary<BaseClass, BaseClass>>( KeyValueContext.RefToValue, initialValue, refStore );
            var finalValue = SerializationUnit.Deserialize<Dictionary<BaseClass, BaseClass>>( KeyValueContext.RefToValue, data, refStore );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }
    }
}