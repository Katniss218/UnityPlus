
namespace UnityPlus.Serialization
{
    /// <summary>
    /// This is used when you need to store a member of the base type inside the memberwise mapping.
    /// </summary>
    internal class PassthroughMember<TSource, TSourceBase> : MemberBase<TSource> where TSource : class, TSourceBase
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
            return _member.Save( source, sourceData, s );
        }

        public override bool Load( ref object member, SerializedData sourceData, ILoader l )
        {
            return _member.Load( ref member, sourceData, l );
        }

        public override void Assign( ref TSource source, object member )
        {
            TSourceBase source2 = source;
            _member.Assign( ref source2, member );
            source = (TSource)source2;
        }
    }
}