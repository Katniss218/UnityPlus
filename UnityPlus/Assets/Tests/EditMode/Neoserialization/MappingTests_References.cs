//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityPlus.Serialization;
//using UnityPlus.Serialization.Json;
//using UnityPlus.Serialization.ReferenceMaps;

//namespace Neoserialization
//{
//    public class MappingTests_References
//    {
//        [Test]
//        public void Mapping___NullReference___RoundTrip()
//        {
//            // Arrange
//            var initialValue = (BaseClass)null;

//            // Act
//            var data = SerializationUnit.Serialize<BaseClass>( ObjectContext.Ref, initialValue );
//            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

//            // Assert
//            Assert.That( finalValue, Is.Null );
//        }

//        [Test]
//        public void Mapping___InterfaceNullReference___RoundTrip()
//        {
//            // Arrange
//            var initialValue = (IAnInterface)null;

//            // Act
//            var data = SerializationUnit.Serialize<IAnInterface>( ObjectContext.Ref, initialValue );
//            var finalValue = SerializationUnit.Deserialize<IAnInterface>( data );

//            // Assert
//            Assert.That( finalValue, Is.Null );
//        }

//        [Test]
//        public void Mapping___ClassReference___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new BaseClass();
//            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

//            // Act
//            var data = SerializationUnit.Serialize<BaseClass>( ObjectContext.Ref, initialValue, refStore );
//            var finalValue = SerializationUnit.Deserialize<BaseClass>( ObjectContext.Ref, data, refStore );

//            // Assert
//            Assert.That( finalValue, Is.SameAs( initialValue ) );
//        }

//        [Test]
//        public void Mapping___InterfaceReference___RoundTrip()
//        {
//            // Arrange
//            IAnInterface initialValue = new InterfaceClass();
//            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

//            // Act
//            var data = SerializationUnit.Serialize<IAnInterface>( ObjectContext.Ref, initialValue, refStore );
//            var finalValue = SerializationUnit.Deserialize<IAnInterface>( ObjectContext.Ref, data, refStore );

//            // Assert
//            Assert.That( finalValue, Is.SameAs( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Member_NullReference___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new ReferencingClass()
//            {
//                refMember = null,
//                interfaceRefMember = null
//            };

//            // Act
//            var data = SerializationUnit.Serialize<ReferencingClass>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<ReferencingClass>( data );

//            // Assert
//            Assert.That( finalValue.refMember, Is.Null );
//        }

//        [Test]
//        public void Mapping___Member_Reference___RoundTrip()
//        {
//            // Arrange
//            var refValue = new BaseClass();
//            var interfaceRefValue = new InterfaceClass();
//            var initialValue = new ReferencingClass()
//            {
//                refMember = refValue,
//                interfaceRefMember = interfaceRefValue
//            };

//            // Act
//            // Round-trip the referenced instance and the class that references it.
//            var su = SerializationUnit.FromObjects<object>( refValue, initialValue, interfaceRefValue );
//            su.Serialize();
//            var su2 = SerializationUnit.FromData<object>( su.GetData() );
//            su2.Deserialize();

//            var finalValue = su2.GetObjects<ReferencingClass>().First();
//            var finalRefValue = su2.GetObjects<BaseClass>().First();
//            var finalInterfaceRefValue = su2.GetObjects<InterfaceClass>().First();

//            // Assert
//            Assert.That( finalValue.refMember, Is.SameAs( finalRefValue ) );
//            Assert.That( finalValue.interfaceRefMember, Is.SameAs( finalInterfaceRefValue ) );
//        }

//        [Test]
//        public void Mapping___Member_ExistingReference___RoundTrip()
//        {
//            // Arrange
//            var refValue = new BaseClass();
//            var interfaceRefValue = new InterfaceClass();
//            var initialValue = new ReferencingClass()
//            {
//                refMember = refValue,
//                interfaceRefMember = interfaceRefValue
//            };
//            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

//            // Act
//            // Round-trip only the referencing class, making sure it references the same object.
//            var su = SerializationUnit.FromObjects<object>( initialValue );
//            su.Serialize( refStore );
//            var su2 = SerializationUnit.FromData<object>( su.GetData() );
//            su2.Deserialize( refStore );

//            var finalValue = su2.GetObjects<ReferencingClass>().First();

//            // Assert
//            Assert.That( finalValue.refMember, Is.SameAs( refValue ) );
//            Assert.That( finalValue.interfaceRefMember, Is.SameAs( interfaceRefValue ) );
//        }

