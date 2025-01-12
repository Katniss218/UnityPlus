using NUnit.Framework;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace Neoserialization
{
    public class AsyncMappingTests_Arrays
    {
        [Test]
        public void Mapping___IntArray___RoundTrip()
        {
            // Arrange
            var initialValue = new int[500];

            for( int i = 0; i < initialValue.Length; i++ )
            {
                initialValue[i] = i;
            }
             
            var su = SerializationUnit.FromObjectsAsync<int[]>( initialValue );
            su.AllowedMilisecondsPerInvocation = 100;
            //do
            //{ 
            su.Serialize();
            su.Serialize();
            /*su.Serialize();
            su.Serialize();
            su.Serialize();
            su.Serialize();
            su.Serialize();*/
            //} while( !su.Result.HasFlag( SerializationResult.Finished ) );
            var data = su.GetData().First();
            Debug.Log( ((SerializedArray)data["value"]).Count );

            var su2 = SerializationUnit.FromDataAsync<int[]>( data );
            su2.AllowedMilisecondsPerInvocation = 1000;
            //do
            //{
            su2.Deserialize();
            su2.Deserialize();
            su2.Deserialize();
            su2.Deserialize();
            su2.Deserialize();
            su2.Deserialize();
            su2.Deserialize();
            //} while( !su2.Result.HasFlag( SerializationResult.Finished ) );
            int[] finalValue = su2.GetObjects().First();
            // var data = SerializationUnit.Serialize<int[]>( initialValue );
            // var finalValue = SerializationUnit.Deserialize<int[]>( data );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }
    }
}