using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using UnityEngine.TestTools;

namespace Serialization.Json
{
    public class JsonStringReaderTests
    {
        [Test]
        public void EatValue___True___ParsesCorrectly()
        {
            // Arrange
            string json = "true";
            JsonStringReader sut = new JsonStringReader( json );

            // Act
            SerializedData val = sut.EatValue();

            // Assert
            Assert.That( val, Is.EqualTo( (SerializedData)true ) );
        }

        [Test]
        public void EatValue___False___ParsesCorrectly()
        {
            // Arrange
            string json = "false";
            JsonStringReader sut = new JsonStringReader( json );

            // Act
            SerializedData val = sut.EatValue();

            // Assert
            Assert.That( val, Is.EqualTo( (SerializedData)false ) );
        }

        [Test]
        public void EatValue___Null___ParsesCorrectly()
        {
            // Arrange
            string json = "null";
            JsonStringReader sut = new JsonStringReader( json );

            // Act
            SerializedData val = sut.EatValue();

            // Assert
            Assert.That( val, Is.EqualTo( (SerializedData)null ) );
        }

        [Test]
        public void EatString___Easy___ParsesCorrectly()
        {
            // Arrange
            string json = "\"Hello World!\"";
            JsonStringReader sut = new JsonStringReader( json );

            // Act
            string val = sut.EatString();

            // Assert
            Assert.That( val, Is.EqualTo( "Hello World!" ) );
        }

        [Test]
        public void EatString___Escaped___ParsesCorrectly()
        {
            // Arrange
            string json = "\"Hello World \\r\\n \\t \\\\ Hello / \\u0032\"";
            JsonStringReader sut = new JsonStringReader( json );

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
            JsonStringReader sut = new JsonStringReader( json );

            // Act
            SerializedObject val = (SerializedObject)sut.Parse();

            // Assert
            Assert.That( (int)val["Image"]["Width"], Is.EqualTo( 800 ) );
            Assert.That( (int)val["Image"]["Height"], Is.EqualTo( 600 ) );
            Assert.That( (string)val["Image"]["Title"], Is.EqualTo( "View from 15th Floor" ) );
            Assert.That( (string)val["Image"]["Thumbnail"]["Url"], Is.EqualTo( "http://www.example.com/image/481989943" ) );
            Assert.That( (int)val["Image"]["Thumbnail"]["Height"], Is.EqualTo( 125 ) );
            Assert.That( (int)val["Image"]["Thumbnail"]["Width"], Is.EqualTo( 100 ) );
            Assert.That( (bool)val["Image"]["Animated"], Is.EqualTo( false ) );

            Assert.That( (int)((SerializedArray)val["Image"]["IDs"]).Count, Is.EqualTo( 4 ) );
            Assert.That( (int)val["Image"]["IDs"][0], Is.EqualTo( 116 ) );
            Assert.That( (int)val["Image"]["IDs"][1], Is.EqualTo( 943 ) );
            Assert.That( (int)val["Image"]["IDs"][2], Is.EqualTo( 234 ) );
            Assert.That( (int)val["Image"]["IDs"][3], Is.EqualTo( 38793 ) );
        }

        [Test]
        public void EatNumber___Easy___ParsesCorrectly()
        {
            // Arrange
            string json = "218";
            JsonStringReader sut = new JsonStringReader( json );

            // Act
            SerializedPrimitive val = sut.EatNumber();

            // Assert
            Assert.That( val, Is.EqualTo( (SerializedPrimitive)218 ) );
        }

        [Test]
        public void EatNumber___Hard___ParsesCorrectly()
        {
            // Arrange
            string json = "-3.1415E+11";
            JsonStringReader sut = new JsonStringReader( json );

            // Act
            SerializedPrimitive val = sut.EatNumber();

            // Assert
            Assert.That( (float)val, Is.EqualTo( -3.1415E+11f ) ); // not ideal because depends on the round-tripping in SerializedValue working correctly.
        }

        [Test]
        public void EatArray___Numbers_Correct___ParsesCorrectly()
        {
            // Arrange
            string json = "[ 1, 2.0, 3 ]";
            JsonStringReader sut = new JsonStringReader( json );

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
            JsonStringReader sut = new JsonStringReader( json );

            // Act
            // Assert
            Assert.That( sut.EatArray, Throws.Exception );
        }

        [Test]
        public void EatArray___Strings_Missing_Comma___ThrowsError()
        {
            // Arrange
            string json = "[ \"hello\", \"hi\" \"hey\" ]";
            JsonStringReader sut = new JsonStringReader( json );

            // Act
            // Assert
            Assert.That( sut.EatArray, Throws.Exception );
        }

        [Test]
        public void EatObject___Correct___ParsesCorrectly()
        {
            // Arrange
            string json = "{ \"name\": \"value\", \"name2\": 218 }";
            JsonStringReader sut = new JsonStringReader( json );

            // Act
            SerializedObject val = sut.EatObject();

            // Assert
            Assert.That( val.Count, Is.EqualTo( 2 ) );
            Assert.That( (string)val["name"], Is.EqualTo( "value" ) );
            Assert.That( (int)val["name2"], Is.EqualTo( 218 ) );
        }
    }
}