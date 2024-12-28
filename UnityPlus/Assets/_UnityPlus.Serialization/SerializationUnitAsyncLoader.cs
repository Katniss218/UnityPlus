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
        private bool[] _finishedMembers;
        private SerializedData[] _data;
        private T[] _objects;

        private int _context = default;

        public IForwardReferenceMap RefMap { get; set; }

        public Dictionary<SerializedData, SerializationMapping> MappingCache { get; }

        internal SerializationUnitAsyncLoader( SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.MappingCache = new Dictionary<SerializedData, SerializationMapping>( new SerializedDataReferenceComparer() );
            this._data = data;
            this._context = context;
        }

        internal SerializationUnitAsyncLoader( T[] objects, SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.MappingCache = new Dictionary<SerializedData, SerializationMapping>( new SerializedDataReferenceComparer() );
            this._objects = objects;
            this._data = data;
            this._context = context;
        }

        public bool ShouldPause()
        {
            return false;
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( int maxIters = 10 )
        {
            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.LoadCallback();
                if( result == MappingResult.Finished || result == MappingResult.NoChange )
                    return;
                //Debug.Log( i );
            }
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

        private MappingResult LoadCallback()
        {
            _finishedMembers ??= new bool[_data.Length];
            _objects ??= new T[_data.Length];

            bool anyFailed = false;
            bool anyFinished = false;
            bool anyProgressed = false;

            for( int i = 0; i < _data.Length; i++ )
            {
                if( _finishedMembers[i] )
                    continue;

                SerializedData data = _data[i];

                var mapping = MappingHelper.GetMapping_Load<T>( _context, MappingHelper.GetSerializedType<T>( data ), data, this );

                T member = _objects[i];
                var memberResult = mapping.SafeLoad( ref member, data, this );
                switch( memberResult )
                {
                    case MappingResult.Finished:
                        _finishedMembers[i] = true;
                        anyFinished = true;
                        break;
                    case MappingResult.Failed:
                        anyFailed = true;
                        break;
                    case MappingResult.Progressed:
                        anyProgressed = true;
                        break;
                }

                _objects[i] = member;
            }

            return MappingResult_Ex.GetCompoundResult( anyFailed, anyFinished, anyProgressed );
        }
    }
}