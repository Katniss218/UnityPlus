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
    public class ReferencingClass
    {
        public BaseClass refMember;
    }

    public class MappingTests_References
    {
        [SerializationMappingProvider( typeof( ReferencingClass ) )]
        public static SerializationMapping ReferencingClassMapping()
        {
            return new MemberwiseSerializationMapping<ReferencingClass>()
            {
                ("ref_member", new Member<ReferencingClass, BaseClass>( ObjectContext.Ref, o => o.refMember ))
            };
        }

        [Test]
        public void Mapping___NullReference___RoundTrip()
        {
            // Arrange
            var initialValue = new ReferencingClass()
            {
                refMember = null
            };

            // Act
            var su = SerializationUnit.FromObjects<object>( initialValue );
            su.Serialize();
            var su2 = SerializationUnit.FromData<object>( su.GetData() );
            su2.Deserialize();

            var finalValue = su2.GetObjectsOfType<ReferencingClass>().First();

            // Assert
            Assert.That( finalValue.refMember, Is.SameAs( null ) );
        }

        [Test]
        public void Mapping___Reference___RoundTrip()
        {
            // Arrange
            var refValue = new BaseClass();
            var initialValue = new ReferencingClass()
            {
                refMember = refValue
            };

            // Act
            // Round-trip the referenced instance and the class that references it.
            var su = SerializationUnit.FromObjects<object>( refValue, initialValue );
            su.Serialize();
            var su2 = SerializationUnit.FromData<object>( su.GetData() );
            su2.Deserialize();

            var finalValue = su2.GetObjectsOfType<ReferencingClass>().First();
            var baseValue = su2.GetObjectsOfType<BaseClass>().First();

            // Assert
            Assert.That( finalValue.refMember, Is.SameAs( baseValue ) );
        }

        [Test]
        public void Mapping___ExistingReference___RoundTrip()
        {
            // Arrange
            var refValue = new BaseClass();
            var initialValue = new ReferencingClass()
            {
                refMember = refValue
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
        }
    }
}