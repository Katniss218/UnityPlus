using System;
using System.Linq.Expressions;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Serializes a member of type <typeparamref name="TMember"/>, that is referenced by <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that contains the member.</typeparam>
    /// <typeparam name="TMember">The type of the member (field/property/etc) that contains the reference.</typeparam>
    public class MemberReference<TSource, TMember> : MemberBase<TSource>, IMappedReferenceMember<TSource> where TMember : class
    {
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

        /// <param name="member">Example: `o => o.thrustTransform`.</param>
        public MemberReference( Expression<Func<TSource, TMember>> member )
        {
            _getter = AccessorUtils.CreateGetter( member );
            _setter = AccessorUtils.CreateSetter( member );
        }

        public SerializedData Save( TSource source, IReverseReferenceMap s )
        {
            var member = _getter.Invoke( source );

            return s.WriteObjectReference( member );
        }

        public void LoadReferences( TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            var newMemberValue = l.ReadObjectReference<TMember>( memberData );

            _setter.Invoke( source, newMemberValue );
        }
    }
}