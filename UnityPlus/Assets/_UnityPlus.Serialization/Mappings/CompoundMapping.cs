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
    public class CompoundMapping<TSource> : SerializationMapping, IEnumerable<(string, MemberMapping<TSource>)>
    {
        private List<(string, MemberMapping<TSource>)> _items = new();

        public void Add( (string, MemberMapping<TSource>) item )
        {
            _items.Add( item );
        }

        public IEnumerator<(string, MemberMapping<TSource>)> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        public SerializedObject GetObjectsPass( TSource obj, IReverseReferenceMap s )
        {
            // get data from all members

            SerializedObject root = new SerializedObject();

            foreach( var item in _items )
            {
                if( item.Item2 is IObjectMapping<TSource> dm )
                {
                    SerializedObject memberData = dm.GetObjectsPass( obj, s );
                    root[item.Item1] = memberData;
                }
            }

            return root;
        }

        public void SetObjectsPass( TSource obj, SerializedObject root, IForwardReferenceMap l )
        {
            foreach( var item in _items )
            {
                if( item.Item2 is IObjectMapping<TSource> dm )
                {
                    if( root.TryGetValue<SerializedObject>( item.Item1, out var data ) )
                        dm.SetObjectsPass( obj, data, l );
                }
            }
        }

        public SerializedObject GetDataPass( TSource obj, IReverseReferenceMap s )
        {
            // get data from all members

            SerializedObject root = new SerializedObject();

            foreach( var item in _items )
            {
                if( item.Item2 is IDataMapping<TSource> dm )
                {
                    SerializedData data = dm.GetDataPass( obj, s );
                    root[item.Item1] = data;
                }
            }

            return root;
        }

        public void SetDataPass( TSource obj, SerializedObject root, IForwardReferenceMap l )
        {
            foreach( var item in _items )
            {
                if( item.Item2 is IDataMapping<TSource> dm )
                {
                    if( root.TryGetValue( item.Item1, out var data ) )
                        dm.SetDataPass( obj, data, l );
                }
            }
        }
    }
}