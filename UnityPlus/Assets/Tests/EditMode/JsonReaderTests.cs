using System;
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
        public void EatValue___True___ParsesCorrectly()
        {
            // Arrange
            string json = "true";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedValue val = sut.EatValue();

            // Assert
            Assert.That( val, Is.EqualTo( (SerializedValue)true ) );
        }

        [Test]
        public void EatValue___False___ParsesCorrectly()
        {
            // Arrange
            string json = "false";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedValue val = sut.EatValue();

            // Assert
            Assert.That( val, Is.EqualTo( (SerializedValue)false ) );
        }

        [Test]
        public void EatValue___Null___ParsesCorrectly()
        {
            // Arrange
            string json = "null";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedValue val = sut.EatValue();

            // Assert
            Assert.That( val, Is.EqualTo( (SerializedValue)null ) );
        }

        [Test]
        public void EatString___Easy___ParsesCorrectly()
        {
            // Arrange
            string json = "\"Hello World!\"";
            JsonReader sut = new JsonReader( json );

            // Act
            string val = sut.EatString();

            // Assert
            Assert.That( val, Is.EqualTo( "Hello World!" ) );
        }

        [Test]
        public void EatString___Escaped___ParsesCorrectly()
        {
            // Arrange
            string json = "\"Hello World \\r\\n \\t \\\\ Hello \\/ \\u0032\"";
            JsonReader sut = new JsonReader( json );

            // Act
            string val = sut.EatString();

            // Assert
            Assert.That( val, Is.EqualTo( "Hello World \r\n \t \\ Hello / \u0032" ) );
        }

        [Test]
        public void Parse___Complicated___ParsesCorrectly()
        {
            // Arrange 
            // {"Image":{"Width":800,"Height":600,"Title":"View from 15th Floor","Thumbnail":{"Url":"http://www.example.com/image/481989943","Height":125,"Width":100},"Animated":false,"IDs":[116,943,234,38793]}}
            string json = @"{
    ""Image"": 
    {
        ""Width"": 800,
        ""Height"": 600,
        ""Title"": ""View from 15th Floor"",
        ""Thumbnail"":
        {
            ""Url"": ""http://www.example.com/image/481989943"",
            ""Height"": 125,
            ""Width"": 100
        },
        ""Animated"" : false,
        ""IDs"": [116, 943, 234, 38793]
    }
}";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedObject val = (SerializedObject)sut.Parse();

            // Assert
            Assert.That( (int)((SerializedObject)val["Image"])["Width"], Is.EqualTo( 800 ) );
            Assert.That( (int)((SerializedObject)val["Image"])["Height"], Is.EqualTo( 600 ) );
            Assert.That( (string)((SerializedObject)val["Image"])["Title"], Is.EqualTo( "View from 15th Floor" ) );
            Assert.That( (string)((SerializedObject)((SerializedObject)val["Image"])["Thumbnail"])["Url"], Is.EqualTo( "http://www.example.com/image/481989943" ) );
            Assert.That( (int)((SerializedObject)((SerializedObject)val["Image"])["Thumbnail"])["Height"], Is.EqualTo( 125 ) );
            Assert.That( (int)((SerializedObject)((SerializedObject)val["Image"])["Thumbnail"])["Width"], Is.EqualTo( 100 ) );
            Assert.That( (bool)((SerializedObject)val["Image"])["Animated"], Is.EqualTo( false ) );
            Assert.That( (int)((SerializedArray)((SerializedObject)val["Image"])["IDs"]).Count, Is.EqualTo( 4 ) );
            Assert.That( (int)((SerializedArray)((SerializedObject)val["Image"])["IDs"])[0], Is.EqualTo( 116 ) );
            Assert.That( (int)((SerializedArray)((SerializedObject)val["Image"])["IDs"])[1], Is.EqualTo( 943 ) );
            Assert.That( (int)((SerializedArray)((SerializedObject)val["Image"])["IDs"])[2], Is.EqualTo( 234 ) );
            Assert.That( (int)((SerializedArray)((SerializedObject)val["Image"])["IDs"])[3], Is.EqualTo( 38793 ) );
        }

        [Test]
        public void EatNumber___Easy___ParsesCorrectly()
        {
            // Arrange
            string json = "218";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedValue val = sut.EatNumber();

            // Assert
            Assert.That( val, Is.EqualTo( (SerializedValue)218 ) );
        }

        [Test]
        public void EatNumber___Hard___ParsesCorrectly()
        {
            // Arrange
            string json = "-3.1415E+11";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedValue val = sut.EatNumber();

            // Assert
            Assert.That( (float)val, Is.EqualTo( -3.1415E+11f ) ); // not ideal because depends on the round-tripping in SerializedValue working correctly.
        }

        [Test]
        public void EatArray___Numbers_Correct___ParsesCorrectly()
        {
            // Arrange
            string json = "[ 1, 2.0, 3 ]";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedArray val = sut.EatArray();

            // Assert
            Assert.That( val.Count, Is.EqualTo( 3 ) );
            Assert.That( (int)val[0], Is.EqualTo( 1 ) );
            Assert.That( (float)val[1], Is.EqualTo( 2.0f ) );
            Assert.That( (int)val[2], Is.EqualTo( 3 ) );
        }

        [Test]
        public void EatArray___Numbers_Missing_Comma___ThrowsError()
        {
            // Arrange
            string json = "[ 1, 2.0 3 ]";
            JsonReader sut = new JsonReader( json );

            // Act
            // Assert
            Assert.That( sut.EatArray, Throws.Exception );
        }

        [Test]
        public void EatArray___Strings_Missing_Comma___ThrowsError()
        {
            // Arrange
            string json = "[ \"hello\", \"hi\" \"hey\" ]";
            JsonReader sut = new JsonReader( json );

            // Act
            // Assert
            Assert.That( sut.EatArray, Throws.Exception );
        }

        [Test]
        public void EatObject___Correct___ParsesCorrectly()
        {
            // Arrange
            string json = "{ \"name\": \"value\", \"name2\": 218 }";
            JsonReader sut = new JsonReader( json );

            // Act
            SerializedObject val = sut.EatObject();

            // Assert
            Assert.That( val.Count, Is.EqualTo( 2 ) );
            Assert.That( (string)val["name"], Is.EqualTo( "value" ) );
            Assert.That( (int)val["name2"], Is.EqualTo( 218 ) );
        }
    }
}