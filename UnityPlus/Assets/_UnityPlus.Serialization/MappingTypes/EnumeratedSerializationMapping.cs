using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public class EnumeratedSerializationMapping<TSource, TElement> : SerializationMapping where TSource : IEnumerable<TElement>
    {
        Action<TSource, int, TElement> elementSetter;
        int elementContext;
        private bool _objectHasBeenInstantiated;

        Func<SerializedData, ILoader, object> _rawFactory = null;
        Func<int, object> factory;
        Func<IEnumerable<TElement>, object> lateFactory;
        List<TElement> elementStorageLocations;

        List<int> _previousFailures;
        private int _startFrom = 0;

        public EnumeratedSerializationMapping( Action<TSource, int, TElement> setter )
        {
            elementContext = 0;
            this.elementSetter = setter;
        }

        public EnumeratedSerializationMapping( int elementContext, Action<TSource, int, TElement> setter )
        {
            this.elementContext = elementContext;
            this.elementSetter = setter;
        }

        public override SerializationMapping GetInstance()
        {
            return new EnumeratedSerializationMapping<TSource, TElement>( elementContext, elementSetter )
            {
                Context = Context,
            };
        }

        public override MappingResult Save<T>( T obj, ref SerializedData data, ISaver s )
        {
            if( obj == null )
            {
                return MappingResult.Finished;
            }

            TSource sourceObj = (TSource)(object)obj;

            SerializedArray serArray;
            if( data == null )
            {
                data = new SerializedObject();
                data[KeyNames.ID] = s.RefMap.GetID( sourceObj ).SerializeGuid();
                data[KeyNames.TYPE] = obj.GetType().SerializeType();

                serArray = new SerializedArray();
                data["value"] = serArray;
            }
            else
            {
                serArray = (SerializedArray)data["value"];
            }

            foreach( var elem in sourceObj )
            {
                var mapping = SerializationMappingRegistry.GetMapping<TElement>( elementContext, elem );

                SerializedData dataElem = null;

                MappingResult result = mapping.SafeSave( elem, ref dataElem, s );

                serArray.Add( dataElem );
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
                var result = mapping.SafeLoad<TElement>( ref element2, elemData, l );
                if( result == MappingResult.Finished )
                {
                    elementSetter.Invoke( obj2, i, element2 );
                }
                else
                {
#warning TODO - enumerables can use the fact that the order isn't well defined here.
#warning TODO - handle previous failures when called again
                    elementStorageLocations ??= new();
                    elementStorageLocations.Add( element2 );
                }

                /*if( l.ShouldPause() )
                {
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

        public EnumeratedSerializationMapping<TSource, TElement> WithRawFactory( Func<SerializedData, ILoader, object> rawFactory )
        {
            this._rawFactory = rawFactory;
            return this;
        }

        public EnumeratedSerializationMapping<TSource, TElement> WithFactory( Func<IEnumerable<TElement>, object> factory )
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
        public EnumeratedSerializationMapping<TSource, TElement> WithFactory( Func<int, object> factory )
        {
            this.factory = factory;
            return this;
        }
    }
}