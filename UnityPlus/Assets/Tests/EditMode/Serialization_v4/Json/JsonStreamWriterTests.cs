
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace Serialization.Json
{
    public class JsonStreamWriterTests
    {
        [Test]
        public void Write___ComplicatedObject___WritesCorrectlyToStream()
        {
            // Arrange
            var root = new SerializedObject();
            root["Id"] = (SerializedPrimitive)101;
            root["Name"] = (SerializedPrimitive)"Stream Test";

            var array = new SerializedArray();
            array.Add( (SerializedPrimitive)1 );
            array.Add( (SerializedPrimitive)2 );
            root["Values"] = array;

            using( var stream = new MemoryStream() )
            {
                JsonStreamWriter sut = new JsonStreamWriter( root, stream );

                // Act
                sut.Write();

                // Assert
                string result = Encoding.UTF8.GetString( stream.ToArray() );
                Assert.That( result, Contains.Substring( "\"Id\":101" ) );
                Assert.That( result, Contains.Substring( "\"Name\":\"Stream Test\"" ) );
                Assert.That( result, Contains.Substring( "\"Values\":[1,2]" ) );
            }
        }

        [Test]
        public void Write___DeepNesting___WritesCorrectly()
        {
            // Arrange
            var root = new SerializedObject();
            var current = root;

            // Create nested structure: {"L0":{"L1":{"L2":...}}}
            for( int i = 0; i < 5; i++ )
            {
                var next = new SerializedObject();
                current[$"L{i}"] = next;
                current = next;
            }
            current["Leaf"] = (SerializedPrimitive)"End";

            using( var stream = new MemoryStream() )
            {
                JsonStreamWriter sut = new JsonStreamWriter( root, stream );

                // Act
                sut.Write();

                // Assert
                string result = Encoding.UTF8.GetString( stream.ToArray() );
                Assert.That( result, Contains.Substring( "\"L0\":{" ) );
                Assert.That( result, Contains.Substring( "\"L4\":{" ) );
                Assert.That( result, Contains.Substring( "\"Leaf\":\"End\"" ) );
                Assert.That( result.EndsWith( "}}}}}}" ) );
            }
        }
    }
}
