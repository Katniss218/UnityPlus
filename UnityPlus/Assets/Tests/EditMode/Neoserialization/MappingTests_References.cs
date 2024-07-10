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
    public class MappingTests_References
    {
        [Test]
        public void Mapping___NullReference___RoundTrip()
        {
            // Arrange
            var initialValue = (BaseClass)null;

            // Act
            var data = SerializationUnit.Serialize<BaseClass>( ObjectContext.Ref, initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

            // Assert
            Assert.That( finalValue, Is.Null );
        }

        [Test]
        public void Mapping___ClassReference___RoundTrip()
        {
            // Arrange
            var initialValue = new BaseClass();
            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

            // Act
            var data = SerializationUnit.Serialize<BaseClass>( ObjectContext.Ref, initialValue, refStore );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( ObjectContext.Ref, data, refStore );

            // Assert
            Assert.That( finalValue, Is.SameAs( initialValue ) );
        }
        
        [Test]
        public void Mapping___InterfaceReference___RoundTrip()
        {
            // Arrange
            IAnInterface initialValue = new InterfaceClass();
            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

            // Act
            var data = SerializationUnit.Serialize<IAnInterface>( ObjectContext.Ref, initialValue, refStore );
            var finalValue = SerializationUnit.Deserialize<IAnInterface>( ObjectContext.Ref, data, refStore );

            // Assert
            Assert.That( finalValue, Is.SameAs( initialValue ) );
        }

        [Test]
        public void Mapping___Member_NullReference___RoundTrip()
        {
            // Arrange
            var initialValue = new ReferencingClass()
            {
                refMember = null,
                interfaceRefMember = null
            };

            // Act
            var data = SerializationUnit.Serialize<ReferencingClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<ReferencingClass>( data );

            // Assert
            Assert.That( finalValue.refMember, Is.Null );
        }

        [Test]
        public void Mapping___Member_Reference___RoundTrip()
        {
            // Arrange
            var refValue = new BaseClass();
            var interfaceRefValue = new InterfaceClass();
            var initialValue = new ReferencingClass()
            {
                refMember = refValue,
                interfaceRefMember = interfaceRefValue
            };

            // Act
            // Round-trip the referenced instance and the class that references it.
            var su = SerializationUnit.FromObjects<object>( refValue, interfaceRefValue, initialValue );
            su.Serialize();
            var su2 = SerializationUnit.FromData<object>( su.GetData() );
            su2.Deserialize();

            var finalValue = su2.GetObjectsOfType<ReferencingClass>().First();
            var finalRefValue = su2.GetObjectsOfType<BaseClass>().First();
            var finalInterfaceRefValue = su2.GetObjectsOfType<InterfaceClass>().First();

            // Assert
            Assert.That( finalValue.refMember, Is.SameAs( finalRefValue ) );
            Assert.That( finalValue.interfaceRefMember, Is.SameAs( finalInterfaceRefValue ) );
        }

        [Test]
        public void Mapping___Member_ExistingReference___RoundTrip()
        {
            // Arrange
            var refValue = new BaseClass();
            var interfaceRefValue = new InterfaceClass();
            var initialValue = new ReferencingClass()
            {
                refMember = refValue,
                interfaceRefMember = interfaceRefValue
            };
            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

            // Act
            // Round-trip only the referencing class, making sure it references the same object.
            var su = SerializationUnit.FromObjects<object>( initialValue );
            su.Serialize( refStore );
            var su2 = SerializationUnit.FromData<object>( su.GetData() );
            su2.Deserialize( refStore );

            var finalValue = su2.GetObjectsOfType<ReferencingClass>().First();

            // Assert
            Assert.That( finalValue.refMember, Is.SameAs( refValue ) );
            Assert.That( finalValue.interfaceRefMember, Is.SameAs( interfaceRefValue ) );
        }
    }
}