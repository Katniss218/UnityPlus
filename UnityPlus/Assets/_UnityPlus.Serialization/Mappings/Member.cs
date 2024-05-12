using System;
using System.Linq.Expressions;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Serializes a member of type <typeparamref name="TMember"/>, that belongs to a type <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that contains the member.</typeparam>
    /// <typeparam name="TMember">The type of the member (field/property/etc).</typeparam>
    public class Member<TSource, TMember> : MemberBase<TSource>, IMappedMember<TSource>
    {
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

#warning TODO - caching of member mappings is possible for field/property types that don't have any types deriving from them (e.g. member of type `float`, GameObject, etc).

        /// <param name="member">Example: `o => o.position`.</param>
        public Member( Expression<Func<TSource, TMember>> member )
        {
            _getter = AccessorUtils.CreateGetter( member );
            _setter = AccessorUtils.CreateSetter( member );
        }

        public Member( Func<TSource, TMember> getter, Action<TSource, TMember> setter )
        {
            _getter = getter;
            _setter = setter;
        }

        public SerializedData Save( TSource source, IReverseReferenceMap s )
        {
            var member = _getter.Invoke( source );

            var mapping = SerializationMappingRegistry.GetMappingOrDefault<TMember>( member );

            return mapping.Save( member, s );
        }

        public void Load( TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            if( _setter == null )
                return;

            Type memberType = typeof( TMember );
            if( memberData.TryGetValue( KeyNames.TYPE, out var type ) )
            {
                memberType = type.ToType();
            }

            var mapping = SerializationMappingRegistry.GetMappingOrDefault<TMember>( memberType );

            var member = (TMember)mapping.Load( memberData, l );
            _setter.Invoke( source, member );
        }
    }
}