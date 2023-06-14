using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Serialization.ComponentData;
using UnityEngine.Serialization.Json;
using UnityEngine.TestTools;

namespace Serialization
{
    public class SerializedDataTests
    {
        [Test]
        public void SerializedValues_CompareByValue()
        {
            // Arrange
            var val1 = new SerializedValue( null );
            var val2 = new SerializedValue( null );

            var val3 = new SerializedValue( true );
            var val4 = new SerializedValue( false );

            // Act

            // Assert
            Assert.That( val1, Is.EqualTo( val2 ) );
            Assert.That( val3, Is.Not.EqualTo( val4 ) );
        }
    }
}