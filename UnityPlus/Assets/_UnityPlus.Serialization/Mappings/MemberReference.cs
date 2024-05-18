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
        private readonly Getter<TSource, TMember> _getter;
        private readonly Setter<TSource, TMember> _setter;

        /// <param name="member">Example: `o => o.thrustTransform`.</param>
        public MemberReference( Expression<Func<TSource, TMember>> member )
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
            // Basically, we need per-member data, as well as per-type data. Not just member overriding type completely.

            var member = _getter.Invoke( source );

            return s.WriteObjectReference( member );
        }

        public void LoadReferences( ref TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            var newMemberValue = l.ReadObjectReference<TMember>( memberData );

            _setter.Invoke( ref source, newMemberValue );
        }
    }

    /// <summary>
    /// Serializes a member of type <typeparamref name="TMember"/>, that is referenced by <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that contains the member.</typeparam>
    /// <typeparam name="TMember">The type of the member (field/property/etc) that contains the reference.</typeparam>
    public class MemberReferenceArray<TSource, TMember> : MemberBase<TSource>, IMappedReferenceMember<TSource> where TMember : class
    {
#warning TODO - kinda ugly having a separate member type just for arrays of references.

        private readonly Getter<TSource, TMember[]> _getter;
        private readonly Setter<TSource, TMember[]> _setter;

        /// <param name="member">Example: `o => o.thrustTransform`.</param>
        public MemberReferenceArray( Expression<Func<TSource, TMember[]>> member )
        {
            _getter = AccessorUtils.CreateGetter( member );
            _setter = AccessorUtils.CreateSetter( member );
        }

        public SerializedData Save( TSource source, IReverseReferenceMap s )
        {
            var member = _getter.Invoke( source );

            SerializedArray serializedArray = new SerializedArray();
            for( int i = 0; i < member.Length; i++ )
            {
                TMember value = member[i];

                var data = s.WriteObjectReference( value );

                serializedArray.Add( data );
            }

            return serializedArray;
        }

        public void LoadReferences( ref TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            SerializedArray serializedArray = (SerializedArray)memberData;

            TMember[] newMemberValue = new TMember[serializedArray.Count];

            for( int i = 0; i < serializedArray.Count; i++ )
            {
                SerializedData elementData = serializedArray[i];

                var element = l.ReadObjectReference<TMember>( elementData );
                newMemberValue[i] = element;
            }

            _setter.Invoke( ref source, newMemberValue );
        }
    }
}