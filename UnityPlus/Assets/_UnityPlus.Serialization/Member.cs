using System;
using System.Linq.Expressions;
using UnityEngine.Networking.Types;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Serializes a member of type <typeparamref name="TMember"/>, that belongs to a type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that contains the member.</typeparam>
    /// <typeparam name="TMember">The type of the member (field/property/etc).</typeparam>
    public sealed class Member<TSource, TMember> : MemberBase<TSource>
    {
        private readonly int _context = ObjectContext.Default;

        private readonly Getter<TSource, TMember> _getter;
        private readonly Setter<TSource, TMember> _setter;
        private readonly RefSetter<TSource, TMember> _structSetter;

        private readonly Expression<Func<TSource, TMember>> _memberAccessExpr;

        public string Name { get; }

        /// <summary>
        /// Checks if the member serialization represents a simple member access (object.member = value), as opposed to something more complicated.
        /// </summary>
        public bool IsSimpleAccess => _memberAccessExpr != null;

        /// <summary>
        /// Gets the serialization context that this member should use.
        /// </summary>
        public int Context => _context;

        private bool _hasCachedMapping;
        private SerializationMapping _cachedMapping;

        private void TryCacheMemberMapping()
        {
            Type type = typeof( TMember );
            if( type.IsValueType || (!type.IsInterface && type.BaseType == null) )
            {
                var mapping1 = SerializationMappingRegistry.GetMappingOrNull( _context, typeof( TMember ) );
                var mapping2 = mapping1.GetInstance();
                if( object.ReferenceEquals( mapping1, mapping2 ) ) // This is needed due to GetInstance and mappings that can hold state (like the dict mapping).
                {
                    _hasCachedMapping = true;
                    _cachedMapping = mapping1;
                }
            }
        }

        public override MemberBase<TSource> Copy()
        {
            return (MemberBase<TSource>)this.MemberwiseClone();
        }

        // expression constructors

        /// <param name="member">Example: `o => o.position`.</param>
        public Member( string name, Expression<Func<TSource, TMember>> member )
        {
            this.Name = name;
            _memberAccessExpr = member;
            TryCacheMemberMapping();
            _getter = AccessorUtils.CreateGetter( member );

            if( typeof( TSource ).IsValueType )
                _structSetter = AccessorUtils.CreateStructSetter( member );
            else
                _setter = AccessorUtils.CreateSetter( member );
        }

        /// <param name="member">Example: `o => o.position`.</param>
        public Member( string name, int context, Expression<Func<TSource, TMember>> member )
        {
            this.Name = name;
            _memberAccessExpr = member;
            _context = context;
            TryCacheMemberMapping();
            _getter = AccessorUtils.CreateGetter( member );

            if( typeof( TSource ).IsValueType )
                _structSetter = AccessorUtils.CreateStructSetter( member );
            else
                _setter = AccessorUtils.CreateSetter( member );
        }

        // custom getter/setter constructors

        public Member( string name, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            if( typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"Member `{typeof( TSource ).FullName}` This constructor can only be used with a reference type TSource." );

            this.Name = name;
            _memberAccessExpr = null;
            TryCacheMemberMapping();
            _getter = getter;
            _setter = setter;
        }

        public Member( string name, int context, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            if( typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"Member `{typeof( TSource ).FullName}` This constructor can only be used with a reference type TSource." );

            this.Name = name;
            _memberAccessExpr = null;
            _context = context;
            TryCacheMemberMapping();
            _getter = getter;
            _setter = setter;
        }

        public Member( string name, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            if( !typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"Member `{typeof( TSource ).FullName}` This constructor can only be used with a value type TSource." );

            this.Name = name;
            _memberAccessExpr = null;
            TryCacheMemberMapping();
            _getter = getter;
            _structSetter = setter;
        }

        public Member( string name, int context, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            if( !typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"Member `{typeof( TSource ).FullName}` This constructor can only be used with a value type TSource." );

            this.Name = name;
            _memberAccessExpr = null;
            _context = context;
            TryCacheMemberMapping();
            _getter = getter;
            _structSetter = setter;
        }

        //
        //  Logic
        //

        //TMember _member;
        bool isDone;

        public override bool Save( TSource source, SerializedData sourceData, ISaver s )
        {
            TMember member = _getter.Invoke( source );

            var mapping = SerializationMappingRegistry.GetMapping<TMember>( _context, member );

            if( !sourceData.TryGetValue( Name, out var data ) )
                data = null;
            var ret = mapping.SafeSave( member, ref data, s );
            sourceData[Name] = data;

            return ret;
        }

        public override bool Load( ref object member, SerializedData sourceData, ILoader l )
        {
            sourceData.TryGetValue( Name, out SerializedData data ); // data can be null, that's okay.

            Type memberType = typeof( TMember );
            if( data != null && data.TryGetValue( KeyNames.TYPE, out var type ) )
            {
                memberType = type.DeserializeType();
            }

            SerializationMapping mapping;
            if( _hasCachedMapping )
            {
                mapping = _cachedMapping;
                if( data != null )
                {
                    l.MappingCache[data] = mapping;
                }
            }
            else
            {
                mapping = MappingHelper.GetMapping_Load<TMember>( _context, memberType, data, l );
            }

            TMember member2 = default;
            var isFullyLoaded = mapping.SafeLoad<TMember>( ref member2, data, l );
            member = member2;
            return isFullyLoaded;
        }

        public override void Assign( ref TSource source, object member )
        {
            if( _structSetter == null )
                _setter.Invoke( source, (TMember)member );
            else
                _structSetter.Invoke( ref source, (TMember)member );
        }
    }
}