using NUnit.Framework;
using System;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace Neoserialization
{
    public class MappingTests_Read_BadFormat
    {
        [Test]
        public void Reading___MissingTypeHeader___()
        {
#warning TODO - Try to read a type with/without header and the data has the opposite. Deserialization should result in failures until the member is eventually left at what it was.

        }
        
        [Test]
        public void Reading___MissingIDHeader___CreatedButNotAddedToTheRefStore()
        {
            var initialValue = new SerializedObject()
            {
                { "$type", "Neoserialization.OwningClass, EditMode, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                //{ "$id", "1cfbb788-5602-4b03-bc62-39e1ecb0e6b6" },
                { "ref_member", (SerializedPrimitive)null }
            };

            var refMap = new BidirectionalReferenceStore();

            var obj = SerializationUnit.Deserialize<OwningClass>( ObjectContext.Value, initialValue, refMap );
            bool isInRefmap = refMap.TryGetID( obj, out var guid );

            Assert.That( obj, Is.EqualTo( new OwningClass() ) );
            Assert.That( isInRefmap, Is.False );
        }

        [Test]
        public void Reading___MissingMember___LeftAsDefault()
        {
            var initialValue = new SerializedObject()
            {
                { "$type", "Neoserialization.OwningClass, EditMode, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" }
            };

            var refMap = new BidirectionalReferenceStore();

            var obj = SerializationUnit.Deserialize<OwningClass>( ObjectContext.Value, initialValue, refMap );

            Assert.That( obj, Is.EqualTo( new OwningClass() ) );
            Assert.That( obj.refMember, Is.Null );
        }

        [Test]
        public void Reading___ArrayInsteadOfObject___()
        {
#warning TODO - maybe handle this with the 'errors', add an error, map it to a hierarchy of guids that can be followed to get to the gameobject(s) to delete them.
            // Arrange
            var initialValue = new SerializedObject()
            {
                { "$type", "Neoserialization.OwningClass, EditMode, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                { "$id", "1cfbb788-5602-4b03-bc62-39e1ecb0e6b6" },
                { "ref_member", new SerializedArray() {
                } }
            };
            var refMap = new BidirectionalReferenceStore();

            // Act
            var obj = SerializationUnit.Deserialize<OwningClass>( ObjectContext.Value, initialValue, refMap );

            // Assert
            //Assert.Throws<Exception>( () => SerializationUnit.Deserialize<OwningClass>( ObjectContext.Value, initialValue, refMap ) );
            Assert.That( obj, Is.EqualTo( new OwningClass() ) );
        }
    }
}