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
    public class MappingTests_Arrays
    {
        [Test]
        public void Mapping___NullArray___RoundTrip()
        {
            // Arrange
            int[] initialValue = null;

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<int[]>( data );

            // Assert
            Assert.That( finalValue, Is.Null );
        }

        [Test]
        public void Mapping___EmptyArray___RoundTrip()
        {
            // Arrange
            var initialValue = new int[] { };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<int[]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___IntArray___RoundTrip()
        {
            // Arrange
            var initialValue = new int[] { 3, 4, 5, 9, 42, 218 };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<int[]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___ArrayWithNulls___RoundTrip()
        {
            // Arrange
            var initialValue = new BaseClass[] { new BaseClass(), null };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass[]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___MixedObjectArray___RoundTrip()
        {
#warning TODO - the simple types like int, float, etc. don't save the type alongside them. This could be resolved by providing a different mapping and using explicitly-typed context inside mixed arrays (arrays whose element type isn't a struct).
            // this can even be done in a generic way.


            // Arrange
            var initialValue = new object[] { 3, 4.1f, 5, new BaseClass(), "hello" };

            // Act
            var data = SerializationUnit.Serialize( initialValue );
            var finalValue = SerializationUnit.Deserialize<object[]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }
    }
}