using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Creates a <see cref="SerializedArray"/> from the mapped object.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class ListMapping<TSource> : SerializationMapping, IEnumerable<MemberMapping<TSource>>
    {
        private List<MemberMapping<TSource>> _items = new();

        public IEnumerator<MemberMapping<TSource>> GetEnumerator()
        {
            return ((IEnumerable<MemberMapping<TSource>>)_items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        public void Add( MemberMapping<TSource> item )
        {
            _items.Add( item );
        }

        public SerializedArray GetObjectsPass( TSource obj, IReverseReferenceMap s )
        {
            // get data from all members

            SerializedArray root = new SerializedArray();

            return root;
        }

        public void SetObjectsPass( TSource obj, SerializedArray root, IForwardReferenceMap l )
        {
        }

        public SerializedArray GetDataPass( TSource obj, IReverseReferenceMap s )
        {
            // get data from all members

            SerializedArray root = new SerializedArray();

            return root;
        }

        public void SetDataPass( TSource obj, SerializedArray root, IForwardReferenceMap l )
        {
        }
    }
}