using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps a member of an object of type <typeparamref name="TSource"/>.
    /// </summary>
    public abstract class MappedMember<TSource>
    {
    }

    internal interface IMappedMember<TSource>
    {
        /// <summary>
        /// Saves the member, and returns the <see cref="SerializedData"/> representing it.
        /// </summary>
        SerializedData Save( TSource source, IReverseReferenceMap s );

        /// <summary>
        /// Instantiates the member from <see cref="SerializedData"/> using the most appropriate mapping for the member type and serialized object's '$type', and assigns it to the member.
        /// </summary>
        void Load( TSource source, SerializedData data, IForwardReferenceMap l );
    }

    internal interface IMappedReferenceMember<TSource>
    {
        SerializedData Save( TSource source, IReverseReferenceMap s );

        void LoadReferences( TSource source, SerializedData data, IForwardReferenceMap l );
    }

    public class Member<TSource, TMember> : MappedMember<TSource>, IMappedMember<TSource>
    {
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

#warning TODO - caching of member mappings is possible for field/property types that don't have any types deriving from them (e.g. member of type `float`, GameObject, etc).

        public Member( Expression<Func<TSource, TMember>> member )
        {
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
        }

        public Member( Func<TSource, TMember> getter, Action<TSource, TMember> setter )
        {
            _getter = getter;
            _setter = setter;
        }

        public SerializedData Save( TSource source, IReverseReferenceMap s )
        {
            var member = _getter.Invoke( source );

            var mapping = SerializationMapping.GetMappingFor( member );

            return mapping.Save( member, s );
        }

        public void Load( TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            Type memberType = typeof( TMember );
            if( memberData.TryGetValue( KeyNames.TYPE, out var type ) )
            {
                memberType = type.ToType();
            }

            var mapping = SerializationMapping.GetMappingFor<TMember>( memberType );

            var member = (TMember)mapping.Load( memberData, l );
            _setter.Invoke( source, member );
        }
    }

    public class MemberAsset<TSource, TMember> : MappedMember<TSource>, IMappedMember<TSource> where TMember : class
    {
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

        public MemberAsset( Expression<Func<TSource, TMember>> member )
        {
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
        }


        public SerializedData Save( TSource source, IReverseReferenceMap s )
        {
            var member = _getter.Invoke( source );

            return s.WriteAssetReference( member );
        }

        public void Load( TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            var newMemberValue = l.ReadAssetReference<TMember>( memberData );
            _setter.Invoke( source, newMemberValue );
        }
    }

    
    public class MemberReference<TSource, TMember> : MappedMember<TSource>, IMappedReferenceMember<TSource> where TMember : class
    {
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

        public MemberReference( Expression<Func<TSource, TMember>> member )
        {
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
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