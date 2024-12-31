using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization.ReferenceMaps;
using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace UnityPlus.Serialization
{
    public class SerializationUnitAsyncLoader<T> : ILoader
    {
        private bool[] _finishedMembers;
        private SerializedData[] _data;
        private T[] _objects;

        private int _context = default;

        public long AllowedMilisecondsPerInvocation { get; set; } = 100;
        long _lastInvocationTimestamp = 0;

        public IForwardReferenceMap RefMap { get; set; }
        public MappingResult Result { get; private set; }

        internal SerializationUnitAsyncLoader( SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._data = data;
            this._context = context;
        }

        internal SerializationUnitAsyncLoader( T[] objects, SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this._objects = objects;
            this._data = data;
            this._context = context;
        }

        public bool ShouldPause()
        {
            long stamp = Stopwatch.GetTimestamp();
            long miliseconds = ((stamp - _lastInvocationTimestamp) * 1000) / Stopwatch.Frequency;

            return (miliseconds > AllowedMilisecondsPerInvocation);
        }

        //
        //  Acting methods.
        //

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( int maxIters = 10 )
        {
            this._finishedMembers = new bool[_data.Length];
            this._objects = new T[_data.Length];
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.LoadCallback();
                this.Result = result;
                if( result != MappingResult.Progressed )
                    return;
            }
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( IForwardReferenceMap l, int maxIters = 10 )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this._finishedMembers = new bool[_data.Length];
            this._objects = new T[_data.Length];
            this.RefMap = l;
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.LoadCallback();
                this.Result = result;
                if( result != MappingResult.Progressed )
                    return;
            }
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( int maxIters = 10 )
        {
            this._finishedMembers = new bool[_data.Length];
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.LoadCallback();
                this.Result = result;
                if( result != MappingResult.Progressed )
                    return;
            }
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( IForwardReferenceMap l, int maxIters = 10 )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this._finishedMembers = new bool[_data.Length];
            this.RefMap = l;
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            for( int i = 0; i < maxIters; i++ )
            {
                MappingResult result = this.LoadCallback();
                this.Result = result;
                if( result != MappingResult.Progressed )
                    return;
            }
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
        public IEnumerable<TDerived> GetObjects<TDerived>()
        {
            return _objects.OfType<TDerived>();
        }

        private MappingResult LoadCallback()
        {
            bool anyFailed = false;
            bool anyFinished = false;
            bool anyProgressed = false;

            for( int i = 0; i < _data.Length; i++ )
            {
                if( _finishedMembers[i] )
                    continue;

                SerializedData data = _data[i];

                var mapping = SerializationMappingRegistry.GetMapping<T>( _context, MappingHelper.GetSerializedType<T>( data ) );

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