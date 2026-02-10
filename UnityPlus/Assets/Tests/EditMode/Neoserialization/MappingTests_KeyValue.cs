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
//    public class MappingTests_KeyValue
//    {
//        [Test]
//        public void Mapping___KeyValuePair_ValToVal___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new KeyValuePair<BaseClass, int>( new BaseClass() { baseMember = 2 }, 5 );

//            // Act
//            var data = SerializationUnit.Serialize<KeyValuePair<BaseClass, int>>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<KeyValuePair<BaseClass, int>>( data );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___KeyValuePair_RefToVal___RoundTrip()
//        {
//            // Arrange
//            var baseClass = new BaseClass();
//            var initialValue = new KeyValuePair<BaseClass, int>( baseClass, 5 );
//            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

//            // Act
//            var data = SerializationUnit.Serialize<KeyValuePair<BaseClass, int>>( KeyValueContext.RefToValue, initialValue, refStore );
//            var finalValue = SerializationUnit.Deserialize<KeyValuePair<BaseClass, int>>( KeyValueContext.RefToValue, data, refStore );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) ); // This doesn't check for reference equality by default.
//            Assert.That( finalValue.Key, Is.SameAs( baseClass ) );
//        }
        
//        [Test]
//        public void Mapping___KeyValuePair_ValToRef___RoundTrip()
//        {
//            // Arrange
//            var baseClass = new BaseClass();
//            var initialValue = new KeyValuePair<string, BaseClass>( "first", baseClass );
//            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

//            // Act
//            var data = SerializationUnit.Serialize<KeyValuePair<string, BaseClass>>( KeyValueContext.ValueToRef, initialValue, refStore );
//            var finalValue = SerializationUnit.Deserialize<KeyValuePair<string, BaseClass>>( KeyValueContext.ValueToRef, data, refStore );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) ); // This doesn't check for reference equality by default.
//            Assert.That( finalValue.Value, Is.SameAs( baseClass ) );
//        }
        
//        [Test]
//        public void Mapping___KeyValuePair_RefToRef___RoundTrip()
//        {
//            // Arrange
//            var o1 = new BaseClass();
//            var o2 = new BaseClass();
//            var initialValue = new KeyValuePair<BaseClass, BaseClass>( o1, o2 );
//            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

//            // Act
//            var data = SerializationUnit.Serialize<KeyValuePair<BaseClass, BaseClass>>( KeyValueContext.RefToRef, initialValue, refStore );
//            var finalValue = SerializationUnit.Deserialize<KeyValuePair<BaseClass, BaseClass>>( KeyValueContext.RefToRef, data, refStore );

//            // Assert
//            Assert.That( finalValue, Is.EqualTo( initialValue ) ); // This doesn't check for reference equality by default.
//            Assert.That( finalValue.Key, Is.SameAs( o1 ) );
//            Assert.That( finalValue.Value, Is.SameAs( o2 ) );
//        }

//        [Test]
//        public void Mapping___Dictionary_ValToVal___RoundTrip()
//        {
//            // Arrange
//            var initialValue = new Dictionary<BaseClass, int>()
//            {
//                { new BaseClass() { baseMember = 2 }, 5 },
//                { new BaseClass() { baseMember = 2 }, 42 },
//                { new BaseClass() { baseMember = 2 }, 218 }
//            };

//            // Act
//            var data = SerializationUnit.Serialize<Dictionary<BaseClass, int>>( initialValue );
//            var finalValue = SerializationUnit.Deserialize<Dictionary<BaseClass, int>>( data );

//            // Assert
//            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
//        }

//        [Test]
//        public void Mapping___Dictionary_RefToVal___RoundTrip()
//        {
//            // Arrange
//            var o1 = new BaseClass();
//            var o2 = new BaseClass();
//            var initialValue = new Dictionary<BaseClass, BaseClass>()
//            {
//                { o1, new DerivedClass() },
//                { o2, new DerivedClass() }
//            };
//            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

//            // Act
//            var data = SerializationUnit.Serialize<Dictionary<BaseClass, BaseClass>>( KeyValueContext.RefToValue, initialValue, refStore );
//            var finalValue = SerializationUnit.Deserialize<Dictionary<BaseClass, BaseClass>>( KeyValueContext.RefToValue, data, refStore );

//            // Assert
//            Assert.That( finalValue, Is.EquivalentTo( initialValue ) ); // This doesn't check for reference equality by default.
//            Assert.That( finalValue.First().Key, Is.SameAs( o1 ) );
//            Assert.That( finalValue.Last().Key, Is.SameAs( o2 ) );
//        }

//        [Test]
//        public void Mapping___Dictionary_ValToRef___RoundTrip()
//        {
//            // Arrange
//            var o1 = new BaseClass();
//            var o2 = new BaseClass();
//            var initialValue = new Dictionary<BaseClass, BaseClass>()
//            {
//                { new DerivedClass(), o1 },
//                { new DerivedClass(), o2 }
//            };
//            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

//            // Act
//            var data = SerializationUnit.Serialize<Dictionary<BaseClass, BaseClass>>( KeyValueContext.ValueToRef, initialValue, refStore );
//            var finalValue = SerializationUnit.Deserialize<Dictionary<BaseClass, BaseClass>>( KeyValueContext.ValueToRef, data, refStore );

//            // Assert
//            Assert.That( finalValue, Is.EquivalentTo( initialValue ) ); // This doesn't check for reference equality by default.
//            Assert.That( finalValue.First().Value, Is.SameAs( o1 ) );
//            Assert.That( finalValue.Last().Value, Is.SameAs( o2 ) );
//        }

//        [Test]
//        public void Mapping___Dictionary_RefToRef___RoundTrip()
//        {
//            // Arrange
//            var o1 = new BaseClass();
//            var o2 = new BaseClass();
//            var o3 = new BaseClass();
//            var o4 = new BaseClass();
//            var initialValue = new Dictionary<BaseClass, BaseClass>()
//            {
//                { o1, o2 },
//                { o3, o4 }
//            };
//            BidirectionalReferenceStore refStore = new BidirectionalReferenceStore();

//            // Act
//            var data = SerializationUnit.Serialize<Dictionary<BaseClass, BaseClass>>( KeyValueContext.RefToRef, initialValue, refStore );
//            var finalValue = SerializationUnit.Deserialize<Dictionary<BaseClass, BaseClass>>( KeyValueContext.RefToRef, data, refStore );

//            Assert.That( finalValue, Is.EquivalentTo( initialValue ) ); // This doesn't check for reference equality by default.
//            Assert.That( finalValue.First().Key, Is.SameAs( o1 ) );
//            Assert.That( finalValue.First().Value, Is.SameAs( o2 ) );
//            Assert.That( finalValue.Last().Key, Is.SameAs( o3 ) );
//            Assert.That( finalValue.Last().Value, Is.SameAs( o4 ) );
//        }
//    }
//}