using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization
{
    public class SerializationUnitAsyncLoader<T> : ILoader
    {
        private SerializedData[] _data;
        private T[] _objects;
        int _startIndex;
        Dictionary<int, RetryEntry<T>> _retryElements;
        bool _wasFailureNoRetry;

        private int _context = default;

        public long AllowedMilisecondsPerInvocation { get; set; } = 100;
        long _lastInvocationTimestamp = 0;

        public IForwardReferenceMap RefMap { get; set; }

        public int CurrentPass { get; private set; }

        public SerializationResult Result { get; private set; }

        internal SerializationUnitAsyncLoader( SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.CurrentPass = -1;
            this._data = data;
            this._context = context;
        }

        internal SerializationUnitAsyncLoader( T[] objects, SerializedData[] data, int context )
        {
            this.RefMap = new BidirectionalReferenceStore();
            this.CurrentPass = -1;
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
        public void Deserialize()
        {
            this._objects = new T[_data.Length];
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.Result = this.LoadCallback( false );
        }

        /// <summary>
        /// Performs deserialization of the previously specified objects.
        /// </summary>
        public void Deserialize( IForwardReferenceMap l )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this._objects = new T[_data.Length];
            this.RefMap = l;
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.Result = this.LoadCallback( false );
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate()
        {
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.Result = this.LoadCallback( true );
        }

        /// <summary>
        /// Performs population of members of the previously specified objects.
        /// </summary>
        public void Populate( IForwardReferenceMap l )
        {
            if( l == null )
                throw new ArgumentNullException( nameof( l ), $"The reference map to use can't be null." );

            this.RefMap = l;
            _lastInvocationTimestamp = Stopwatch.GetTimestamp();

            this.Result = this.LoadCallback( true );
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

        private SerializationResult LoadCallback( bool populate )
        {
#warning TODO - only increment when starting a new pass
            this.CurrentPass++;

            if( _retryElements != null )
            {
                List<int> retryMembersThatSucceededThisTime = new();

                foreach( (int i, var entry) in _retryElements )
                {
                    if( entry.pass == CurrentPass )
                        continue;

                    T obj = _objects[i];
                    SerializedData data = _data[i];

                    var mapping = SerializationMappingRegistry.GetMapping<T>( _context, obj );

                    SerializationResult elementResult = mapping.SafeLoad( ref obj, data, this, populate );
                    if( elementResult.HasFlag( SerializationResult.Failed ) )
                    {
                        entry.pass = CurrentPass;
                    }
                    else if( elementResult.HasFlag( SerializationResult.Finished ) )
                    {
                        retryMembersThatSucceededThisTime.Add( i );
                    }

                    _objects[i] = obj;
                }

                foreach( var i in retryMembersThatSucceededThisTime )
                {
                    _retryElements.Remove( i );
                }
            }

            for( int i = _startIndex; i < _data.Length; i++ )
            {
                T obj = _objects[i];
                SerializedData data = _data[i];

                var mapping = SerializationMappingRegistry.GetMapping<T>( _context, MappingHelper.GetSerializedType<T>( data ) );

                SerializationResult elementResult = mapping.SafeLoad( ref obj, data, this, populate );
                if( elementResult.HasFlag( SerializationResult.Finished ) )
                {
                    if( elementResult.HasFlag( SerializationResult.Failed ) )
                        _wasFailureNoRetry = true;

                    _startIndex = i + 1;
                }
                else
                {
                    _retryElements ??= new();
                    _retryElements.Add( i, new RetryEntry<T>( obj, mapping, CurrentPass ) );
                }

                _objects[i] = obj;
            }

            SerializationResult result = SerializationResult.NoChange;
            if( _wasFailureNoRetry || _retryElements != null && _retryElements.Count != 0 )
                result |= SerializationResult.HasFailures;
            if( _retryElements == null || _retryElements.Count == 0 )
                result |= SerializationResult.Finished;

            if( result.HasFlag( SerializationResult.Finished ) && result.HasFlag( SerializationResult.HasFailures ) )
                result |= SerializationResult.Failed;

            return result;
        }
    }
}