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
        public void Mapping___BaseClass___RoundTrip()
        {
            // Arrange
            var initialValue = new BaseClass() { baseMember = 2 };

            // Act
            var data = SerializationUnit.Serialize<BaseClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___DerivedClass_Polymorphic___RoundTrip()
        {
            // Arrange
            BaseClass initialValue = new DerivedClass() { baseMember = 2, derivedMember = "42" };

            // Act
            var data = SerializationUnit.Serialize<BaseClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___Interface___RoundTrip()
        {
            // Arrange
            IAnInterface initialValue = new InterfaceClass() { interfaceMember = 2.4f };

            // Act
            var data = SerializationUnit.Serialize<IAnInterface>( initialValue );
            var finalValue = SerializationUnit.Deserialize<IAnInterface>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }
        
        [Test]
        public void Mapping___Null_Class___RoundTrip()
        {
            // Arrange
            BaseClass initialValue = null;

            // Act
            var data = SerializationUnit.Serialize<BaseClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

            // Assert
            Assert.That( finalValue, Is.Null );
        }

        [Test]
        public void Mapping___Null_Interface___RoundTrip()
        {
            // Arrange
            IAnInterface initialValue = null;

            // Act
            var data = SerializationUnit.Serialize<IAnInterface>( initialValue );
            var finalValue = SerializationUnit.Deserialize<IAnInterface>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

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

        [Test]
        public void Mapping___Sealed_Then_Unsealed___DoesntRoundTrip()
        {
            // Arrange
            var initialValue = new SerializedArray()
            {
                (SerializedPrimitive)5.4112f,
                (SerializedPrimitive)"hello world"
            };

            // Act
            var data = SerializationUnit.Serialize<SerializedArray>( initialValue ); // Note different <TMember> between those 2 lines.
            var finalValue = SerializationUnit.Deserialize<SerializedData>( data );

            // By the specifics of how serialization should be performed, we can assume that the TMember type is consistent across serialization and deserialization.

            // Assert
            Assert.That( finalValue, Is.Null );
        }
    }
}