//        [Test]
//        public void Mapping___Array_Reference___RoundTrip()
//        {
//            // Arrange
//            var refValue = new BaseClass();
//            var interfaceRefValue = new InterfaceClass();
//            var initialValue = new ArrayReferenceClass()
//            {
//                refs = new object[]
//                {
//                    refValue,
//                    refValue,
//                    interfaceRefValue,
//                    interfaceRefValue
//                }
//            };

//            // Act
//            // Round-trip the referenced instance and the class that references it.
//            var su = SerializationUnit.FromObjects<object>( refValue, initialValue, interfaceRefValue );
//            su.Serialize();
//            var su2 = SerializationUnit.FromData<object>( su.GetData() );
//            su2.Deserialize();

//            var finalValue = su2.GetObjects<ArrayReferenceClass>().First();
//            var finalRefValue = su2.GetObjects<BaseClass>().First();
//            var finalInterfaceRefValue = su2.GetObjects<InterfaceClass>().First();

//            // Assert
//            Assert.That( finalValue.refs, Has.Length.EqualTo( 4 ) );
//            Assert.That( finalValue.refs[0], Is.SameAs( finalRefValue ) );
//            Assert.That( finalValue.refs[1], Is.SameAs( finalRefValue ) );
//            Assert.That( finalValue.refs[2], Is.SameAs( finalInterfaceRefValue ) );
//            Assert.That( finalValue.refs[3], Is.SameAs( finalInterfaceRefValue ) );
//        }

//        [Test]
//        public void Mapping___List_Reference___RoundTrip()
//        {
//            // Arrange
//            var refValue = new BaseClass();
//            var interfaceRefValue = new InterfaceClass();
//            var initialValue = new ListReferenceClass()
//            {
//                refs = new()
//                {
//                    refValue,
//                    refValue,
//                    interfaceRefValue,
//                    interfaceRefValue
//                }
//            };

//            // Act
//            // Round-trip the referenced instance and the class that references it.
//            var su = SerializationUnit.FromObjects<object>( refValue, initialValue, interfaceRefValue );
//            su.Serialize();
//            var su2 = SerializationUnit.FromData<object>( su.GetData() );
//            su2.Deserialize();

//            var finalValue = su2.GetObjects<ListReferenceClass>().First();
//            var finalRefValue = su2.GetObjects<BaseClass>().First();
//            var finalInterfaceRefValue = su2.GetObjects<InterfaceClass>().First();

//            // Assert
//            Assert.That( finalValue.refs, Has.Count.EqualTo( 4 ) );
//            Assert.That( finalValue.refs[0], Is.SameAs( finalRefValue ) );
//            Assert.That( finalValue.refs[1], Is.SameAs( finalRefValue ) );
//            Assert.That( finalValue.refs[2], Is.SameAs( finalInterfaceRefValue ) );
//            Assert.That( finalValue.refs[3], Is.SameAs( finalInterfaceRefValue ) );
//        }

//        [Test]
//        public void Mapping___Dictionary_Reference___RoundTrip()
//        {
//            // Arrange
//            var refValue = new BaseClass();
//            var interfaceRefValue = new InterfaceClass();
//            var initialValue = new DictionaryReferenceClass()
//            {
//                refs = new Dictionary<object, object>()
//                {
//                    { refValue, interfaceRefValue },
//                    { interfaceRefValue, refValue }
//                }
//            };

//            // Act
//            // Round-trip the referenced instance and the class that references it.
//            var su = SerializationUnit.FromObjects<object>( refValue, initialValue, interfaceRefValue );
//            su.Serialize();
//            var su2 = SerializationUnit.FromData<object>( su.GetData() );
//            su2.Deserialize();

//            var finalValue = su2.GetObjects<DictionaryReferenceClass>().First();
//            var finalRefValue = su2.GetObjects<BaseClass>().First();
//            var finalInterfaceRefValue = su2.GetObjects<InterfaceClass>().First();

//            // Assert
//            Assert.That( finalValue.refs.First().Key, Is.SameAs( finalRefValue ) );
//            Assert.That( finalValue.refs.First().Value, Is.SameAs( finalInterfaceRefValue ) );
//            Assert.That( finalValue.refs.Last().Key, Is.SameAs( finalInterfaceRefValue ) );
//            Assert.That( finalValue.refs.Last().Value, Is.SameAs( finalRefValue ) );
//        }
//    }
//}