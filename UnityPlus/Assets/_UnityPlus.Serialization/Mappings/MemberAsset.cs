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
        private readonly Getter<TSource, TMember> _getter;
        private readonly Setter<TSource, TMember> _setter;

        /// <param name="member">Example: `o => o.sharedMesh`.</param>
        public MemberAsset( Expression<Func<TSource, TMember>> member )
        {
            _getter = AccessorUtils.CreateGetter( member );
            _setter = AccessorUtils.CreateSetter( member );
        }

        public SerializedData Save( TSource source, IReverseReferenceMap s )
        {
            var member = _getter.Invoke( source );

            return s.WriteAssetReference( member );
        }

        public void Load( ref TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            var newMemberValue = l.ReadAssetReference<TMember>( memberData );
            _setter.Invoke( ref source, newMemberValue );
        }
    }


    /// <summary>
    /// Serializes a member of type <typeparamref name="TMember"/>, that is referenced by <typeparamref name="TSource"/>.
    /// </summary>
    /// <typeparam name="TSource">The type that contains the member.</typeparam>
    /// <typeparam name="TMember">The type of the member (field/property/etc) that contains the reference.</typeparam>
    public class MemberAssetArray<TSource, TMember> : MemberBase<TSource>, IMappedMember<TSource> where TMember : class
    {
#warning TODO - kinda ugly having a separate member type just for arrays of assets.

        private readonly Getter<TSource, TMember[]> _getter;
        private readonly Setter<TSource, TMember[]> _setter;

        /// <param name="member">Example: `o => o.thrustTransform`.</param>
        public MemberAssetArray( Expression<Func<TSource, TMember[]>> member )
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

                var data = s.WriteAssetReference( value );

                serializedArray.Add( data );
            }

            return serializedArray;
        }

        public void Load( ref TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            SerializedArray serializedArray = (SerializedArray)memberData;

            TMember[] newMemberValue = new TMember[serializedArray.Count];

            for( int i = 0; i < serializedArray.Count; i++ )
            {
                SerializedData elementData = serializedArray[i];

                var element = l.ReadAssetReference<TMember>( elementData );
                newMemberValue[i] = element;
            }

            _setter.Invoke( ref source, newMemberValue );
        }
    }
}