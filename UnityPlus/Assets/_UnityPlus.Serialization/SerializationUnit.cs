using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityPlus.Serialization
{
    public static class SerializationUnit
    {
        /// <summary>
        /// Helper method to serialize a single object easily.
        /// </summary>
        public static SerializedData Serialize<T>( T obj )
        {
            var su = FromObjects<T>( obj );
            su.Serialize();
            return su.GetData().First();
        }

        /// <summary>
        /// Helper method to serialize a single object easily.
        /// </summary>
        public static SerializedData Serialize<T>( T obj, IReverseReferenceMap s )
        {
            var su = FromObjects<T>( obj );
            su.Serialize( s );
            return su.GetData().First();
        }

        /// <summary>
        /// Helper method to deserialize a single object easily.
        /// </summary>
        public static T Deserialize<T>( SerializedData data )
        {
            var su = FromData<T>( data );
            su.Deserialize();
            return su.GetObjectsOfType<T>().First();
        }

        /// <summary>
        /// Helper method to deserialize a single object easily.
        /// </summary>
        public static T Deserialize<T>( SerializedData data, IForwardReferenceMap l )
        {
            var su = FromData<T>( data );
            su.Deserialize( l );
            return su.GetObjectsOfType<T>().First();
        }

        /// <summary>
        /// Helper method to populate the members of a single object easily.
        /// </summary>
        public static void Populate<T>( T obj, SerializedData data ) where T : class
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate();
        }

        /// <summary>
        /// Helper method to populate the members of a single object easily.
        /// </summary>
        public static void Populate<T>( T obj, SerializedData data, IForwardReferenceMap l ) where T : class
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate( l );
        }

        /// <summary>
        /// Helper method to populate the members of a single struct object easily.
        /// </summary>
        public static void Populate<T>( ref T obj, SerializedData data ) where T : struct
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate();
            obj = su.GetObjectsOfType<T>().First();
        }

        /// <summary>
        /// Helper method to populate the members of a single struct object easily.
        /// </summary>
        public static void Populate<T>( ref T obj, SerializedData data, IForwardReferenceMap l ) where T : struct
        {
            var su = PopulateObject<T>( obj, data );
            su.Populate( l );
            obj = su.GetObjectsOfType<T>().First();
        }

        //
        //  Creation methods (separate create + act + retrieve).
        //

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified object of type <typeparamref name="T"/>.
        /// </summary>
        public static SerializationUnitSaver FromObjects<T>( T obj )
        {
            return new SerializationUnitSaver( new object[] { obj }, typeof( T ), ObjectContext.Default );
        }

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified collection of objects.
        /// </summary>
        public static SerializationUnitSaver FromObjects( IEnumerable<object> objects )
        {
            return new SerializationUnitSaver( objects.ToArray(), typeof( object ), ObjectContext.Default );
        }

        /// <summary>
        /// Creates a serialization unit that will serialize (save) the specified collection of objects.
        /// </summary>
        public static SerializationUnitSaver FromObjects( params object[] objects )
        {
            return new SerializationUnitSaver( objects, typeof( object ), ObjectContext.Default );
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) an object of type <typeparamref name="T"/> from the specified serialized representation.
        /// </summary>
        public static SerializationUnitLoader FromData<T>( SerializedData data )
        {
            return new SerializationUnitLoader( new SerializedData[] { data }, typeof( T ), ObjectContext.Default );
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) a collection of objects from the specified serialized representations.
        /// </summary>
        public static SerializationUnitLoader FromData( IEnumerable<SerializedData> data )
        {
            return new SerializationUnitLoader( data.ToArray(), typeof(object), ObjectContext.Default );
        }

        /// <summary>
        /// Creates a serialization unit that will deserialize (instantiate and load) a collection of objects from the specified serialized representations.
        /// </summary>
        public static SerializationUnitLoader FromData( params SerializedData[] data )
        {
            return new SerializationUnitLoader( data, typeof( object ), ObjectContext.Default );
        }

        /// <summary>
        /// Creates a serialization unit that will populate (load) the members of the specified object of type <typeparamref name="T"/> with the specified serialized representation of the same object.
        /// </summary>
        public static SerializationUnitLoader PopulateObject<T>( T obj, SerializedData data )
        {
            return new SerializationUnitLoader( new object[] { obj }, new SerializedData[] { data }, typeof( T ), ObjectContext.Default );
        }

        /// <summary>
        /// Creates a serialization unit that will populate (load) the members of the specified objects with the corresponding specified serialized representations (objects[i] <![CDATA[<]]>==> data[i]).
        /// </summary>
        public static SerializationUnitLoader PopulateObjects( object[] objects, SerializedData[] data )
        {
            return new SerializationUnitLoader( objects, data, typeof( object ), ObjectContext.Default );
        }
    }
}