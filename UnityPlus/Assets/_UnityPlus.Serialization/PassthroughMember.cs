
namespace UnityPlus.Serialization
{
    /// <summary>
    /// This is used when you need to store a member of the base type inside the memberwise mapping.
    /// </summary>
    internal class PassthroughMember<TSource, TSourceBase> : MemberBase<TSource> where TSourceBase : class, TSource
        // internal class PassthroughMember<TSource, TSourceBase> : MemberBase<TSource> where TSourceBase : class
    {
        MemberBase<TSourceBase> _member;

        internal static PassthroughMember<TSource, TSourceBase> Create( MemberBase<TSourceBase> member )
        {
            return new PassthroughMember<TSource, TSourceBase>()
            {
                _member = member,
            };
        }

        public override MemberBase<TSource> Copy()
        {
            return (MemberBase<TSource>)this.MemberwiseClone();
        }

        public override bool Save( TSource source, SerializedData sourceData, ISaver s )
        {
            //return _member.Save( source as TSourceBase, s );
            return _member.Save( (TSourceBase)source, sourceData, s );
        }

        public override bool Load( ref object member, SerializedData sourceData, ILoader l )
        {
            //TSourceBase src = source as TSourceBase; // won't work for structs, but structs aren't inheritable anyway.
            //TSourceBase src = (TSourceBase)source; // won't work for structs, but structs aren't inheritable anyway.

            return _member.Load( ref member, sourceData, l );
        }

        public override void Assign( ref TSource source, object member )
        {
            TSourceBase source2 = (TSourceBase)source;
            //TSourceBase source2 = (TSourceBase)(object)source;
            _member.Assign( ref source2, member );
            source = source2;
            //source = (TSource)(object)source2;
        }
    }
}