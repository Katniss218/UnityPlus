using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;

namespace UnityPlus.Serialization
{
    internal interface ISerializationMappingWithCustomFactory
    {
        Func<SerializedData, IForwardReferenceMap, object> CustomFactory { get; }
    }


    /// <summary>
    /// Serializes a member of type <typeparamref name="TMember"/>, that belongs to a type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that contains the member.</typeparam>
    /// <typeparam name="TMember">The type of the member (field/property/etc).</typeparam>
    internal class PassthroughMember<TSource, TSourceBase> : MemberBase<TSource>, IMappedMember<TSource>, IMappedReferenceMember<TSource> where TSourceBase : class
    {
        IMappedMember<TSourceBase> _member;
        IMappedReferenceMember<TSourceBase> _refmember;

        internal static PassthroughMember<TSource, TSourceBase> Create( MemberBase<TSourceBase> member )
        {
            var m = new PassthroughMember<TSource, TSourceBase>()
            {
                _member = member as IMappedMember<TSourceBase>,
                _refmember = member as IMappedReferenceMember<TSourceBase>
            };
            return m;
        }

        public SerializedData Save( TSource source, IReverseReferenceMap s )
        {
            if( _refmember == null )
                return _member?.Save( source as TSourceBase, s ) ?? null;
            else
                return _refmember?.Save( source as TSourceBase, s ) ?? null;
        }

        public void Load( ref TSource source, SerializedData data, IForwardReferenceMap l )
        {
            TSourceBase src = source as TSourceBase; // won't work for structs, but structs aren't inheritable anyway.

            if( _member != null )
                _member?.Load( ref src, data, l );
        }

        public void LoadReferences( ref TSource source, SerializedData data, IForwardReferenceMap l )
        {
            TSourceBase src = source as TSourceBase; // won't work for structs, but structs aren't inheritable anyway.

            if( _refmember != null )
                _refmember?.LoadReferences( ref src, data, l );
        }
    }

    /// <summary>
    /// Creates a <see cref="SerializedObject"/> from the child mappings.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public class CompoundSerializationMapping<TSource> : SerializationMapping, IEnumerable<(string, MemberBase<TSource>)>, ISerializationMappingWithCustomFactory
    {
        private readonly List<(string, MemberBase<TSource>)> _items = new();
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
        /// Makes this type include the members of the specified base type in its serialization.
        /// </summary>
        public CompoundSerializationMapping<TSource> IncludeMembers<TSourceBase>() where TSourceBase : class
        {
            if( !typeof( TSourceBase ).IsAssignableFrom( typeof( TSource ) ) )
            {
                Debug.LogWarning( $"Tried to include members of `{typeof( TSourceBase ).FullName}` into `{typeof( TSource ).FullName}`, which is not derived from `{typeof( TSourceBase ).FullName}`." );
                return this;
            }

            try
            {
                SerializationMapping mapping = SerializationMappingRegistry.GetMapping( typeof( TSourceBase ) );

                if( ReferenceEquals( mapping, this ) ) // mapping for `this` is a cached mapping of base type.
                    return this;

                if( mapping is CompoundSerializationMapping<TSourceBase> baseMapping )
                {
                    foreach( var item in baseMapping._items )
                    {
                        var member = item.Item2;

                        MemberBase<TSource> m = PassthroughMember<TSource, TSourceBase>.Create( member );

                        this._items.Add( (item.Item1, m) );
                    }
                }
            }
            catch { }

            return this;
        }

        /// <summary>
        /// Makes the deserialization use the factory of the nearest base type of <typeparamref name="TSource"/>.
        /// </summary>
        public CompoundSerializationMapping<TSource> UseBaseTypeFactory()
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
                        member.Load( ref obj, memberData, l );
                    }
                }
            }

            return obj;
        }

        public override void LoadReferences( ref object obj, SerializedData data, IForwardReferenceMap l )
        {
            var objM = (TSource)obj;

            foreach( var item in _items )
            {
                if( item.Item2 is IMappedReferenceMember<TSource> member )
                {
                    if( data.TryGetValue( item.Item1, out var memberData ) )
                    {
                        member.LoadReferences( ref objM, memberData, l );
                    }
                }
            }

            obj = objM;
        }
    }
}