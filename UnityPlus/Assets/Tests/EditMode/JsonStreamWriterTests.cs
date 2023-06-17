using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ComponentData;
using UnityPlus.Serialization.Json;
using UnityEngine.TestTools;

namespace Serialization.Json
{
    public class JsonStreamWriterTests
    {
        [Test]
        public void Write_String_Object()
        {
            // Arrange
            SerializedObject json = new SerializedObject()
            {
                { "test1", "hello" },
                { "test2", "hi" },
                { "test3", "hey" }
            };

            using( MemoryStream s = new MemoryStream() )
            {
                // Act
                new JsonStreamWriter(json, s ).Write();

                string str = Encoding.UTF8.GetString( s.ToArray() );
                // Assert
                Assert.That( str, Is.EqualTo( "{\"test1\":\"hello\",\"test2\":\"hi\",\"test3\":\"hey\"}" ) );
            }
        }
        [Test]
        public void Write_String_Object_Escaped()
        {
            // Arrange
            SerializedObject json = new SerializedObject()
            {
                { "test", (SerializedPrimitive)"Hello World \r\n \t \\ Hello / 2" }
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
        [Test]
        public void Write_Multiple_Array()
        {
            // Arrange
            SerializedArray json = new SerializedArray()
            {
                "hello",
                "hi",
                "hey"
            };

            using( MemoryStream s = new MemoryStream() )
            {
                // Act
                new JsonStreamWriter( json, s ).Write();

                string str = Encoding.UTF8.GetString( s.ToArray() );
                // Assert
                Assert.That( str, Is.EqualTo( "[\"hello\",\"hi\",\"hey\"]" ) );
            }
        }

        [Test]
        public void Write_Bool()
        {
            // Arrange
            SerializedPrimitive json1 = (SerializedPrimitive)true;
            SerializedPrimitive json2 = (SerializedPrimitive)false;

            using( MemoryStream s = new MemoryStream() )
            {
                // Act
                new JsonStreamWriter( json1, s ).Write();

                string str = Encoding.UTF8.GetString( s.ToArray() );
                // Assert
                Assert.That( str, Is.EqualTo( "true" ) );
            }
            using( MemoryStream s = new MemoryStream() )
            {
                // Act
                new JsonStreamWriter( json2, s ).Write();

                string str = Encoding.UTF8.GetString( s.ToArray() );
                // Assert
                Assert.That( str, Is.EqualTo( "false" ) );
            }
        }

        [Test]
        public void Write_Number()
        {
            // Arrange
            SerializedPrimitive json = (SerializedPrimitive)(-3.1415f);

            using( MemoryStream s = new MemoryStream() )
            {
                // Act
                new JsonStreamWriter( json, s ).Write();

                string str = Encoding.UTF8.GetString( s.ToArray() );
                // Assert
                Assert.That( str, Does.StartWith( "-3.141" ) ); // float precision
            }
        }
    }
}