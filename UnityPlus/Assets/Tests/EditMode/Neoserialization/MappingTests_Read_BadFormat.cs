using NUnit.Framework;
using System;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace Neoserialization
{
    public class MappingTests_Read_BadFormat
    {
        [Test]
        public void Reading___BadType___DeserializedAsDefault()
        {
#warning TODO - Try to read a type with/without header and the data has the opposite. Deserialization should result in failures until the member is eventually left at what it was.

            // Arrange
            var initialValue = new SerializedObject()
            {
                { "$type", "UnityEngine.GameObjectNonExistent, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                { "$id", "1cfbb788-5602-4b03-bc62-39e1ecb0e6b6" },
                { "value", (SerializedPrimitive)true }
            };
            var refMap = new BidirectionalReferenceStore();

            // Act
            var data = SerializationUnit.Deserialize<object>( ObjectContext.Value, initialValue, refMap );

            // Assert
            Assert.That( data, Is.EqualTo( default( object ) ) );
        }
    }
}