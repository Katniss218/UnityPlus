using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public class SerializationUnitLoader : ILoader
    {
        private SerializedData[] _data;
        private object[] _objects;

        private Type _memberType; // Specifies the type that all serialized/deserialized objects will derive from. May be `typeof(object)`
        private int _context = default;

        private SerializationMapping[] _mappingCache;

        public IForwardReferenceMap RefMap { get; set; }

        internal SerializationUnitLoader( SerializedData[] data, Type memberType, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._data = data;
            this._memberType = memberType;
            this._context = context;
        }

        internal SerializationUnitLoader( object[] objects, SerializedData[] data, Type memberType, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._objects = objects;
            this._data = data;
            this._memberType = memberType;
            this._context = context;
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize()
        {
            this.LoadCallback();
            this.LoadReferencesCallback();
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( IForwardReferenceMap l )
        {
            this.RefMap = l;
            this.LoadCallback();
            this.LoadReferencesCallback();
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate()
        {
            this.PopulateCallback();
            this.LoadReferencesCallback();
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( IForwardReferenceMap l )
        {
            this.RefMap = l;
            this.PopulateCallback();
            this.LoadReferencesCallback();
        }

        //
        //  Retrieval methods.
        //

        /// <summary>
        /// Returns the objects that were deserialized or populated.
        /// </summary>
        public IEnumerable<object> GetObjects()
        {
            return _objects;
        }

        /// <summary>
        /// Returns the objects that were deserialized or populated, but only those that are of the specified type.
        /// </summary>
        public IEnumerable<T> GetObjectsOfType<T>()
        {
            return _objects.OfType<T>();
        }


        private void PopulateCallback()
        {
            // Called by the loader.

            _mappingCache = new SerializationMapping[_data.Length];

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                if( !data.TryGetValue( KeyNames.TYPE, out var type ) )
                    continue;

                Type type2 = type.DeserializeType();

                var mapping = SerializationMappingRegistry.GetMappingOrEmpty( _context, type2 );
                _mappingCache[i] = mapping;

                // Parity with Member (mostly).
                object member;
                switch( mapping.SerializationStyle )
                {
                    default:
                        continue;
                    case SerializationStyle.PrimitiveStruct:
                        member = mapping.Instantiate( data, this );
                        break;
                    case SerializationStyle.NonPrimitive:
                        member = _objects[i]; // Don't instantiate when populating, object should already be created.
                        mapping.Load( ref member, data, this );
                        break;
                }

                _objects[i] = member;
            }
        }

        private void LoadCallback()
        {
            // Called by the loader.

            _objects = new object[_data.Length];
            _mappingCache = new SerializationMapping[_data.Length];

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                Type typeToAssignTo = _memberType;

                if( data.TryGetValue( KeyNames.TYPE, out var type ) )
                    typeToAssignTo = type.DeserializeType();

                var mapping = SerializationMappingRegistry.GetMappingOrEmpty( _context, typeToAssignTo );
                _mappingCache[i] = mapping;

                // Parity with Member.
                object member;
                switch( mapping.SerializationStyle )
                {
                    default:
                        continue;
                    case SerializationStyle.PrimitiveStruct:
                        member = mapping.Instantiate( data, this );
                        break;
                    case SerializationStyle.NonPrimitive:
                        member = mapping.Instantiate( data, this );
                        mapping.Load( ref member, data, this );
                        break;
                }

                _objects[i] = member;
            }
        }

        private void LoadReferencesCallback()
        {
            // Called by the loader.

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                if( data == null )
                    continue;

                var mapping = _mappingCache[i];

                if( mapping == null )
                    continue; // error.

                object member = _objects[i];
                switch( mapping.SerializationStyle )
                {
                    default:
                        continue;
                    case SerializationStyle.PrimitiveObject:
                        member = mapping.Instantiate( data, this );
                        break;
                    case SerializationStyle.NonPrimitive:
                        mapping.LoadReferences( ref member, data, this );
                        break;
                }
                _objects[i] = member;
            }
        }
    }
}
