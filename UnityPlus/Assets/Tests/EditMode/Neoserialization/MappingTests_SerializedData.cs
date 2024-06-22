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
    public class MappingTests_SerializedData
    {
        [Test]
        public void Mapping___SerializedPrimitive___RoundTrip()
        {
#warning SerializedData and derived classes should contain the $id, and $type keys, as any other class (they *can* be referenced)
            // Arrange
            var initialValue = (SerializedPrimitive)5.4112f;

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<SerializedData>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___SerializedArray___RoundTrip()
        {
            // Arrange
            var initialValue = new SerializedArray()
            { 
                (SerializedPrimitive)5.4112f,
                (SerializedPrimitive)"hello world"
            };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<SerializedData>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }
        [Test]
        public void Mapping___SerializedObject___RoundTrip()
        {
            // Arrange
            var initialValue = new SerializedObject()
            {
                { "member1", (SerializedPrimitive)5.4112f },
                { "member1", (SerializedPrimitive)"hello world" }
            };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<SerializedData>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }
    }
}