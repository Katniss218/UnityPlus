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
    public class MappingTests_Populate
    {
        [Test]
        public void Mapping___Populate_Object()
        {
            // Arrange
            var initialValue = new DerivedClass() { baseMember = 2, derivedMember = "42" };
            var finalValue = new DerivedClass();

            // Act
            var data = SerializationUnit.Serialize<DerivedClass>( initialValue );
            SerializationUnit.Populate<DerivedClass>( finalValue, data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___Populate_Struct()
        {
            // Arrange
            var initialValue = new Vector3();
            var finalValue = new Vector3();

            // Act
            var data = SerializationUnit.Serialize<Vector3>( initialValue );
            SerializationUnit.Populate<Vector3>( ref finalValue, data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }
    }
}