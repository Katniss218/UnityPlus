using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public class SerializationUnitSaver : ISaver
    {
        private SerializedData[] _data;
        private object[] _objects;

        private Type _memberType; // Specifies the type that all serialized/deserialized objects will derive from. May be `typeof(object)`
        private int _context = default;

        public IReverseReferenceMap RefMap { get; set; }

        internal SerializationUnitSaver( object[] objects, Type memberType, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._objects = objects;
            this._memberType = memberType;
            this._context = context;
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public void Serialize()
        {
            this.SaveCallback();
        }

        /// <summary>
        /// Performs serialization of the previously specified objects.
        /// </summary>
        public void Serialize( IReverseReferenceMap s )
        {
            this.RefMap = s;
            this.SaveCallback();
        }

        //
        //  Retrieval methods.
        //

        /// <summary>
        /// Returns the data that was serialized.
        /// </summary>
        public IEnumerable<SerializedData> GetData()
        {
            return _data;
        }

        /// <summary>
        /// Returns the data that was serialized, but only of objects that are of the specified type.
        /// </summary>
        public IEnumerable<SerializedData> GetDataOfType<T>()
        {
            return _data.Where( d =>
            {
                return d.TryGetValue( KeyNames.TYPE, out var type ) && typeof( T ).IsAssignableFrom( type.DeserializeType() );
            } );
        }

        private void SaveCallback()
        {
            _data = new SerializedData[_objects.Length];

            for( int i = 0; i < _objects.Length; i++ )
            {
                object obj = _objects[i];

                if( obj == null )
                    continue;

                var mapping = SerializationMappingRegistry.GetMappingOrDefault( _context, obj );

                _data[i] = mapping.Save( obj, this );
            }
        }
    }
}
