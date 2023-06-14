using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Serialization.ComponentData;
using UnityEngine.Serialization.Json;
using UnityEngine.TestTools;

namespace Serialization.Json
{
    public class JsonReaderTests
    {
        [Test]
        public void EatValue_True_ParseCorrectly()
        {
            // Arrange
            string json = "true";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedValue val = sut.EatValue();

            // Assert
            Assert.That( val, Is.EqualTo( new SerializedValue( true ) ) );
        }

        [Test]
        public void EatValue_False_ParseCorrectly()
        {
            // Arrange
            string json = "false";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedValue val = sut.EatValue();

            // Assert
            Assert.That( val, Is.EqualTo( new SerializedValue( false ) ) );
        }

        [Test]
        public void EatValue_Null_ParseCorrectly()
        {
            // Arrange
            string json = "null";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedValue val = sut.EatValue();

            // Assert
            Assert.That( val, Is.EqualTo( null ) );
        }
    }
}