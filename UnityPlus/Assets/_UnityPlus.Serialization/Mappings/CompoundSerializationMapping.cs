using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    internal interface ISerializationMappingWithCustomFactory
    {
        Func<SerializedData, IForwardReferenceMap, object> CustomFactory { get; }
    }

    /// <summary>
    /// Creates a <see cref="SerializedObject"/> from the child mappings.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class CompoundSerializationMapping<TSource> : SerializationMapping, IEnumerable<(string, MemberBase<TSource>)>, ISerializationMappingWithCustomFactory
    {
        private List<(string, MemberBase<TSource>)> _items = new();
        public Func<SerializedData, IForwardReferenceMap, object> CustomFactory { get; private set; } = null;

        public CompoundSerializationMapping()
        {

        }

        /// <summary>
        /// Makes the deserialization use a custom factory method instead of <see cref="Activator.CreateInstance{T}()"/>.
        /// </summary>
        /// <remarks>
        /// The factory is only needed to create an instance, not to set its internal state. The state should be set using the members.
        /// </remarks>
        /// <param name="customFactory">The method used to create an instance of <typeparamref name="TSource"/> from its serialized representation.</param>
        public CompoundSerializationMapping<TSource> WithFactory( Func<SerializedData, IForwardReferenceMap, object> customFactory )
        {
            this.CustomFactory = customFactory;
            return this;
        }

        /// <summary>
        /// Makes the deserialization use the factory of the nearest base type of <typeparamref name="TSource"/>.
        /// </summary>
        public CompoundSerializationMapping<TSource> IncludeRecursiveBaseTypeFactory()
        {
            do
            {
                Type baseType = typeof( TSource ).BaseType;
                if( baseType == null )
                    return this;

                try
                {
                    SerializationMapping mapping = SerializationMappingRegistry.GetMapping( baseType );
                    if( mapping is ISerializationMappingWithCustomFactory m )
                    {
                        this.CustomFactory = m.CustomFactory;
                        return this;
                    }
                }
                catch { }

            } while( this.CustomFactory == null );

            return this;
        }

        public void Add( (string, MemberBase<TSource>) item )
        {
            _items.Add( item );
        }

        public IEnumerator<(string, MemberBase<TSource>)> GetEnumerator()
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
            if( CustomFactory == null )
            {
                obj = Activator.CreateInstance<TSource>();
                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.SetObj( id.DeserializeGuid(), obj );
                }
            }
            else
            {
                obj = (TSource)CustomFactory.Invoke( data, l );
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

        public override void LoadReferences( object obj, SerializedData data, IForwardReferenceMap l )
        {
            var objM = (TSource)obj;

            foreach( var item in _items )
            {
                if( item.Item2 is IMappedReferenceMember<TSource> member )
                {
                    if( data.TryGetValue( item.Item1, out var memberData ) )
                    {
                        member.LoadReferences( objM, memberData, l );
                    }
                }
            }
        }
    }
}