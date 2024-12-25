using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public class EnumeratedSerializationMapping<TSource, TElement> : SerializationMapping where TSource : IEnumerable<TElement>
    {
        Action<TSource, int, TElement> setter;
        int elementContext;

        public EnumeratedSerializationMapping( Action<TSource, int, TElement> setter )
        {
            elementContext = 0;
            this.setter = setter;
        }

        public EnumeratedSerializationMapping( int elementContext, Action<TSource, int, TElement> setter )
        {
            this.elementContext = elementContext;
            this.setter = setter;
        }

        public override SerializationMapping GetInstance()
        {
            return new EnumeratedSerializationMapping<TSource, TElement>( elementContext, setter )
            {
                Context = Context,
            };
        }

        public override bool Save<T>( T obj, ref SerializedData data, ISaver s )
        {
            // only add data that is fully formed. skip otherwise and add later.
            throw new NotImplementedException();
        }

        public override bool Load<T>( ref T obj, SerializedData data, ILoader l )
        {
            // only add members that are fully formed. needs to hold the data otherwise.
            throw new NotImplementedException();
        }

        public IndexedSerializationMapping<TSource, TElement> WithFactory( Func<IEnumerable<TElement>, object> factory )
        {
            // loads members into a list first, then passes that list here.
            // can be used for e.g. readonlydictionary.
            throw new NotImplementedException();
        }
    }

#warning TODO add a collection mapping for IList / IList<T>?

    public class IndexedSerializationMapping<TSource, TElement> : SerializationMapping
    {
        int elementContext;
        Func<TSource, int> elementCountGetter;
        Func<TSource, int, TElement> elementGetter;
        Action<TSource, int, TElement> elementSetter;

        Func<int, object> earlyFactory;
        Func<IEnumerable<TElement>, object> factory;

        //HashSet<int> _indices = new();

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
                earlyFactory = earlyFactory,
                factory = factory
            };
        }

        public override bool Save<T>( T obj, ref SerializedData data, ISaver s )
        {
            if( obj == null )
                return false;

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

            for( int i = 0; i < length; i++ )
            {
#warning TODO - store which elements were already done.
                var elem = elementGetter.Invoke( sourceObj, i );

                var mapping = SerializationMappingRegistry.GetMapping<TElement>( elementContext, elem );

                SerializedData dataElem = null;

                var ret = mapping.SafeSave( elem, ref dataElem, s );

                if( i < serArray.Count )
                {
                    serArray[i] = dataElem;
                }
                else
                {
                    serArray.Add( dataElem );
                }

                if( s.ShouldPause() )
                {
                    break;
                }
                //_indices for later.
                // assume indices up to a given 'last' index are finished, except for indices specified by a collection.
                // start at the 'last' index, but try to save/load the potentially failed indices as well.
            }
            return true;
        }

        public override bool Load<T>( ref T obj, SerializedData data, ILoader l )
        {
            TSource obj2 = (TSource)(object)obj;

            SerializedArray array = (SerializedArray)data["value"];
            int length = array.Count;

            if( obj2 == null && earlyFactory != null ) // ready for instantiation means all members that'll be passed into the factory are fully deserialized.
            {
                obj2 = (TSource)earlyFactory.Invoke( length );
            }

            for( int i = 0; i < length; i++ )
            {
                var elemData = array[i];

                Type memberType = typeof( TElement );
                if( elemData != null && elemData.TryGetValue( KeyNames.TYPE, out var type ) )
                {
                    memberType = type.DeserializeType();
                }

                var mapping = MappingHelper.GetMapping_Load<TElement>( elementContext, memberType, elemData, l );

                TElement element2 = default;
                var isFullyLoaded = mapping.SafeLoad<TElement>( ref element2, elemData, l );

                elementSetter.Invoke( obj2, i, element2 );

                if( l.ShouldPause() )
                {
                    break;
                }
            }

            obj = (T)(object)obj2;
            return true;
        }

        public IndexedSerializationMapping<TSource, TElement> WithFactory( Func<IEnumerable<TElement>, object> factory )
        {
            this.factory = factory;
            // loads elements into a list first, then passes that list here.
            // can be used for e.g. readonlylist.
            return this;
        }

        public IndexedSerializationMapping<TSource, TElement> WithFactory( Func<int, object> factory )
        {
            this.earlyFactory = factory;
            return this;
        }
    }
}