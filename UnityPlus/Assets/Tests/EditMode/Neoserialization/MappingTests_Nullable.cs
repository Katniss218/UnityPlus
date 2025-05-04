using NUnit.Framework;
using UnityEngine;
using UnityPlus.Serialization;

namespace Neoserialization
{
    public class MappingTests_Nullable
    {
        [Test]
        public void Mapping___Nullable_Int_Value___RoundTrip()
        {
            // Arrange
            int? initialValue = 5;

            // Act
            var data = SerializationUnit.Serialize<int?>( initialValue );
            var finalValue = SerializationUnit.Deserialize<int?>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
            Assert.That( finalValue.HasValue, Is.True );
        }

        [Test]
        public void Mapping___Nullable_Int_Null___RoundTrip()
        {
            // Arrange
            int? initialValue = null;

            // Act
            var data = SerializationUnit.Serialize<int?>( initialValue );
            var finalValue = SerializationUnit.Deserialize<int?>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
            Assert.That( finalValue.HasValue, Is.False );
        }

        [Test]
        public void Mapping___Nullable_Vector2_Value___RoundTrip()
        {
            // Arrange
            Vector2? initialValue = new Vector2( 5.4f, -2 );

            // Act
            var data = SerializationUnit.Serialize<Vector2?>( initialValue );
            var finalValue = SerializationUnit.Deserialize<Vector2?>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
            Assert.That( finalValue.HasValue, Is.True );
        }

        [Test]
        public void Mapping___Nullable_Vector2_Null___RoundTrip()
        {
            // Arrange
            Vector2? initialValue = null;

            // Act
            var data = SerializationUnit.Serialize<Vector2?>( initialValue );
            var finalValue = SerializationUnit.Deserialize<Vector2?>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
            Assert.That( finalValue.HasValue, Is.False );
        }

#warning TODO - needs a test for a retry case.

        [Test]
        public void Mapping___Nullable_Null___Format_LikeNullObject()
        {
            // Arrange
            Vector2? initialValue = null;
            BaseClass initialValue2 = null;

            // Act
            var data = SerializationUnit.Serialize<Vector2?>( initialValue );
            var data2 = SerializationUnit.Serialize<BaseClass>( initialValue2 );

            // Assert
            Assert.That( data, Is.EqualTo( data2 ) );
        }
    }
}