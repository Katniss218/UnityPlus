using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using UnityEngine.TestTools;

namespace Serialization.Json
{
    public class JsonStringWriterTests
    {
        [Test]
        public void Write_Multiple_Object()
        {
            // Arrange
            SerializedObject json = new SerializedObject()
            {
                { "test1", "hello" },
                { "test2", "hi" },
                { "test3", "hey" }
            };
            StringBuilder sb = new StringBuilder();

            // Act
            new JsonStringWriter( json, sb ).Write();
            string str = sb.ToString();

            // Assert
            Assert.That( str, Is.EqualTo( "{\"test1\":\"hello\",\"test2\":\"hi\",\"test3\":\"hey\"}" ) );
        }

        [Test]
        public void Write_String_Object_Escaped()
        {
            // Arrange
            SerializedObject json = new SerializedObject()
            {
                { "test", "Hello World \r\n \t \\ Hello / 2" }
            };
            StringBuilder sb = new StringBuilder();

            // Act
            new JsonStringWriter( json, sb ).Write();
            string str = sb.ToString();

            // Assert
            Assert.That( str, Is.EqualTo( "{\"test\":\"Hello World \\r\\n \\t \\\\ Hello / 2\"}" ) );
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
            StringBuilder sb = new StringBuilder();

            // Act
            new JsonStringWriter( json, sb ).Write();
            string str = sb.ToString();

            // Assert
            Assert.That( str, Is.EqualTo( "[\"hello\",\"hi\",\"hey\"]" ) );
        }

        [Test]
        public void Write_Bool()
        {
            // Arrange
            SerializedPrimitive json1 = (SerializedPrimitive)true;
            SerializedPrimitive json2 = (SerializedPrimitive)false;

            // Act
            StringBuilder sb1 = new StringBuilder();
            new JsonStringWriter( json1, sb1 ).Write();
            string str1 = sb1.ToString();

            StringBuilder sb2 = new StringBuilder();
            new JsonStringWriter( json2, sb2 ).Write();
            string str2 = sb2.ToString();

            // Assert
            Assert.That( str1, Is.EqualTo( "true" ) );
            Assert.That( str2, Is.EqualTo( "false" ) );
        }

        [Test]
        public void Write_Number()
        {
            // Arrange
            SerializedPrimitive json = (SerializedPrimitive)(-3.1415f);
            StringBuilder sb = new StringBuilder();

            // Act
            new JsonStringWriter( json, sb ).Write();
            string str = sb.ToString();

            // Assert
            Assert.That( str, Does.StartWith( "-3.141" ) ); // float precision
        }
    }
}