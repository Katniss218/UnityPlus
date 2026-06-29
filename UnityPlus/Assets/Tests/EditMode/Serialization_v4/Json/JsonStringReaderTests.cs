using NUnit.Framework;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace Serialization.Json
{
    public class JsonStringReaderTests
    {
        [Test]
        public void Parse___Complicated___ParsesCorrectly()
        {
            // Arrange 
            string json = @"{
    ""Image"": 
    {
        ""Width"": 800,
        ""Height"": 600,
        ""Title"": ""View from 15th Floor"",
        ""Author"": null,
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
            SerializedObject val = (SerializedObject)sut.Read();

            // Assert
            Assert.That( (int)val["Image"]["Width"], Is.EqualTo( 800 ) );
            Assert.That( (int)val["Image"]["Height"], Is.EqualTo( 600 ) );
            Assert.That( (string)val["Image"]["Title"], Is.EqualTo( "View from 15th Floor" ) );
            Assert.That( (string)val["Image"]["Thumbnail"]["Url"], Is.EqualTo( "http://www.example.com/image/481989943" ) );
            Assert.That( (int)val["Image"]["Thumbnail"]["Height"], Is.EqualTo( 125 ) );
            Assert.That( (int)val["Image"]["Thumbnail"]["Width"], Is.EqualTo( 100 ) );
            Assert.That( (bool)val["Image"]["Animated"], Is.EqualTo( false ) );
            Assert.That( (object)val["Image"]["Author"], Is.Null );

            Assert.That( (int)((SerializedArray)val["Image"]["IDs"]).Count, Is.EqualTo( 4 ) );
            Assert.That( (int)val["Image"]["IDs"][0], Is.EqualTo( 116 ) );
            Assert.That( (int)val["Image"]["IDs"][1], Is.EqualTo( 943 ) );
            Assert.That( (int)val["Image"]["IDs"][2], Is.EqualTo( 234 ) );
            Assert.That( (int)val["Image"]["IDs"][3], Is.EqualTo( 38793 ) );
        }

        [Test]
        public void Parse___SpecialCharacters___ParsesCorrectly()
        {
            // Arrange
            // Testing: Quotes, Backslashes, Unicode, Tabs/Newlines in string
            string json = @"{
                ""Data"": ""Line1\nLine2\tTabbed"",
                ""Escaped"": ""Say \""Hello\"" now"",
                ""Path"": ""C:\\Windows\\System32"",
                ""Unicode"": ""\u00A9 Copyright""
            }";

            JsonStringReader sut = new JsonStringReader( json );

            // Act
            SerializedObject val = (SerializedObject)sut.Read();

            // Assert
            Assert.That( (string)val["Data"], Is.EqualTo( "Line1\nLine2\tTabbed" ) );
            Assert.That( (string)val["Escaped"], Is.EqualTo( "Say \"Hello\" now" ) );
            Assert.That( (string)val["Path"], Is.EqualTo( "C:\\Windows\\System32" ) );
            Assert.That( (string)val["Unicode"], Is.EqualTo( "© Copyright" ) );
        }

        [Test]
        public void Parse___Numbers___ParsesCorrectly()
        {
            // Arrange
            string json = @"{
                ""Integer"": 12345,
                ""Negative"": -50,
                ""Float"": 123.456,
                ""Scientific"": 1.2e-3,
                ""NegativeFloat"": -0.005
            }";

            JsonStringReader sut = new JsonStringReader( json );

            // Act
            SerializedObject val = (SerializedObject)sut.Read();

            // Assert
            Assert.That( (int)val["Integer"], Is.EqualTo( 12345 ) );
            Assert.That( (int)val["Negative"], Is.EqualTo( -50 ) );
            Assert.That( (double)val["Float"], Is.EqualTo( 123.456 ).Within( 0.0001 ) );
            Assert.That( (double)val["Scientific"], Is.EqualTo( 0.0012 ).Within( 0.0001 ) );
            Assert.That( (double)val["NegativeFloat"], Is.EqualTo( -0.005 ).Within( 0.0001 ) );
        }
    }
}
