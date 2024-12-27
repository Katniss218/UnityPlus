using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UnityPlus.Serialization
{
    public interface IMemberwiseTemp
    {
        Func<SerializedData, ILoader, object> _rawFactory { get; }
    }

    /// <summary>
    /// Creates a <see cref="SerializedObject"/> from the child mappings.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public sealed class MemberwiseSerializationMapping<TSource> : SerializationMapping, IMemberwiseTemp
    {
        private List<MemberBase<TSource>> _remainingMembers = new();

        List<object> _memberStorageLocations;
        int totalMembers;

        public Func<SerializedData, ILoader, object> _rawFactory { get; set; } = null;
        private MemberBase<TSource>[] _factoryMembers = null;
        Delegate _untypedFactory = null;

        public MemberwiseSerializationMapping()
        {
            UseBaseTypeFactoryRecursive();
            IncludeBaseMembersRecursive();
        }

        private MemberwiseSerializationMapping( MemberwiseSerializationMapping<TSource> copy )
        {
            this.Context = copy.Context;
            this._remainingMembers = copy._remainingMembers.ToList(); // copy the list, but without copying the members.
            this.totalMembers = copy._remainingMembers.Count;
            this._rawFactory = copy._rawFactory;
            this._factoryMembers = copy._factoryMembers;
            this._untypedFactory = copy._untypedFactory;
        }

        public override SerializationMapping GetInstance()
        {
            return new MemberwiseSerializationMapping<TSource>( this );
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

#warning TODO - we need reflection for that.
            if( mapping is IMemberwiseTemp m )
            {
                this._rawFactory = m._rawFactory;
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
            {
                return true;
            }

            TSource sourceObj = (TSource)(object)obj;

            if( data == null )
            {
                data = new SerializedObject();

                data[KeyNames.ID] = s.RefMap.GetID( sourceObj ).SerializeGuid();
                data[KeyNames.TYPE] = obj.GetType().SerializeType();
            }

            for( int i = 0; i < this._remainingMembers.Count; i++ )
            {
                var member = this._remainingMembers[i];
                if( member.Save( sourceObj, data, s ) )
                {
                    this._remainingMembers.RemoveAt( i );
                    i--;
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
            if( data == null || _remainingMembers.Count == 0 )
            {
                return true;
            }

            TSource obj2 = (obj == null) ? default : (TSource)(object)obj;

            // obj can be null here, this is normal.

            if( _memberStorageLocations == null )
            {
                _memberStorageLocations = new List<object>( totalMembers );
                for( int i = 0; i < totalMembers; i++ )
                {
                    _memberStorageLocations.Add( null );
                }
            }

            for( int i = 0; i < this._remainingMembers.Count; i++ )
            {
                // Instantiate the object that contains the members ('parent'), if available.
                if( obj2 == null && FactoryMembersReadyForInstantiation() ) // ready for instantiation means all members that'll be passed into the factory are fully deserialized.
                {
                    obj2 = Instantiate( data, l );
                }

#warning TODO - We can store directly to source object if the object exists.
                var member = this._remainingMembers[i];

                object memberLoc = this._remainingMembers[i];
                int storageIndex = totalMembers - (_remainingMembers.Count - i);

#warning TODO - keep the members until the object is created.
                if( member.Load( ref memberLoc, data, l ) )
                {
                    member.Assign( ref obj2, memberLoc );
                    this._remainingMembers.RemoveAt( i );
                    //this._memberStorageLocations.RemoveAt( storageIndex ); no need to remove this actually.
                    i--;
                }
                else
                {
                    _memberStorageLocations[storageIndex] = member; // update it back
                }

                if( l.ShouldPause() )
                {
                    break;
                }
            }

            obj = (T)(object)obj2;

            return this._remainingMembers.Count <= 0;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource Instantiate( SerializedData data, ILoader l )
        {
            TSource obj;
            if( _untypedFactory != null )
            {
                obj = (TSource)_untypedFactory.DynamicInvoke( _memberStorageLocations.ToArray() );
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
                if( data.TryGetValue( KeyNames.ID, out var id ) )
                {
                    l.RefMap.SetObj( id.DeserializeGuid(), obj );
                }
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
            this._remainingMembers.Add( new Member<TSource, TMember>( serializedName, context, member ) );
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

        /// <summary>
        /// Makes the deserialization use a custom factory method instead of <see cref="Activator.CreateInstance{T}()"/>.
        /// </summary>
        /// <remarks>
        /// The factory is only needed to create an instance, not to set its internal state. The state should be set using the members.
        /// </remarks>
        /// <param name="customFactory">The method used to create an instance of <typeparamref name="TSource"/> from its serialized representation.</param>
        public MemberwiseSerializationMapping<TSource> WithRawFactory( Func<SerializedData, ILoader, object> customFactory )
        {
            this._rawFactory = customFactory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory( Func<object> factory )
        {
            // factory invoked immediately, the type is fully mutable,
            // just instantiated with a function instead of a parameterless constructor.
            _untypedFactory = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1>( Func<TMember1, object> factory )
        {
            _factoryMembers = this._remainingMembers.ToArray();
            _untypedFactory = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2>( Func<TMember1, TMember2, object> factory )
        {
            _factoryMembers = this._remainingMembers.ToArray();
            _untypedFactory = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3>( Func<TMember1, TMember2, TMember3, object> factory )
        {
            // factory is invoked with all members.
            _factoryMembers = this._remainingMembers.ToArray();
            _untypedFactory = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4>( Func<TMember1, TMember2, TMember3, TMember4, object> factory )
        {
            // factory is invoked with all members.
            _factoryMembers = this._remainingMembers.ToArray();
            _untypedFactory = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1>( string member1Name, Func<TMember1, object> factory )
        {
            // factory is invoked once all the specified members are created.
            // members are created in the order they're added by default.
            _factoryMembers = this._remainingMembers.ToArray();
            //OnInstantiate = factory;
            throw new NotImplementedException();
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2>( string member1Name, string member2Name, Func<TMember1, TMember2, object> factory )
        {
            _factoryMembers = this._remainingMembers.ToArray();
            //OnInstantiate = factory;
            throw new NotImplementedException();
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3>( string member1Name, string member2Name, string member3Name, Func<TMember1, TMember2, TMember3, object> factory )
        {
            _factoryMembers = this._remainingMembers.ToArray();
            //OnInstantiate = factory;
            throw new NotImplementedException();
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4>( string member1Name, string member2Name, string member3Name, string member4Name, Func<TMember1, TMember2, TMember3, TMember4, object> factory )
        {
            _factoryMembers = this._remainingMembers.ToArray();
            //OnInstantiate = factory;
            throw new NotImplementedException();
            return this;
        }
    }
}