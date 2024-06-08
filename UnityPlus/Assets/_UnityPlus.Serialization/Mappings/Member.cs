using System;
using System.Linq.Expressions;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Serializes a member of type <typeparamref name="TMember"/>, that belongs to a type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that contains the member.</typeparam>
    /// <typeparam name="TMember">The type of the member (field/property/etc).</typeparam>
    public class Member<TSource, TMember> : MemberBase<TSource>//, IMappedMember<TSource>, IMappedReferenceMember<TSource>
    {
        private readonly int _context = default( int );

        private readonly Getter<TSource, TMember> _getter;
        private readonly Setter<TSource, TMember> _setter;
        private readonly RefSetter<TSource, TMember> _structSetter;

        private bool _isCacheable;
        private SerializationMapping _cachedMapping;

        private void TryCacheMemberMapping()
        {
            Type type = typeof( TMember );
            if( type.IsValueType
             || (!type.IsInterface && type.BaseType == null) )
            {
                _isCacheable = true;
                _cachedMapping = SerializationMappingRegistry.GetMappingOrEmpty( _context, typeof( TMember ) );
            }
        }

        // expression-constructors

        /// <param name="member">Example: `o => o.position`.</param>
        public Member( Expression<Func<TSource, TMember>> member )
        {
            TryCacheMemberMapping();
            _getter = AccessorUtils.CreateGetter( member );

            if( typeof( TSource ).IsValueType )
                _structSetter = AccessorUtils.CreateStructSetter( member );
            else
                _setter = AccessorUtils.CreateSetter( member );
        }

        /// <param name="member">Example: `o => o.position`.</param>
        public Member( int context, Expression<Func<TSource, TMember>> member )
        {
            _context = context;
            TryCacheMemberMapping();
            _getter = AccessorUtils.CreateGetter( member );

            if( typeof( TSource ).IsValueType )
                _structSetter = AccessorUtils.CreateStructSetter( member );
            else
                _setter = AccessorUtils.CreateSetter( member );
        }

        // class custom getters/setters

        public Member( Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            if( typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"[{typeof( TSource ).FullName}] Use the constructor with the value type setter." );

            TryCacheMemberMapping();
            _getter = getter;
            _setter = setter;
        }

        public Member( int context, Getter<TSource, TMember> getter, Setter<TSource, TMember> setter )
        {
            if( typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"[{typeof( TSource ).FullName}] Use the constructor with the value type setter." );

            _context = context;
            TryCacheMemberMapping();
            _getter = getter;
            _setter = setter;
        }

        // struct custom getters/setters

        public Member( Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            if( !typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"[{typeof( TSource ).FullName}] Use the constructor with the reference type setter." );

            TryCacheMemberMapping();
            _getter = getter;
            _structSetter = setter;
        }

        public Member( int context, Getter<TSource, TMember> getter, RefSetter<TSource, TMember> setter )
        {
            if( !typeof( TSource ).IsValueType )
                throw new InvalidOperationException( $"[{typeof( TSource ).FullName}] Use the constructor with the reference type setter." );

            _context = context;
            TryCacheMemberMapping();
            _getter = getter;
            _structSetter = setter;
        }

        //
        //  Logic
        //

        public override SerializedData Save( TSource source, ISaver s )
        {
            var member = _getter.Invoke( source );

            var mapping = SerializationMappingRegistry.GetMappingOrDefault<TMember>( _context, member );

            if( mapping.SerializationStyle != SerializationStyle.None )
                return mapping.Save( member, s );
            return (SerializedData)null;
        }

        // The public-facing methods on the SerializationUnit are like a member,
        //   but the member itself can't be populated, only the end user may choose to do that on the root object(s).

        public override void Load( ref TSource source, SerializedData data, ILoader l )
        {
            Type memberType = typeof( TMember );
            if( data.TryGetValue( KeyNames.TYPE, out var type ) )
            {
                memberType = type.DeserializeType();
            }

            var mapping = _isCacheable ? _cachedMapping : SerializationMappingRegistry.GetMappingOrDefault<TMember>( _context, memberType );

            TMember member = default;
            MappingHelper.DoLoad( mapping, ref member, data, l );

            if( _structSetter == null )
                _setter.Invoke( source, member );
            else
                _structSetter.Invoke( ref source, member );
        }

        public override void LoadReferences( ref TSource source, SerializedData data, ILoader l )
        {
            TMember member = _getter.Invoke( source );

            // This is needed, to reach the references nested inside objects that themselves don't contain any references.
            var mapping = SerializationMappingRegistry.GetMappingOrDefault<TMember>( _context, member );

            MappingHelper.DoLoadReferences( mapping, ref member, data, l );

            // This is needed, if the setter is custom (not auto-generated from field access (but NOT property access)) (look at LODGroup and its LOD[])
            // Basically, we don't have the guarantee that the class we have referenceequals the private state.
            if( _structSetter == null )
                _setter.Invoke( source, member );
            else
                _structSetter.Invoke( ref source, member );
        }
    }
}