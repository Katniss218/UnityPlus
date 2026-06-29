
using System.Text;
using NUnit.Framework;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace Serialization.Json
{
    public class JsonStringWriterTests
    {
        [Test]
        public void Write___ComplicatedObject___WritesCorrectJson()
        {
            // Arrange
            var root = new SerializedObject();
            var image = new SerializedObject();
            root["Image"] = image;

            image["Width"] = (SerializedPrimitive)800;
            image["Height"] = (SerializedPrimitive)600;
            image["Title"] = (SerializedPrimitive)"View from 15th Floor";
            image["Author"] = null; // serialized null
            image["Animated"] = (SerializedPrimitive)false;

            var thumb = new SerializedObject();
            image["Thumbnail"] = thumb;
            thumb["Url"] = (SerializedPrimitive)"http://www.example.com";
            thumb["Height"] = (SerializedPrimitive)125;

            var ids = new SerializedArray();
            image["IDs"] = ids;
            ids.Add( (SerializedPrimitive)116 );
            ids.Add( (SerializedPrimitive)943 );

            StringBuilder sb = new StringBuilder();
            JsonStringWriter sut = new JsonStringWriter( root, sb );

            // Act
            sut.Write();
            string result = sb.ToString();

            // Assert
            // Note: JsonStringWriter does not pretty print, so we check for structural presence.
            Assert.That( result, Contains.Substring( "\"Width\":800" ) );
            Assert.That( result, Contains.Substring( "\"Title\":\"View from 15th Floor\"" ) );
            Assert.That( result, Contains.Substring( "\"Animated\":false" ) );
            Assert.That( result, Contains.Substring( "\"IDs\":[116,943]" ) );
            Assert.That( result, Contains.Substring( "null" ) );
        }

        [Test]
        public void Write___SpecialCharacters___EscapesCorrectly()
        {
            // Arrange
            var root = new SerializedObject();
            root["Path"] = (SerializedPrimitive)"C:\\Test";
            root["Quote"] = (SerializedPrimitive)"She said \"Hi\"";
            root["Newline"] = (SerializedPrimitive)"Line1\nLine2";

            StringBuilder sb = new StringBuilder();
            JsonStringWriter sut = new JsonStringWriter( root, sb );

            // Act
            sut.Write();
            string result = sb.ToString();

            // Assert
            // In JSON string, backslash is doubled (\\) and quote is escaped (\").
            // C# string literal requires doubling backslashes too.
            Assert.That( result, Contains.Substring( "\"Path\":\"C:\\\\Test\"" ) );
            Assert.That( result, Contains.Substring( "\"Quote\":\"She said \\\"Hi\\\"\"" ) );
            Assert.That( result, Contains.Substring( "\"Newline\":\"Line1\\nLine2\"" ) );
        }

        [Test]
        public void Write___Primitives___WritesCorrectly()
        {
            // Arrange
            var arr = new SerializedArray();
            arr.Add( (SerializedPrimitive)true );
            arr.Add( (SerializedPrimitive)false );
            arr.Add( null );
            arr.Add( (SerializedPrimitive)123 );
            arr.Add( (SerializedPrimitive)12.5 );

            StringBuilder sb = new StringBuilder();
            JsonStringWriter sut = new JsonStringWriter( arr, sb );

            // Act
            sut.Write();
            string result = sb.ToString();

            // Assert
            Assert.That( result, Is.EqualTo( "[true,false,null,123,12.5]" ) );
        }
    }
}
