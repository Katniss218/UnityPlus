//using System;

//namespace UnityPlus
//{
//    /// <summary>
//    /// Identifies an element (both custom and builtin) in the player loop path.
//    /// </summary>
//    public struct PlayerLoopCustomPathElement : IEquatable<PlayerLoopCustomPathElement>
//    {
//        public Type builtinType;
//        public string id;

//        public PlayerLoopCustomPathElement( Type builtinType )
//        {
//            this.builtinType = builtinType;
//            this.id = null;
//        }

//        public PlayerLoopCustomPathElement( string id )
//        {
//            this.builtinType = null;
//            this.id = id;
//        }

//        public override int GetHashCode()
//        {
//            return (builtinType?.GetHashCode() ?? 0) ^ (id?.GetHashCode() ?? 0);
//        }
//        public override string ToString()
//        {
//            return builtinType != null ? builtinType.Name : id;
//        }
//        public override bool Equals( object obj )
//        {
//            return obj is PlayerLoopCustomPathElement other && Equals( other );
//        }
//        public bool Equals( PlayerLoopCustomPathElement other )
//        {
//            return other.builtinType == builtinType && other.id == id;
//        }

//        public static bool operator ==( PlayerLoopCustomPathElement a, PlayerLoopCustomPathElement b ) => a.Equals( b );
//        public static bool operator !=( PlayerLoopCustomPathElement a, PlayerLoopCustomPathElement b ) => !a.Equals( b );

//        public static implicit operator PlayerLoopCustomPathElement( Type type ) => new PlayerLoopCustomPathElement( type );
//        public static implicit operator PlayerLoopCustomPathElement( string id ) => new PlayerLoopCustomPathElement( id );
//    }
//}