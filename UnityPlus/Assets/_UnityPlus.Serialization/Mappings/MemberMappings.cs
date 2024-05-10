using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization
{
    // saving is easy, can be done in any order.

    // the general idea for loading would be to traverse the tree twice.
    // once to create every object that may referenced somewhere (class).
    // and once more to fill in the members of those objects.

#warning TODO - determine what is referencable.


#warning TODO - only references aren't owned by the type. i.e. only references might not have a setobjects/asobject pass.

    /// <summary>
    /// Maps a member of an object of type <typeparamref name="TSource"/>.
    /// </summary>
    public abstract class MemberMapping<TSource>
    {
    }

    internal interface IObjectMapping<TSource>
    {
        SerializedObject GetObjectsPass( TSource source, IReverseReferenceMap s );

        void SetObjectsPass( TSource source, SerializedObject data, IForwardReferenceMap l );
    }

    internal interface IDataMapping<TSource>
    {
        SerializedData GetDataPass( TSource source, IReverseReferenceMap s );

        void SetDataPass( TSource source, SerializedData data, IForwardReferenceMap l );
    }

    public class MemberData<TSource, TMember> : MemberMapping<TSource>, IDataMapping<TSource>
    {
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

#warning TODO - caching of member mappings is possible for field/property types that don't have any types deriving from them (e.g. member of type `float`, GameObject, etc).

        public MemberData( Expression<Func<TSource, TMember>> member )
        {
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
        }

        public MemberData( Func<TSource, TMember> getter, Action<TSource, TMember> setter )
        {
            _getter = getter;
            _setter = setter;
        }

        public SerializedData GetDataPass( TSource source, IReverseReferenceMap s )
        {
            var member = _getter.Invoke( source );

            var mapping = (IDataMapping<TMember>)SerializationMapping.GetMappingFor( member );

            return mapping.GetDataPass( member, s );
        }

        public void SetDataPass( TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            var member = _getter.Invoke( source );

            var mapping = (IDataMapping<TMember>)SerializationMapping.GetMappingFor( member );

            mapping.SetDataPass( member, memberData, l ); // SetData doesn't assign anything, it retrieves the member, and calls setdata further, on it.
#warning TODO - it should assign, the entire setting stuff should probably be reworked.
        }
    }

    public class MemberAsset<TSource, TMember> : MemberMapping<TSource>, IDataMapping<TSource> where TMember : class
    {
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

        public MemberAsset( Expression<Func<TSource, TMember>> member )
        {
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
        }

        public SerializedData GetDataPass( TSource source, IReverseReferenceMap s )
        {
            var member = _getter.Invoke( source );

            return s.WriteAssetReference( member );
        }

        public void SetDataPass( TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            var newMemberValue = l.ReadAssetReference<TMember>( memberData );
            _setter.Invoke( source, newMemberValue );
        }
    }

    public class MemberReference<TSource, TMember> : MemberMapping<TSource>, IDataMapping<TSource> where TMember : class
    {
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;

        public MemberReference( Expression<Func<TSource, TMember>> member )
        {
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
        }

        public SerializedData GetDataPass( TSource source, IReverseReferenceMap s )
        {
            var member = _getter.Invoke( source );

            return s.WriteObjectReference( member );
        }

        public void SetDataPass( TSource source, SerializedData memberData, IForwardReferenceMap l )
        {
            var newMemberValue = l.ReadObjectReference<TMember>( memberData );
            _setter.Invoke( source, newMemberValue );
        }
    }

    public class MemberObjectMapping<TSource, TMember> : MemberMapping<TSource>, IObjectMapping<TSource>
    {
        private readonly Func<TSource, TMember> _getter;
        private readonly Action<TSource, TMember> _setter;
        private readonly Func<TSource, TMember> _customFactory = null;

        public MemberObjectMapping( Expression<Func<TSource, TMember>> member )
        {
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
        }

        public MemberObjectMapping( Expression<Func<TSource, TMember>> member, Func<TSource, TMember> customFactory )
        {
            _getter = MappingUtils.CreateGetter( member );
            _setter = MappingUtils.CreateSetter( member );
            _customFactory = customFactory;
        }

        public SerializedObject GetObjectsPass( TSource source, IReverseReferenceMap s )
        {
            var member = _getter.Invoke( source );

            var mapping = (IObjectMapping<TMember>)SerializationMapping.GetMappingFor( member );

            return mapping.GetObjectsPass( member, s ); // TODO - this needs to create `$id` and `$type` keys, and recursively call getobjects on the children.
        }

        public void SetObjectsPass( TSource source, SerializedObject memberData, IForwardReferenceMap l )
        {
            //var mapping = SerializationMapping.GetMappingFor<TMember>(); // TODO - use the `$type` field.

            var member = _getter.Invoke( source );

            var mapping = (IObjectMapping<TMember>)SerializationMapping.GetMappingFor( member );

            mapping.SetObjectsPass( member, memberData, l ); // SetObjects doesn't assign anything, it retrieves the member, and calls setobjects further, on it.
        }
    }
}