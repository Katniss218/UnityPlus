using NUnit.Framework;
using System.Collections.Generic;
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
            const int SIZE = 70000; // The size has to be large enough to force multiple invocations.
            // Arrange
            var initialValue = new int[SIZE];

            for( int i = 0; i < SIZE; i++ )
            {
                initialValue[i] = i;
            }

            var su = SerializationUnit.FromObjectsAsync<int[]>( initialValue );
            su.AllowedMilisecondsPerInvocation = 10;

            do
            {
                su.Serialize();
            } while( !su.Result.HasFlag( SerializationResult.Finished ) );

            var data = su.GetData().First();
            Debug.Log( "S" + ((SerializedArray)data["value"]).Count );


            var su2 = SerializationUnit.FromDataAsync<int[]>( data );
            su2.AllowedMilisecondsPerInvocation = 10;

            do
            {
                su2.Deserialize();
            } while( !su2.Result.HasFlag( SerializationResult.Finished ) );

            int[] finalValue = su2.GetObjects().First();
            Debug.Log( "D" + (finalValue.Where( x => x > 0 ).Count() + 1) );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }

        [Test]
        public void Mapping___Dictionary___RoundTrip()
        {
            const int SIZE = 20000; // The size has to be large enough to force multiple invocations.

            // Arrange
            var initialValue = new Dictionary<string, int>( SIZE );

            for( int i = 0; i < SIZE; i++ )
            {
                initialValue.Add( i.ToString(), i );
            }

            var su = SerializationUnit.FromObjectsAsync<Dictionary<string, int>>( initialValue );
            su.AllowedMilisecondsPerInvocation = 10;

            do
            {
                su.Serialize();
            } while( !su.Result.HasFlag( SerializationResult.Finished ) );

            var data = su.GetData().First();
            Debug.Log( "S" + ((SerializedArray)data["value"]).Count );


            var su2 = SerializationUnit.FromDataAsync<Dictionary<string, int>>( data );
            su2.AllowedMilisecondsPerInvocation = 10;

            do
            {
                su2.Deserialize();
            } while( !su2.Result.HasFlag( SerializationResult.Finished ) );

            Dictionary<string, int> finalValue = su2.GetObjects().First();
            Debug.Log( "D" + finalValue.Count() );

            // Assert
            Assert.That( finalValue, Is.EquivalentTo( initialValue ) );
        }
    }
}