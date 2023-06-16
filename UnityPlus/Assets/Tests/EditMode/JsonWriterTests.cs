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
        public void Write_String_Object()
        {
            // Arrange
            SerializedObject json = new SerializedObject()
            {
                { "test", "hello" }
            };

            using( MemoryStream s = new MemoryStream() )
            {
                // Act
                new JsonStreamWriter(json, s ).Write();

                string str = Encoding.UTF8.GetString( s.ToArray() );
                // Assert
                Assert.That( str, Is.EqualTo( "{\"test\":\"hello\"}" ) );
            }
        }
        [Test]
        public void Write_String_Object_Escaped()
        {
            // Arrange
            SerializedObject json = new SerializedObject()
            {
                { "test", "Hello World \r\n \t \\ Hello / 2" }
            };

            using( MemoryStream s = new MemoryStream() )
            {
                // Act
                new JsonStreamWriter(json, s ).Write();

                string str = Encoding.UTF8.GetString( s.ToArray() );
                // Assert
                Assert.That( str, Is.EqualTo( "{\"test\":\"Hello World \\r\\n \\t \\\\ Hello / 2\"}" ) );
            }
        }
    }
}