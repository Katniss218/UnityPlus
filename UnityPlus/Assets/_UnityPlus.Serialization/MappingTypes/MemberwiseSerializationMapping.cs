using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Creates a <see cref="SerializedObject"/> from the child mappings.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public sealed class MemberwiseSerializationMapping<TSource> : SerializationMapping, IInstantiableSerializationMapping
    {
        private MemberBase<TSource>[] _factoryMembers;
        private List<MemberBase<TSource>> _remainingMembers = new();
        public Func<SerializedData, ILoader, object> OnInstantiate { get; private set; } = null;

        List<object> _memberStorageLocations;
        int totalMembers;

        public MemberwiseSerializationMapping()
        {
            //
            UseBaseTypeFactoryRecursive();
            IncludeBaseMembersRecursive();
        }

        public override SerializationMapping GetInstance()
        {
            var x = new MemberwiseSerializationMapping<TSource>()
            {
                Context = this.Context,
                _remainingMembers = this._remainingMembers.ToList(), // copy the list, but without copying the members.
                totalMembers = this._remainingMembers.Count,
                OnInstantiate = this.OnInstantiate,
            };
            x._remainingMembers.Reverse();
            return x;
        }

        /// <summary>
        /// Makes the deserialization use a custom factory method instead of <see cref="Activator.CreateInstance{T}()"/>.
        /// </summary>
        /// <remarks>
        /// The factory is only needed to create an instance, not to set its internal state. The state should be set using the members.
        /// </remarks>
        /// <param name="customFactory">The method used to create an instance of <typeparamref name="TSource"/> from its serialized representation.</param>
        public MemberwiseSerializationMapping<TSource> WithFactory( Func<SerializedData, ILoader, object> customFactory )
        {
            this.OnInstantiate = customFactory;
            return this;
        }

        /// <summary>
        /// Makes this type include the members of the specified base type in its serialization.
        /// </summary>
        private MemberwiseSerializationMapping<TSource> IncludeBaseMembersRecursive()
        {
            Type baseType = typeof( TSource ).BaseType;
            if( baseType == null )
                return this;

            SerializationMapping mapping = SerializationMappingRegistry.GetMappingOrNull( this.Context, baseType );

            if( mapping == null )
                return this;

            if( ReferenceEquals( mapping, this ) ) // mapping for `this` is a cached mapping of base type.
                return this;

            Type mappingType = mapping.GetType();

            if( mappingType.IsConstructedGenericType
             && mappingType.GetGenericTypeDefinition() == typeof( MemberwiseSerializationMapping<> ) )
            {
                Type mappedType = mappingType.GetGenericArguments().First();

                if( !mappedType.IsAssignableFrom( baseType ) )
                    return this;

                FieldInfo listField = mappingType.GetField( nameof( _remainingMembers ), BindingFlags.Instance | BindingFlags.NonPublic );

                IList mapping__members = listField.GetValue( mapping ) as IList;

                Type memberType = typeof( MemberBase<> ).MakeGenericType( mappedType );

                foreach( var member in mapping__members )
                {
                    // Would be nice to have this be flattened, instead of one layer of passthrough per inheritance level.
                    MethodInfo method = typeof( PassthroughMember<,> )
                        .MakeGenericType( typeof( TSource ), mappedType )
                        .GetMethod( nameof( PassthroughMember<object, object>.Create ), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

                    MemberBase<TSource> m = (MemberBase<TSource>)method.Invoke( null, new object[] { member } );

                    this._remainingMembers.Add( m );
                }
            }

            return this;
        }

        /// <summary>
        /// Makes the deserialization use the factory of the nearest base type of <typeparamref name="TSource"/>.
        /// </summary>
        private MemberwiseSerializationMapping<TSource> UseBaseTypeFactoryRecursive()
        {
            Type baseType = typeof( TSource ).BaseType;
            if( baseType == null )
                return this;

            SerializationMapping mapping = SerializationMappingRegistry.GetMappingOrNull( this.Context, baseType );

            if( mapping is IInstantiableSerializationMapping m )
            {
                this.OnInstantiate = m.OnInstantiate;
                return this;
            }

            return this;
        }

        //
        //  Mapping methods:
        //

        public override bool Save<T>( T obj, ref SerializedData data, ISaver s )
        {
            if( obj == null )
                return false;

            TSource sourceObj = (TSource)(object)obj;

            if( data == null )
            {
                data = new SerializedObject();

                data[KeyNames.ID] = s.RefMap.GetID( sourceObj ).SerializeGuid();
                data[KeyNames.TYPE] = obj.GetType().SerializeType();
            }

            for( int i = this._remainingMembers.Count - 1; i >= 0; i-- )
            {
                var member = this._remainingMembers[i];
                if( member.Save( sourceObj, data, s ) )
                {
                    this._remainingMembers.RemoveAt( i );
                }

                if( s.ShouldPause() )
                {
                    break;
                }
            }

            return this._remainingMembers.Count <= 0;
        }

        bool FactoryMembersReadyForInstantiation()
        {
            if( _factoryMembers == null )
                return true;

            foreach( var member in _factoryMembers )
            {
                if( _remainingMembers.Contains( member ) )
                    return false;
            }
            return true;
        }

        public override bool Load<T>( ref T obj, SerializedData data, ILoader l )
        {
            TSource obj2 = (TSource)(object)obj;

            // obj can be null here, this is normal.

            if( _memberStorageLocations == null )
            {
                _memberStorageLocations = new List<object>( totalMembers );
                for( int i = 0; i < totalMembers; i++ )
                {
                    _memberStorageLocations.Add( null );
                }
            }

            if( data != null )
            {
                for( int i = this._remainingMembers.Count - 1; i >= 0; i-- )
                {
                    // Instantiate the object that contains the members ('parent'), if available.
                    if( obj2 == null && FactoryMembersReadyForInstantiation() ) // ready for instantiation means all members that'll be passed into the factory are fully deserialized.
                    {
                        obj2 = Instantiate( data, l );
                    }

#warning TODO - We can store directly to source object if the member is primitive.
                    var member = this._remainingMembers[i];

                    object memberLoc = this._remainingMembers[i];

                    if( member.Load( ref memberLoc, data, l ) )
                    {
                        member.Assign( ref obj2, memberLoc );
                        this._remainingMembers.RemoveAt( i );
                        this._memberStorageLocations.RemoveAt( i );
                    }
                    else
                    {
                        _memberStorageLocations[i] = member; // update it back
                    }

                    if( l.ShouldPause() )
                    {
                        break;
                    }
                }
            }

            obj = (T)(object)obj2;

            return this._remainingMembers.Count <= 0;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource Instantiate( SerializedData data, ILoader l )
        {
            TSource obj;
            if( OnInstantiate == null )
            {
                if( data == null )
                    return default;

                obj = Activator.CreateInstance<TSource>();
                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.RefMap.SetObj( id.DeserializeGuid(), obj );
                }
            }
            else
            {
                obj = (TSource)OnInstantiate.Invoke( data, l );
            }

            return obj;
        }

        //

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, Expression<Func<TSource, TMember>> member )
        {
            this._remainingMembers.Add( new Member<TSource, TMember>( serializedName, member ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, int context, Expression<Func<TSource, TMember>> member )
        {
            this._remainingMembers.Add( new Member<TSource, TMember>( serializedName, member ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            this._remainingMembers.Add( new Member<TSource, TMember>( serializedName, getter, setter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, int context, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            this._remainingMembers.Add( new Member<TSource, TMember>( serializedName, context, getter, setter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            this._remainingMembers.Add( new Member<TSource, TMember>( serializedName, getter, setter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, int context, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            this._remainingMembers.Add( new Member<TSource, TMember>( serializedName, context, getter, setter ) );
            return this;
        }

        //

        public MemberwiseSerializationMapping<TSource> WithFactory( Func<object> factory )
        {
            // factory invoked immediately, the type is fully mutable,
            // just instantiated with a function instead of a parameterless constructor.

            throw new NotImplementedException();
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3>( Func<TMember1, TMember2, TMember3, object> factory )
        {
            // factory is invoked with all members.
            _factoryMembers = this._remainingMembers.ToArray();
            throw new NotImplementedException();
            //OnInstantiate = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember>( string memberName, Func<TMember, object> factory )
        {
            // factory is invoked once all the specified members are created.
            // members are created in the order they're added by default.
            throw new NotImplementedException();
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2>( Func<TMember1, TMember2, object> factory )
        {
            throw new NotImplementedException();
            return this;
        }
    }
}