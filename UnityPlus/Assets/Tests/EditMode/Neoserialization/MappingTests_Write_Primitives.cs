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
    public class MappingTests_Write_Primitives
    {
        [Test]
        public void Writing___Bool___CorrectLayout()
        {
            // Arrange
            var initialValue = true;

            // Act
            var data = SerializationUnit.Serialize<bool>( ObjectContext.Value, initialValue );

            // Assert
            Assert.That( data, Is.EqualTo(
                (SerializedPrimitive)true
            ) );
        }

        [Test]
        public void Writing___Bool_Boxed___CorrectLayout()
        {
            // Arrange
            var initialValue = true;
            var refMap = new BidirectionalReferenceStore();

            // Act
            var data = SerializationUnit.Serialize<object>( ObjectContext.Value, initialValue, refMap );

            // Assert
            Assert.That( data, Is.EqualTo(
                new SerializedObject()
                {
                    { "$type", initialValue.GetType().SerializeType() },
                    { "$id", refMap.GetID( initialValue ).SerializeGuid() },
                    { "value", (SerializedPrimitive)true }
                }
            ) );
        }
    }
}