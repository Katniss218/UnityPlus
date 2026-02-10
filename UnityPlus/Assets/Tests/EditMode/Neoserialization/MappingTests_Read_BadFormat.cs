//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityPlus.Serialization;
//using UnityPlus.Serialization.ReferenceMaps;

//namespace Neoserialization
//{
//    public class MappingTests_Read_BadFormat
//    {
//        [Test]
//        public void Reading___MissingTypeHeader___()
//        {
//#warning TODO - Try to read a type with/without header and the data has the opposite. Deserialization should result in failures until the member is eventually left at what it was.

//        }
        
//        [Test]
//        public void Reading___MissingIDHeader___CreatedButNotAddedToTheRefStore()
//        {
//            var initialValue = new SerializedObject()
//            {
//                { "$type", "Neoserialization.OwningClass, EditMode, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
//                //{ "$id", "1cfbb788-5602-4b03-bc62-39e1ecb0e6b6" },
//                { "ref_member", (SerializedPrimitive)null }
//            };

//            var refMap = new BidirectionalReferenceStore();

//            var obj = SerializationUnit.Deserialize<OwningClass>( ObjectContext.Value, initialValue, refMap );
//            bool isInRefmap = refMap.TryGetID( obj, out var guid );

//            Assert.That( obj, Is.EqualTo( new OwningClass() ) );
//            Assert.That( isInRefmap, Is.False );
//        }

//        [Test]
//        public void Reading___MissingMember___LeftAsDefault()
//        {
//            var initialValue = new SerializedObject()
//            {
//                { "$type", "Neoserialization.OwningClass, EditMode, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" }
//            };

//            var refMap = new BidirectionalReferenceStore();

//            var obj = SerializationUnit.Deserialize<OwningClass>( ObjectContext.Value, initialValue, refMap );

//            Assert.That( obj, Is.EqualTo( new OwningClass() ) );
//            Assert.That( obj.refMember, Is.Null );
//        }

//        [Test]
//        public void Reading___ArrayInsteadOfObject___()
//        {
//#warning TODO - maybe handle this with the 'errors', add an error, map it to a hierarchy of guids that can be followed to get to the gameobject(s) to delete them.
//            // Arrange
//            var initialValue = new SerializedObject()
//            {
//                { "$type", "Neoserialization.OwningClass, EditMode, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
//                { "$id", "1cfbb788-5602-4b03-bc62-39e1ecb0e6b6" },
//                { "ref_member", new SerializedArray() {
//                } }
//            };
//            var refMap = new BidirectionalReferenceStore();

//            // Act
//            var obj = SerializationUnit.Deserialize<OwningClass>( ObjectContext.Value, initialValue, refMap );

//            // Assert
//            //Assert.Throws<Exception>( () => SerializationUnit.Deserialize<OwningClass>( ObjectContext.Value, initialValue, refMap ) );
//            Assert.That( obj, Is.EqualTo( new OwningClass() ) );
//        }

//        [Test]
//        public void Mapping___FailedMember___DoesNotCallSetter()
//        {
//            // Arrange
//            var refValue = new BaseClass();
//            var interfaceRefValue = new InterfaceClass();
//            var initialValue = new ReferencingClass()
//            {
//                refMember = refValue,
//                interfaceRefMember = interfaceRefValue
//            };

//            var finalValue = new ReferencingClass()
//            {
//                refMember = refValue, // This would be reset by the setter.
//                interfaceRefMember = interfaceRefValue // This would be reset by the setter.
//            };

//            // Act
//            // Round-trip only the referencing class. Do not pass the ref store so deserialization can't resolve the reference.
//            var su = SerializationUnit.FromObjects<object>( initialValue );
//            su.Serialize();
//            var su2 = SerializationUnit.PopulateObject<object>( finalValue, su.GetData().First() );
//            su2.Populate();

//            // Assert
//            Assert.That( finalValue.refMember, Is.SameAs( refValue ) );
//            Assert.That( finalValue.interfaceRefMember, Is.SameAs( interfaceRefValue ) );
//        }
//    }
//}