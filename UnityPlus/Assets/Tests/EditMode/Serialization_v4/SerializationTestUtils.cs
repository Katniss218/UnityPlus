using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityPlus.Serialization;

namespace Neoserialization.Tests
{
    public class SerializedDataEqualityComparer : IEqualityComparer<SerializedData>
    {
        public static readonly SerializedDataEqualityComparer Instance = new SerializedDataEqualityComparer();

        public bool Equals( SerializedData x, SerializedData y )
        {
            if( ReferenceEquals( x, y ) ) return true;
            if( x is null || y is null ) return false;
            if( x.GetType() != y.GetType() ) return false;

            if( x is SerializedPrimitive px && y is SerializedPrimitive py )
            {
                return px.Equals( py );
            }

            if( x is SerializedArray ax && y is SerializedArray ay )
            {
                if( ax.Count != ay.Count ) return false;
                for( int i = 0; i < ax.Count; i++ )
                {
                    if( !Equals( ax[i], ay[i] ) ) return false;
                }
                return true;
            }

            if( x is SerializedObject ox && y is SerializedObject oy )
            {
                if( ox.Count != oy.Count ) return false;
                foreach( var kvp in ox )
                {
                    if( !oy.TryGetValue( kvp.Key, out var yVal ) ) return false;

                    if( kvp.Key == KeyNames.ID ) continue;
                    if( kvp.Key == KeyNames.REF ) continue;

                    if( !Equals( kvp.Value, yVal ) ) return false;
                }
                return true;
            }

            return false;
        }

        public int GetHashCode( SerializedData obj )
        {
            if( obj == null ) return 0;

            if( obj is SerializedPrimitive p )
            {
                return p.GetHashCode();
            }

            if( obj is SerializedArray a )
            {
                int hash = 17;
                foreach( var item in a )
                {
                    hash = hash * 31 + GetHashCode( item );
                }
                return hash;
            }

            if( obj is SerializedObject o )
            {
                int hash = 17;
                foreach( var key in o.Keys.OrderBy( k => k ) )
                {
                    hash = hash * 23 + key.GetHashCode();
                    if( key != KeyNames.ID && o.TryGetValue( key, out var val ) )
                    {
                        hash = hash * 23 + GetHashCode( val );
                    }
                }
                return hash;
            }

            return obj.GetHashCode();
        }
    }

    public class ReflectionMemberWrapper<T>
    {
        public T Value;
    }

    public static class SerializationTestUtils
    {
        public static void AssertRoundTrip<T>( T original, SerializedData expectedData = null, ContextKey context = default, SerializationConfiguration config = default, Action<T, T> customAssert = null )
        {
            // validate that the serialized data matches the provided data.
            // validate that deserializing and serializing again gives the same data (round-trip).
            // - both as a root and as a member.

            if( context == default )
                context = ContextKey.Default;
            if( config == default )
                config = SerializationConfiguration.Default;

            // 1. Test as root
            SerializedData rootData = SerializationUnit.Serialize( context, original, config );

            Assert.That( rootData, Is.EqualTo( expectedData ).Using( SerializedDataEqualityComparer.Instance ),
                "Serialized data did not match expected shape: (a):" + rootData?.DumpToString() + " : (e):" + expectedData?.DumpToString() );

            T rootDeserialized = SerializationUnit.Deserialize<T>( context, rootData, config );

            if( customAssert != null )
            {
                customAssert( original, rootDeserialized );
            }
            else
            {
                SerializedData rootData2 = SerializationUnit.Serialize( context, rootDeserialized, config );
                Assert.That( rootData2, Is.EqualTo( rootData ).Using( SerializedDataEqualityComparer.Instance ),
                    "Root round-trip serialization did not match expected shape: (a):" + rootData2?.DumpToString() + " : (e):" + rootData?.DumpToString() );
            }

            // 2. Test as member
            var wrapper = new ReflectionMemberWrapper<T>() { Value = original };
            SerializedData wrapperData = SerializationUnit.Serialize( context, wrapper, config );
            var wrapperDeserialized = SerializationUnit.Deserialize<ReflectionMemberWrapper<T>>( context, wrapperData, config );

            if( customAssert != null )
            {
                customAssert( original, wrapperDeserialized.Value );
            }
            else
            {
                SerializedData wrapperData2 = SerializationUnit.Serialize( context, wrapperDeserialized, config );
                Assert.That( wrapperData2, Is.EqualTo( wrapperData ).Using( SerializedDataEqualityComparer.Instance ),
                    "Member round-trip serialization did not match expected shape: (a):" + wrapperData2?.DumpToString() + " : (e):" + wrapperData?.DumpToString() );
            }
        }
    }
}