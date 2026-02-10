
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace Serialization.Json
{
    public class JsonStreamReaderTests
    {
        [Test]
        public void Read___Complicated___ParsesCorrectlyFromStream()
        {
            // Arrange 
            string json = @"{
                ""Config"": {
                    ""Version"": 1.0,
                    ""Enabled"": true,
                    ""Tags"": [""Alpha"", ""Beta""]
                }
            }";

            using( var stream = new MemoryStream( Encoding.UTF8.GetBytes( json ) ) )
            {
                JsonStreamReader sut = new JsonStreamReader( stream );

                // Act
                SerializedObject val = (SerializedObject)sut.Read();

                // Assert
                Assert.That( (double)val["Config"]["Version"], Is.EqualTo( 1.0 ) );
                Assert.That( (bool)val["Config"]["Enabled"], Is.EqualTo( true ) );

                var tags = (SerializedArray)val["Config"]["Tags"];
                Assert.That( tags.Count, Is.EqualTo( 2 ) );
                Assert.That( (string)tags[0], Is.EqualTo( "Alpha" ) );
                Assert.That( (string)tags[1], Is.EqualTo( "Beta" ) );
            }
        }

        [Test]
        public void Read___LargeInput___ParsesCorrectly()
        {
            // Arrange
            // Simulate a stream larger than the internal buffer
            StringBuilder sb = new StringBuilder();
            sb.Append( "{\"Data\":[" );
            for( int i = 0; i < 1000; i++ )
            {
                sb.Append( i );
                if( i < 999 ) sb.Append( "," );
            }
            sb.Append( "]}" );

            using( var stream = new MemoryStream( Encoding.UTF8.GetBytes( sb.ToString() ) ) )
            {
                JsonStreamReader sut = new JsonStreamReader( stream );

                // Act
                SerializedObject val = (SerializedObject)sut.Read();

                // Assert
                SerializedArray arr = (SerializedArray)val["Data"];
                Assert.That( arr.Count, Is.EqualTo( 1000 ) );
                Assert.That( (int)arr[0], Is.EqualTo( 0 ) );
                Assert.That( (int)arr[999], Is.EqualTo( 999 ) );
            }
        }
    }
}
