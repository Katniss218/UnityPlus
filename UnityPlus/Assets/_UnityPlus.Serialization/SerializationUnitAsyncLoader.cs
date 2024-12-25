using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    [Obsolete( "Not finished yet" )]
    public class SerializationUnitAsyncLoader<T> : ILoader
    {
        private SerializedData[] _data;
        private T[] _objects;

        private Type _memberType; // Specifies the type that all serialized/deserialized objects will derive from. May be `typeof(object)`
        private int _context = default;

        public IForwardReferenceMap RefMap { get; set; }

        public Dictionary<SerializedData, SerializationMapping> MappingCache { get; }

        private Stack<LoadAction> loadActionsToPerform; // something like this?

        internal SerializationUnitAsyncLoader( SerializedData[] data, Type memberType, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.MappingCache = new Dictionary<SerializedData, SerializationMapping>( new SerializedDataReferenceComparer() );
            this._data = data;
            this._memberType = memberType;
            this._context = context;
        }

        internal SerializationUnitAsyncLoader( T[] objects, SerializedData[] data, Type memberType, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.MappingCache = new Dictionary<SerializedData, SerializationMapping>( new SerializedDataReferenceComparer() );
            this._objects = objects;
            this._data = data;
            this._memberType = memberType;
            this._context = context;
        }

        public bool ShouldPause()
        {
            throw new NotImplementedException(); // use stopwatch to tell how long the current load is taking so far.
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
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( IForwardReferenceMap l )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this.RefMap = l;
            this.LoadCallback();
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate()
        {
            this.LoadCallback();
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( IForwardReferenceMap l )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this.RefMap = l;
            this.LoadCallback();
        }

        //
        //  Retrieval methods.
        //

        /// <summary>
        /// Returns the objects that were deserialized or populated.
        /// </summary>
        public IEnumerable<T> GetObjects()
        {
            return _objects;
        }

        /// <summary>
        /// Returns the objects that were deserialized or populated, but only those that are of the specified type.
        /// </summary>
        public IEnumerable<TDerived> GetObjectsOfType<TDerived>()
        {
            return _objects.OfType<TDerived>();
        }

        private void LoadCallback()
        {
            // Called by the loader.

            _objects = new T[_data.Length];

            for( int i = 0; i < _data.Length; i++ )
            {
                SerializedData data = _data[i];

                var mapping = MappingHelper.GetMapping_Load<T>( _context, MappingHelper.GetSerializedType<T>( data ), data, this );

                T member = _objects[i];
                if( mapping.SafeLoad( ref member, data, this ) )
                {
                    _objects[i] = member;
                }
            }
        }
    }
}