using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Serialization.ComponentData;
using UnityEngine.Serialization.Json;
using UnityEngine.TestTools;

namespace Serialization.Json
{
    public class JsonWriterTests
    {
        [Test]
        public void EatValue___True___ParsesCorrectly()
        {
            // Arrange
            SerializedObject json = new SerializedObject()
            {
                { "test", "hello" }
            };

            using( MemoryStream s = new MemoryStream() )
            {
                // Act
                json.WriteJson( s );

                string str = Encoding.UTF8.GetString( s.ToArray() );
                // Assert
                Assert.That( str, Is.EqualTo( "{\"test\":\"hello\"}" ) );
            }
        }
    }
}