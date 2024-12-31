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

            if( anyProgressed )
                return MappingResult.Progressed;

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
        private bool _objectHasBeenInstantiated;

        object[] _factoryMemberStorage;
        int _startMember;
        Dictionary<int, (object, SerializationMapping)> _retryMembers;

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

            //
            //      RETRY PREVIOUSLY FAILED MEMBERS
            //

            if( _retryMembers != null )
            {
                List<int> retryMembersThatSucceededThisTime = new();

                foreach( (int i, (object memberObj, SerializationMapping mapping)) in _retryMembers )
                {
                    MemberBase<TSource> member = this._members[i];

                    MappingResult memberResult = member.SaveRetry( memberObj, mapping, data, s );
                    switch( memberResult )
                    {
                        case MappingResult.Finished:
                            retryMembersThatSucceededThisTime.Add( i );
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

                foreach( var i in retryMembersThatSucceededThisTime )
                {
                    _retryMembers.Remove( i );
                }
            }

            //
            //      PROCESS THE MEMBERS THAT HAVE NOT FAILED YET.
            //

            for( int i = _startMember; i < this._members.Count; i++ )
            {
                MemberBase<TSource> member = this._members[i];
                MappingResult memberResult = member.Save( sourceObj, data, s, out var mapping, out var memberObj );
                switch( memberResult )
                {
                    case MappingResult.Finished:
                        _startMember = i + 1;
                        anyFinished = true;
                        break;
                    case MappingResult.Failed:
                        _retryMembers ??= new();
                        _retryMembers.Add( i, (memberObj, mapping) );
                        anyFailed = true;
                        break;
                    case MappingResult.Progressed:
                        _retryMembers ??= new();
                        _retryMembers.Add( i, (memberObj, mapping) );
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

            if( _startMember <= _factoryMembers.Length - 1 )
            {
                return false;
            }

            if( _retryMembers != null )
            {
                foreach( var i in _retryMembers.Keys )
                {
                    if( i <= _factoryMembers.Length - 1 )
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

            TSource sourceObj = (obj == null) ? default : (TSource)(object)obj;

            // obj can be null here, this is normal.

            // Instantiate the object that contains the members ('parent'), if available.
            // It stores when the factory is invoked instead of checking for null,
            //   because structs are never null, but they may be immutable.
            if( !_objectHasBeenInstantiated && FactoryMembersReadyForInstantiation() )
            {
                sourceObj = Instantiate( data, l );
                _objectHasBeenInstantiated = true;
            }

            if( _factoryMembers != null )
            {
                _factoryMemberStorage ??= new object[_factoryMembers.Length];
            }
#warning TODO - populate thinks that the object hasn't been instantiated yet and re-instantiates it.

            bool anyFailed = false;
            bool anyFinished = false;
            bool anyProgressed = false;

            //
            //      RETRY PREVIOUSLY FAILED MEMBERS
            //

            if( _retryMembers != null )
            {
                List<int> retryMembersThatSucceededThisTime = new();

                foreach( (int i, (object o, SerializationMapping m)) in _retryMembers )
                {
                    MemberBase<TSource> member = this._members[i];

                    object memberObj = o;
                    MappingResult memberResult = member.LoadRetry( ref memberObj, m, data, l );
                    switch( memberResult )
                    {
                        case MappingResult.Finished:
                            retryMembersThatSucceededThisTime.Add( i );
                            //_retryMembers.Remove( i );
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
                        _factoryMemberStorage[i] = memberObj;

                        sourceObj = Instantiate( data, l );
                        // assign the initial members (if members are readonly this will silently do nothing).
                        for( int j = 0; j < _factoryMemberStorage.Length; j++ )
                        {
                            _members[j].Set( ref sourceObj, this._factoryMemberStorage[j] );
                        }
                        _objectHasBeenInstantiated = true;
                    }

                    // Store the member for later in case the object doesn't exist yet.
                    if( _objectHasBeenInstantiated )
                    {
                        member.Set( ref sourceObj, memberObj );
                    }
                    else
                    {
                        _factoryMemberStorage[i] = memberObj;
                    }

                    if( l.ShouldPause() )
                    {
                        if( !anyFailed ) // On pause, if everything else has finished, replace the aggregate finished with progressed, since there's more to do later.
                            anyProgressed = true;
                        break;
                    }
                }

                foreach( var i in retryMembersThatSucceededThisTime )
                {
                    _retryMembers.Remove( i );
                }
            }

            //
            //      PROCESS THE MEMBERS THAT HAVE NOT FAILED YET.
            //

            for( int i = _startMember; i < this._members.Count; i++ )
            {
                MemberBase<TSource> member = this._members[i];

                object memberObj = default;
#warning TODO - pass in info if object is instantiated and write directly to it.
                MappingResult memberResult = member.Load( ref memberObj, data, l, out var mapping );
                switch( memberResult )
                {
                    case MappingResult.Finished:
                        _startMember = i + 1;
                        anyFinished = true;
                        break;
                    case MappingResult.Failed:
                        _retryMembers ??= new();
                        _retryMembers.Add( i, (memberObj, mapping) );
                        anyFailed = true;
                        break;
                    case MappingResult.Progressed:
                        _retryMembers ??= new();
                        _retryMembers.Add( i, (memberObj, mapping) );
                        anyProgressed = true;
                        break;
                }

                // Instantiate the object that contains the members ('parent'), if available.
                // It stores when the factory is invoked instead of checking for null,
                //   because structs are never null, but they may be immutable.
                if( !_objectHasBeenInstantiated && FactoryMembersReadyForInstantiation() )
                {
                    _factoryMemberStorage[i] = memberObj;
                    sourceObj = Instantiate( data, l );
                    // assign the initial members (if members are readonly this will silently do nothing).
                    for( int j = 0; j < _factoryMemberStorage.Length; j++ )
                    {
                        _members[j].Set( ref sourceObj, this._factoryMemberStorage[j] );
                    }
                    _objectHasBeenInstantiated = true;
                }

                // Store the member for later in case the object doesn't exist yet.
                if( _objectHasBeenInstantiated )
                {
                    member.Set( ref sourceObj, memberObj );
                }
                else
                {
                    _factoryMemberStorage[i] = memberObj;
                }

                if( l.ShouldPause() )
                {
                    if( !anyFailed ) // On pause, if everything else has finished, replace the aggregate finished with progressed, since there's more to do later.
                        anyProgressed = true;
                    break;
                }
            }

            obj = (T)(object)sourceObj;

            return MappingResult_Ex.GetCompoundResult( anyFailed, anyFinished, anyProgressed );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        TSource Instantiate( SerializedData data, ILoader l )
        {
            TSource obj;
            if( _untypedFactory != null )
            {
                obj = (TSource)_untypedFactory.DynamicInvoke( _factoryMemberStorage );
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

        public MemberwiseSerializationMapping<TSource> WithReadonlyMember<TMember>( string serializedName, Getter<TSource, TMember> getter )
        {
            this._members.Add( new Member<TSource, TMember>( serializedName, ObjectContext.Default, getter ) );
            return this;
        }

        public MemberwiseSerializationMapping<TSource> WithReadonlyMember<TMember>( string serializedName, int context, Getter<TSource, TMember> getter )
        {
            this._members.Add( new Member<TSource, TMember>( serializedName, context, getter ) );
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

#warning TODO - named factories.
        /*
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
        }*/
    }
}