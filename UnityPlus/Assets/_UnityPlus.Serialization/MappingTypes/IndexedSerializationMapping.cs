using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public class IndexedSerializationMapping<TSource, TElement> : SerializationMapping
    {
        int elementContext;
        Func<TSource, int> elementCountGetter;
        Func<TSource, int, TElement> elementGetter;
        Action<TSource, int, TElement> elementSetter;
        private bool _objectHasBeenInstantiated;

        Func<SerializedData, ILoader, object> _rawFactory = null;
        Func<int, object> factory;
        Func<IEnumerable<TElement>, object> lateFactory;
        List<TElement> elementStorageLocations;

        List<int> _previousFailures;
        private int _startFrom = 0;

        public IndexedSerializationMapping( Func<TSource, int> countGetter, Func<TSource, int, TElement> getter, Action<TSource, int, TElement> setter )
        {
            elementContext = 0;
            this.elementCountGetter = countGetter;
            this.elementGetter = getter;
            this.elementSetter = setter;
        }

        public IndexedSerializationMapping( Func<TSource, int> countGetter, int elementContext, Func<TSource, int, TElement> getter, Action<TSource, int, TElement> setter )
        {
            this.elementContext = elementContext;
            this.elementCountGetter = countGetter;
            this.elementGetter = getter;
            this.elementSetter = setter;
        }

        public override SerializationMapping GetInstance()
        {
            return new IndexedSerializationMapping<TSource, TElement>( elementCountGetter, elementContext, elementGetter, elementSetter )
            {
                Context = Context,
                factory = factory,
                lateFactory = lateFactory
            };
        }

        public override MappingResult Save<T>( T obj, ref SerializedData data, ISaver s )
        {
            if( obj == null )
            {
                return MappingResult.Finished;
            }

            TSource sourceObj = (TSource)(object)obj;
            int length = elementCountGetter.Invoke( sourceObj );

            SerializedArray serArray;
            if( data == null )
            {
                data = new SerializedObject();
                data[KeyNames.ID] = s.RefMap.GetID( sourceObj ).SerializeGuid();
                data[KeyNames.TYPE] = obj.GetType().SerializeType();

                serArray = new SerializedArray( length );
                data["value"] = serArray;
            }
            else
            {
                serArray = (SerializedArray)data["value"];
            }

            for( int i = _startFrom; i < length; i++ )
            {
                var elem = elementGetter.Invoke( sourceObj, i );

                var mapping = SerializationMappingRegistry.GetMapping<TElement>( elementContext, elem );

                SerializedData dataElem = null;

                var ret = mapping.SafeSave( elem, ref dataElem, s );

                if( ret != MappingResult.Finished )
                {
#warning TODO - handle previous failures when called again
                    _previousFailures.Add( i );
                }
                if( i < serArray.Count )
                {
                    serArray[i] = dataElem;
                }
                else
                {
                    serArray.Add( dataElem );
                }

                /*if( s.ShouldPause() )
                {
                    _startFrom = i;
                    break;
                }*/
            }

            return MappingResult.Finished;
        }

        public override MappingResult Load<T>( ref T obj, SerializedData data, ILoader l )
        {
            if( data == null )
            {
                return MappingResult.Finished;
            }

            TSource obj2 = (obj == null) ? default : (TSource)(object)obj;

            SerializedArray array = (SerializedArray)data["value"];
            int length = array.Count;

            if( !_objectHasBeenInstantiated )
            {
#warning TODO - instantiateearly can fail, if there is no early factory and the type doesn't have a parameterless constructor.
                obj2 = InstantiateEarly( data, l, length );
                _objectHasBeenInstantiated = true;
            }

            for( int i = _startFrom; i < length; i++ )
            {
                var elemData = array[i];

                Type memberType = typeof( TElement );
                if( elemData != null && elemData.TryGetValue( KeyNames.TYPE, out var type ) )
                {
                    memberType = type.DeserializeType();
                }

                var mapping = MappingHelper.GetMapping_Load<TElement>( elementContext, memberType, elemData, l );

                TElement element2 = default;
                MappingResult result = mapping.SafeLoad<TElement>( ref element2, elemData, l );

                if( _objectHasBeenInstantiated )
                {
                    elementSetter.Invoke( obj2, i, element2 );
                }
                else
                {
#warning TODO - handle previous failures when called again
                    elementStorageLocations ??= new();
                    elementStorageLocations.Add( element2 );
                }

                /*if( l.ShouldPause() )
                {
                    _startFrom = i;
                    break;
                }*/
            }

            if( !_objectHasBeenInstantiated )
            {
                obj2 = InstantiateLate( data, l );
                _objectHasBeenInstantiated = true;
            }

            obj = (T)(object)obj2;
            return MappingResult.Finished;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource InstantiateEarly( SerializedData data, ILoader l, int elemCount )
        {
            // early - raw or earlyfactory or activator
            // late - late factory or activator.
            TSource obj;
            if( factory != null )
            {
                obj = (TSource)factory.Invoke( elemCount );
            }
            else if( _rawFactory != null )
            {
                obj = (TSource)_rawFactory.Invoke( data, l );
            }
            else
            {
                if( data == null )
                    return default;

                obj = Activator.CreateInstance<TSource>();
            }

            if( data.TryGetValue( KeyNames.ID, out var id ) )
            {
                l.RefMap.SetObj( id.DeserializeGuid(), obj );
            }

            return obj;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource InstantiateLate( SerializedData data, ILoader l )
        {
            // early - raw or earlyfactory or activator
            // late - late factory or activator.
            TSource obj;

            if( lateFactory != null )
            {
                obj = (TSource)lateFactory.Invoke( elementStorageLocations );
            }
            else
            {
                if( data == null )
                    return default;

                obj = Activator.CreateInstance<TSource>();
            }

            if( data.TryGetValue( KeyNames.ID, out var id ) )
            {
                l.RefMap.SetObj( id.DeserializeGuid(), obj );
            }

            return obj;
        }

        public IndexedSerializationMapping<TSource, TElement> WithRawFactory( Func<SerializedData, ILoader, object> rawFactory )
        {
            this._rawFactory = rawFactory;
            return this;
        }

        public IndexedSerializationMapping<TSource, TElement> WithFactory( Func<IEnumerable<TElement>, object> factory )
        {
            this.lateFactory = factory;
            // loads elements into a list first, then passes that list here.
            // can be used for e.g. readonlylist.
            return this;
        }

        /// <summary>
        /// Input integer is the element count
        /// </summary>
        /// <param name="factory"></param>
        /// <returns></returns>
        public IndexedSerializationMapping<TSource, TElement> WithFactory( Func<int, object> factory )
        {
            this.factory = factory;
            return this;
        }
    }
}