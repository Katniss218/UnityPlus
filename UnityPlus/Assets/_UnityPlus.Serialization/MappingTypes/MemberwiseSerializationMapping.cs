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
    /// Represents the state of a save/load invocation on a mapping.
    /// </summary>
    public enum MappingResult : byte
    {
        Finished = 0,
        Failed,
        Progressed,
        NoChange
    }

    public static class MappingResult_Ex
    {
        public static MappingResult GetCompoundResult( bool anyFailed, bool anyFinished, bool anyProgressed )
        {
            if( anyFinished )
            {
                if( !anyFailed && !anyProgressed )
                    return MappingResult.Finished;
                else                // some finished but not all
                    return MappingResult.Progressed;
            }
            if( !anyFinished && !anyProgressed )    // none finished and none progressed - all failed (or mix of failed and no change, meaning failed).
                return MappingResult.Failed;

            return MappingResult.NoChange;
        }
    }

    /// <summary>
    /// Creates a <see cref="SerializedObject"/> from the child mappings.
    /// </summary>
    /// <typeparam name="TSource">The type of the object being mapped.</typeparam>
    public sealed class MemberwiseSerializationMapping<TSource> : SerializationMapping, IMemberwiseTemp
    {
        private List<MemberBase<TSource>> _members = new();
        private bool[] _finishedMembers; // Generally seems to be faster than copying the 'remaining' members list and removing members from it.
        private bool _objectHasBeenInstantiated;

        object[] _memberStorageLocations;

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
            this._members = copy._members;
            this._finishedMembers = new bool[copy._members.Count];
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

                FieldInfo listField = mappingType.GetField( nameof( _members ), BindingFlags.Instance | BindingFlags.NonPublic );

                IList mapping__members = listField.GetValue( mapping ) as IList;

                foreach( var member in mapping__members )
                {
                    // Would be nice to have this be flattened, instead of one layer of passthrough per inheritance level.
                    MethodInfo method = typeof( PassthroughMember<,> )
                        .MakeGenericType( typeof( TSource ), mappedType )
                        .GetMethod( nameof( PassthroughMember<object, object>.Create ), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic );

                    MemberBase<TSource> m = (MemberBase<TSource>)method.Invoke( null, new object[] { member } );

                    this._members.Add( m );
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

        public override MappingResult Save<T>( T obj, ref SerializedData data, ISaver s )
        {
            if( obj == null )
            {
                return MappingResult.Finished;
            }

            TSource sourceObj = (TSource)(object)obj;

            if( data == null )
            {
                data = new SerializedObject();

                data[KeyNames.ID] = s.RefMap.GetID( sourceObj ).SerializeGuid();
                data[KeyNames.TYPE] = obj.GetType().SerializeType();
            }

            bool anyFailed = false;
            bool anyFinished = false;
            bool anyProgressed = false;

            for( int i = 0; i < this._members.Count; i++ )
            {
                if( _finishedMembers[i] )
                    continue;

                var member = this._members[i];
                var memberResult = member.Save( sourceObj, data, s );
                switch( memberResult )
                {
                    case MappingResult.Finished:
                        _finishedMembers[i] = true;
                        anyFinished = true;
                        break;
                    case MappingResult.Failed:
                        anyFailed = true;
                        break;
                    case MappingResult.Progressed:
                        anyProgressed = true;
                        break;
                }

                if( s.ShouldPause() )
                {
                    if( !anyFailed ) // On pause, if everything else has finished, replace the aggregate finished with progressed, since there's more to do later.
                        anyProgressed = true;
                    break;
                }
            }

            return MappingResult_Ex.GetCompoundResult( anyFailed, anyFinished, anyProgressed );
        }

        bool FactoryMembersReadyForInstantiation()
        {
            if( _factoryMembers == null )
                return true;

            foreach( var member in _factoryMembers )
            {
                for( int i = 0; i < this._members.Count; i++ )
                {
                    if( this._members[i] != member )
                        continue;

                    if( !this._finishedMembers[i] )
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override MappingResult Load<T>( ref T obj, SerializedData data, ILoader l )
        {
            if( data == null )
            {
                return MappingResult.Finished;
            }

            TSource obj2 = (obj == null) ? default : (TSource)(object)obj;

            // obj can be null here, this is normal.

            // Instantiate the object that contains the members ('parent'), if available.
            // It stores when the factory is invoked instead of checking for null,
            //   because structs are never null, but they may be immutable.
            if( !_objectHasBeenInstantiated && FactoryMembersReadyForInstantiation() )
            {
                obj2 = Instantiate( data, l );
                _objectHasBeenInstantiated = true;
            }

            if( _memberStorageLocations == null )
            {
                _memberStorageLocations = new object[_members.Count];
            }

            bool anyFailed = false;
            bool anyFinished = false;
            bool anyProgressed = false;

            for( int i = 0; i < this._members.Count; i++ )
            {
                if( _finishedMembers[i] )
                    continue;

                var member = this._members[i];
                var memberResult = member.Load( ref this._memberStorageLocations[i], data, l );
                switch( memberResult )
                {
                    case MappingResult.Finished:
                        _finishedMembers[i] = true;
                        anyFinished = true;
                        break;
                    case MappingResult.Failed:
                        anyFailed = true;
                        break;
                    case MappingResult.Progressed:
                        anyProgressed = true;
                        break;
                }

                // Instantiate the object that contains the members ('parent'), if available.
                // It stores when the factory is invoked instead of checking for null,
                //   because structs are never null, but they may be immutable.
                if( !_objectHasBeenInstantiated && FactoryMembersReadyForInstantiation() )
                {
                    obj2 = Instantiate( data, l );
                    _objectHasBeenInstantiated = true;
                }

                if( l.ShouldPause() )
                {
                    if( !anyFailed ) // On pause, if everything else has finished, replace the aggregate finished with progressed, since there's more to do later.
                        anyProgressed = true;
                    break;
                }
            }

            var result = MappingResult_Ex.GetCompoundResult( anyFailed, anyFinished, anyProgressed );
            if( result == MappingResult.Finished )
            {
                // Assigning when everything was finished guarantees that the factory has been called.
                for( int i = 0; i < this._members.Count; i++ )
                {
                    _members[i].Assign( ref obj2, this._memberStorageLocations[i] );
                }
            }

            obj = (T)(object)obj2;

            return result;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource Instantiate( SerializedData data, ILoader l )
        {
            TSource obj;
            if( _untypedFactory != null )
            {
                obj = (TSource)_untypedFactory.DynamicInvoke( _memberStorageLocations.Take( _factoryMembers.Length ).ToArray() );
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

        //

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, Expression<Func<TSource, TMember>> member )
        {
            this._members.Add( new Member<TSource, TMember>( serializedName, ObjectContext.Default, member ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, int context, Expression<Func<TSource, TMember>> member )
        {
            this._members.Add( new Member<TSource, TMember>( serializedName, context, member ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            this._members.Add( new Member<TSource, TMember>( serializedName, ObjectContext.Default, getter, setter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, int context, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            this._members.Add( new Member<TSource, TMember>( serializedName, context, getter, setter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            this._members.Add( new Member<TSource, TMember>( serializedName, ObjectContext.Default, getter, setter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithMember<TMember>( string serializedName, int context, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            this._members.Add( new Member<TSource, TMember>( serializedName, context, getter, setter ) );
            return this;
        }

        //

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
            _factoryMembers = this._members.ToArray();
            _untypedFactory = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2>( Func<TMember1, TMember2, object> factory )
        {
            _factoryMembers = this._members.ToArray();
            _untypedFactory = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3>( Func<TMember1, TMember2, TMember3, object> factory )
        {
            // factory is invoked with all members.
            _factoryMembers = this._members.ToArray();
            _untypedFactory = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4>( Func<TMember1, TMember2, TMember3, TMember4, object> factory )
        {
            // factory is invoked with all members.
            _factoryMembers = this._members.ToArray();
            _untypedFactory = factory;
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1>( string member1Name, Func<TMember1, object> factory )
        {
            // factory is invoked once all the specified members are created.
            // members are created in the order they're added by default.
            _factoryMembers = this._members.ToArray();
            //OnInstantiate = factory;
            throw new NotImplementedException();
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2>( string member1Name, string member2Name, Func<TMember1, TMember2, object> factory )
        {
            _factoryMembers = this._members.ToArray();
            //OnInstantiate = factory;
            throw new NotImplementedException();
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3>( string member1Name, string member2Name, string member3Name, Func<TMember1, TMember2, TMember3, object> factory )
        {
            _factoryMembers = this._members.ToArray();
            //OnInstantiate = factory;
            throw new NotImplementedException();
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithFactory<TMember1, TMember2, TMember3, TMember4>( string member1Name, string member2Name, string member3Name, string member4Name, Func<TMember1, TMember2, TMember3, TMember4, object> factory )
        {
            _factoryMembers = this._members.ToArray();
            //OnInstantiate = factory;
            throw new NotImplementedException();
            return this;
        }
    }
}