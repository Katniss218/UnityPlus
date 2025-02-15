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
    public class MappingTests_Objects
    {
        [Test]
        public void Mapping___NullMember___RoundTrip()
        {
            // Arrange
            OwningClass initialValue = new OwningClass() { refMember = null };

            // Act
            var data = SerializationUnit.Serialize<OwningClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<OwningClass>( data );

            // Assert
            Assert.That( finalValue.refMember, Is.Null );
        }
    }
}