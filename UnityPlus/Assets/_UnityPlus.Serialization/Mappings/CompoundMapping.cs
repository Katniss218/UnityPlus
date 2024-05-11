using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{

    /// <summary>
    /// Creates a <see cref="SerializedObject"/> from the child mappings.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class CompoundMapping<TSource> : SerializationMapping, IEnumerable<(string, MappedMember<TSource>)>
    {
        private List<(string, MappedMember<TSource>)> _items = new();
        private Func<SerializedData, IForwardReferenceMap, TSource> _customFactory = null;

        public CompoundMapping()
        {

        }

        public CompoundMapping<TSource> WithFactory( Func<SerializedData, IForwardReferenceMap, TSource> customFactory )
        {
            this._customFactory = customFactory;
            return this;
        }

        public void Add( (string, MappedMember<TSource>) item )
        {
            _items.Add( item );
        }

        public IEnumerator<(string, MappedMember<TSource>)> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public override SerializedData Save( object obj, IReverseReferenceMap s )
        {
            SerializedObject root = new SerializedObject();

            TSource sourceObj = (TSource)obj;

            root[KeyNames.ID] = s.GetID( sourceObj ).SerializeGuid();
            root[KeyNames.TYPE] = obj.GetType().GetData();

            foreach( var item in _items )
            {
                if( item.Item2 is IMappedMember<TSource> member )
                {
                    SerializedData data = member.Save( sourceObj, s );
                    root[item.Item1] = data;
                }
                else if( item.Item2 is IMappedReferenceMember<TSource> memberRef )
                {
                    SerializedData data = memberRef.Save( sourceObj, s );
                    root[item.Item1] = data;
                }
            }

            return root;
        }

        public override object Load( SerializedData data, IForwardReferenceMap l )
        {
            TSource obj;
            if( _customFactory == null )
            {
                obj = Activator.CreateInstance<TSource>();
                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.SetObj( id.DeserializeGuid(), obj );
                }
            }
            else
            {
                obj = _customFactory.Invoke( data, l );
            }

            foreach( var item in _items )
            {
                if( item.Item2 is IMappedMember<TSource> member )
                {
                    if( data.TryGetValue( item.Item1, out var memberData ) )
                    {
                        member.Load( obj, memberData, l );
                    }
                }
            }

            return obj;
        }

        public override void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            TSource sourceObj = (TSource)obj;

            foreach( var item in _items )
            {
                if( item.Item2 is IMappedMember<TSource> member )
                {
                    if( data.TryGetValue( item.Item1, out var memberData ) )
                    {
                        member.Load( sourceObj, memberData, l );
                    }
                }
            }
        }
    }
}