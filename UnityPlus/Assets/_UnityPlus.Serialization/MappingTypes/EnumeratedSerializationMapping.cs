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
            if( obj == null )
                return true;

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
#warning INFO - this is a primitive mapping, since enumerables can't be paused.
#warning TODO - if this is a primitive, then all of its members must also be primitive. i.e. not pause, else we lose data for that member.
            foreach( var elem in sourceObj )
            {
                var mapping = SerializationMappingRegistry.GetMapping<TElement>( elementContext, elem );

                SerializedData dataElem = null;

                var ret = mapping.SafeSave( elem, ref dataElem, s );

                serArray.Add( dataElem );
            }
            return true;

            // only add data that is fully formed. skip otherwise and add later.
            throw new NotImplementedException();
        }

        public override bool Load<T>( ref T obj, SerializedData data, ILoader l )
        {
            TSource obj2 = (obj == null) ? default : (TSource)(object)obj;

            if( data == null )
            {
                obj = (T)(object)obj2;
                return true;
            }

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
}