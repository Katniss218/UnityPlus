using System;
using System.Linq.Expressions;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Serializes a member of type <typeparamref name="TMember"/>, that is an asset referenced by <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that contains the member.</typeparam>
    /// <typeparam name="TMember">The type of the member (field/property/etc).</typeparam>
    public class MemberAsset<TSource, TMember> : MemberBase<TSource>, IMappedMember<TSource> where TMember : class
    {
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

        /// <param name="member">Example: `o => o.sharedMesh`.</param>
        public MemberAsset( Expression<Func<TSource, TMember>> member )
        {
            _getter = AccessorUtils.CreateGetter( member );
            _setter = AccessorUtils.CreateSetter( member );
        }

        public SerializedData Save( TSource source, IReverseReferenceMap s )
        {
#warning TODO - support arrays and other. Same for MemberReference

            // using the existing array mapping will require it to support references.
            // maybe just have a single member type that'll handle everything?

#warning TODO - pass through how the member desires to save itself to the array / other mappings, in case the mapping represents a collection?
            // maybe instead of calling something to get the mapping and stuff, we could overwrite that for a specific member? so the array will call our function instead of being hardcoded.
            // Currently, array contains the same duplicated code that a normal member has.
            // And a list / dict, will probably contain very similar code.

            // or maybe even do another layer?

            var member = _getter.Invoke( source );

            return s.WriteAssetReference( member );
        }

        public void Load( TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            var newMemberValue = l.ReadAssetReference<TMember>( memberData );
            _setter.Invoke( source, newMemberValue );
        }
    }
}