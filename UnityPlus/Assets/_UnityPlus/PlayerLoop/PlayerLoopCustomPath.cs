//using System;
//using System.Linq;
//using UnityEngine.LowLevel;

//namespace UnityPlus
//{
//    /// <summary>
//    /// Identifies an element (both custom and builtin) in the player loop.
//    /// </summary>
//    public struct PlayerLoopCustomPath : IEquatable<PlayerLoopCustomPath>
//    {
//        string _unresolvedId;
//        // if this was added before the thing that contains it, we can't resolve until the thing is placed.
//        PlayerLoopCustomPathElement[] _resolvedElems;

//        public static readonly PlayerLoopCustomPath Empty = new PlayerLoopCustomPath( (string)null );

//        public PlayerLoopCustomPath( params PlayerLoopCustomPathElement[] elems )
//        {
//            _unresolvedId = null;
//            _resolvedElems = elems;
//        }

//        public PlayerLoopCustomPath( string targetId )
//        {
//            _unresolvedId = targetId;
//            _resolvedElems = null;
//        }

//        public bool GetSystem( ref PlayerLoopSystem root, out PlayerLoopSystem foundSystem )
//        {
//            if( _unresolvedId == null && _resolvedElems == null )
//            {
//                foundSystem = root;
//                return true; // empty path, return root
//            }
//            if( _resolvedElems == null )
//            {
//                // Try to resolve the unresolved ID into path elements.
//                if( !PlayerLoopCallbackRegistry.instance.TryResolvePath( _unresolvedId, out _resolvedElems ) )
//                {
//                    foundSystem = default;
//                    return false; // could not resolve path
//                }
//                _unresolvedId = null; // resolved now
//            }

//            // finds a system that this path points to, for the given input system.
//            // Traverse the path elements recursively.
//            var current = root;
//            foreach( var elem in _resolvedElems ?? Array.Empty<PlayerLoopCustomPathElement>() )
//            {
//                if( elem.builtinType != null )
//                {
//                    // Look for a subsystem with the builtin type
//                    if( current.subSystemList != null )
//                        current = current.subSystemList.FirstOrDefault( s => s.type == elem.builtinType );
//                }
//#warning TODO - paths need something to be able to target raw loop systems too.
//                else if( !string.IsNullOrEmpty( elem.id ) )
//                {
//                    if( current.subSystemList != null )
//                    {
//                        var updateDelegate = // get target update delegate from registry. we can't store it in the path elem because it might change/not be defined yet. The ID is constant and defined.
//                        for( int j = 0; j < current.subSystemList.Length; j++ )
//                        {

//                        }
//                    }
//                    // Look for a custom callback item with matching ID
//                    // how to check whether the playerloopsystem was created from a PlayerLoopCallbackItem with that specific id?
//                    // we could probably check the update delegate, but we need to look it up for each path elem that is not a builtin type.
//                }

//                if( current.type == null ) // was default
//                {
//                    foundSystem = default;
//                    return false; // path not found
//                }
//            }

//            foundSystem = current;
//            return true;
//        }

//        public override int GetHashCode()
//        {
//            if( _resolvedElems == null )
//                return 0;
//            int hash = 17;
//            foreach( var elem in _resolvedElems )
//            {
//                hash = hash * 31 + elem.GetHashCode();
//            }
//            return hash;
//        }

//        public override string ToString()
//        {
//            return string.Join( " -> ", _resolvedElems.Select( e => e.builtinType != null ? e.builtinType.Name : e.id ) );
//        }

//        public override bool Equals( object obj )
//        {
//            return obj is PlayerLoopCustomPath other && Equals( other );
//        }

//        public bool Equals( PlayerLoopCustomPath other )
//        {
//            if( _resolvedElems == null && other._resolvedElems == null )
//                return true;
//            if( _resolvedElems == null || other._resolvedElems == null )
//                return false;
//            if( _resolvedElems.Length != other._resolvedElems.Length )
//                return false;
//            for( int i = 0; i < _resolvedElems.Length; i++ )
//            {
//                if( !_resolvedElems[i].Equals( other._resolvedElems[i] ) )
//                    return false;
//            }
//            return true;
//        }
//    }
//}