//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.LowLevel;
//using UnityEngine.PlayerLoop;

//namespace UnityPlus
//{
//    /// <summary>
//    /// A modern wrapper for Unity's PlayerLoop system that uses named constants and topological sorting.
//    /// Provides a simple API for placing callbacks in the player loop with proper ordering.
//    /// </summary>
//    public static class PlayerLoopWrapper
//    {
//        class Entry
//        {
//            public bool isStale;
//            public List<PlayerLoopCallbackItem> sortedCallbacks;
//        }

//        struct BuiltinLoopData
//        {
//            public Type playerLoopType;
//            public PlayerLoopCustomPath path; // null for root. type represents what to search,
//                                              // when type == typeof(PlayerLoopCallbackItem) then customId is used to match.

//            public BuiltinLoopData( Type playerLoopType, PlayerLoopCustomPath path )
//            {
//                this.playerLoopType = playerLoopType;
//                this.path = path;
//            }
//        }

//        internal static readonly Dictionary<string, PlayerLoopCallbackItem> _callbacks = new(); // if a target is not yet added, callbacks go here.

//        private static readonly Dictionary<string, BuiltinLoopData> _nameToLoopData = new() // builtins
//        {
//            { PlayerLoopConstants.Initialization,       new BuiltinLoopData( typeof( Initialization ), PlayerLoopCustomPath.Empty ) },

//            { PlayerLoopConstants.PreFixedUpdate,       new BuiltinLoopData( typeof( FixedUpdate.ClearLines ), new PlayerLoopCustomPath( typeof( FixedUpdate ) ) ) },
//            { PlayerLoopConstants.FixedUpdate,          new BuiltinLoopData( typeof( FixedUpdate.ScriptRunBehaviourFixedUpdate ), new PlayerLoopCustomPath( typeof( FixedUpdate ) ) ) },
//            { PlayerLoopConstants.PhysicsFixedUpdate,   new BuiltinLoopData( typeof( FixedUpdate.PhysicsFixedUpdate ), new PlayerLoopCustomPath( typeof( FixedUpdate ) ) ) },
//            { PlayerLoopConstants.Physics2DFixedUpdate, new BuiltinLoopData( typeof( FixedUpdate.Physics2DFixedUpdate ), new PlayerLoopCustomPath( typeof( FixedUpdate ) ) ) },

//            { PlayerLoopConstants.EarlyUpdate,          new BuiltinLoopData( typeof( EarlyUpdate ), PlayerLoopCustomPath.Empty ) },
//            { PlayerLoopConstants.Update,               new BuiltinLoopData( typeof( Update.ScriptRunBehaviourUpdate ), new PlayerLoopCustomPath( typeof( Update ) ) ) },
//            { PlayerLoopConstants.LateUpdate,           new BuiltinLoopData( typeof( PreLateUpdate.ScriptRunBehaviourLateUpdate ), new PlayerLoopCustomPath( typeof( PreLateUpdate ) ) ) },
//        };

//        /// <summary>
//        /// Adds a callback to the registry.
//        /// </summary>
//        /// <param name="callback">The callback to add</param>
//        /// <returns>True if the callback was added, false if a callback with the same ID already exists</returns>
//        public static void AddCallback( PlayerLoopCallbackItem callback )
//        {
//            if( callback.ID.builtinType != null )
//                throw new ArgumentException( "Cannot add a builtin type callback." );

//            // when adding, recalculate 'callback' as well as anything that has 'callback' as a parent.

//            _callbacks.Add( callback.ID.id, callback );
//            RefreshPlayerLoop( callback );
//        }

//        /// <summary>
//        /// Removes a callback from the registry.
//        /// </summary>
//        /// <param name="id">The ID of the callback to remove</param>
//        /// <returns>True if the callback was removed, false if it didn't exist</returns>
//        public static void RemoveCallback( PlayerLoopCustomPath target, PlayerLoopCustomPathElement id )
//        {
//            if( !_callbacks.Remove( id.id, out var callback ) )
//                return;
//            RefreshPlayerLoop( callback );
//        }

//        /// <summary>
//        /// Gets all callbacks, sorted by their dependencies.
//        /// </summary>
//        /// <returns>Sorted array of all callbacks</returns>
//        private PlayerLoopSystem[] GetAllSortedCallbacks( PlayerLoopCustomPath target )
//        {
            
//#warning TODO - needs to include callbacks from the default unity player loop as well.
//        }

//        private static void RefreshPlayerLoop( PlayerLoopCallbackItem callback )
//        {
//            try
//            {
//                var currentLoop = PlayerLoop.GetCurrentPlayerLoop();
//                var parentLoop = callback.Target.GetSystem( ref currentLoop, out var foundSystem );

//                // collect everything that is in the parent loop. 
//                // find all callbacks that exist in the registry and have the same 'target' as this.
//                // find all callbacks in the loop that also exist in the previously found set.
//                // remove found callbacks, read new callbacks. sort the entire thing.

//                PlayerLoop.SetPlayerLoop( currentLoop );
//            }
//            catch( Exception ex )
//            {
//                Debug.LogError( $"Failed to refresh player loop: {ex.Message}" );
//                Debug.LogException( ex );
//            }
//        }

//        private static bool TargetFromName( string element, out PlayerLoopCustomPath target )
//        {
//            if( _nameToLoopData.TryGetValue( element, out var entry ) )
//            {
//                target = entry.path;
//                return true;
//            }
//#warning TODO - target might be added later so this needs to still work.
//            bool res = TryGetCallback( element, out var callback );
//            target = res ? callback.Target : PlayerLoopCustomPath.Empty;
//            return res;
//        }

//        static PlayerLoopCustomPathElement[] IDsFromNames( string[] elements )
//        {
//            if( elements == null )
//                return null;

//            var result = new PlayerLoopCustomPathElement[elements.Length];
//            for( int i = 0; i < elements.Length; i++ )
//            {
//                if( _nameToLoopData.TryGetValue( elements[i], out var entry ) )
//                    result[i] = new PlayerLoopCustomPathElement( entry.playerLoopType );
//                else
//                    result[i] = new PlayerLoopCustomPathElement( elements[i] );
//            }
//            return result;
//        }

//        /// <summary>
//        /// Adds a given callback to execute before all the specified target callbacks. <br/>
//        /// The targets should have the same parent player loop system.
//        /// </summary>
//        public static void PlaceBefore( string id, PlayerLoopSystem.UpdateFunction callback, params string[] before )
//        {
//            var callbackItem = new PlayerLoopCallbackItem( id, callback, GetParentPath( before ), IDsFromNames( before ), null );
//            PlaceCallback( callbackItem );
//        }

//        /// <summary>
//        /// Adds a given callback to execute after all the specified target callbacks. <br/>
//        /// The targets should have the same parent player loop system.
//        /// </summary>
//        public static void PlaceAfter( string id, PlayerLoopSystem.UpdateFunction callback, params string[] after )
//        {
//            var callbackItem = new PlayerLoopCallbackItem( id, callback, GetParentPath( after ), null, IDsFromNames( after ) );
//            PlaceCallback( callbackItem );
//        }

//        /// <summary>
//        /// Adds a given callback to execute inside the specified target phase.
//        /// </summary>
//        public static void PlaceIn( string id, string target, PlayerLoopSystem.UpdateFunction callback )
//        {
//            var callbackItem = new PlayerLoopCallbackItem( id, callback, TargetFromName( target ), null, null );
//            PlaceCallback( callbackItem );
//        }

//        public static void Place( string id, string target, PlayerLoopSystem.UpdateFunction callback, IEnumerable<string> before = null, IEnumerable<string> after = null )
//        {
//            PlayerLoopCustomPath targetPath = string.IsNullOrEmpty( target )
//                ? GetParentPath( before.Union( after ) )
//                : FromID( target );
//            string[] beforeArray = before?.ToArray();
//            string[] afterArray = after?.ToArray();

//            var callbackItem = new PlayerLoopCallbackItem( id, callback, targetPath, FromIDs( beforeArray ), FromIDs( afterArray ) );
//            PlaceCallback( callbackItem );
//        }

//        private static void PlaceCallback( PlayerLoopCallbackItem callback )
//        {
//            if( _registry.HasCallback( callback.Target, callback.ID ) )
//            {
//                Debug.LogWarning( $"Callback with ID '{callback.ID}' already exists. Removing old one and adding new one." );
//                _registry.RemoveCallback( callback.Target, callback.ID );
//                _registry.AddCallback( callback );
//                return;
//            }

//            _registry.AddCallback( callback );
//        }

//        public static bool Remove( string id )
//        {
//            if( !_registry.RemoveCallback( id ) )
//                return false;

//            RefreshPlayerLoop();
//            return true;
//        }

//        private static PlayerLoopCustomPath GetParentPath( IEnumerable<string> targetIDs )
//        {

//        }

//        private static void RemoveAllCustomCallbacks( ref PlayerLoopSystem loop )
//        {
//            if( loop.subSystemList == null )
//                return;

//            var systemsToRemove = new List<PlayerLoopSystem>();
//            for( int i = 0; i < loop.subSystemList.Length; i++ )
//            {
//                if( loop.subSystemList[i].type == typeof( PlayerLoopCallbackItem ) )
//                {
//                    systemsToRemove.Add( loop.subSystemList[i] );
//                }
//                else
//                {
//                    RemoveAllCustomCallbacks( ref loop.subSystemList[i] );
//                }
//            }

//            foreach( var systemToRemove in systemsToRemove )
//            {
//                var list = loop.subSystemList.ToList();
//                list.Remove( systemToRemove );
//                loop.subSystemList = list.ToArray();
//            }
//        }

//        private static void AddCallbackToPhase( ref PlayerLoopSystem loop, PlayerLoopCallbackItem callback, string phase )
//        {
//            if( !_phaseToTypeMap.TryGetValue( phase, out var targetType ) )
//            {
//                Debug.LogWarning( $"Unknown phase '{phase}'. Using default phase." );
//                targetType = typeof( UnityEngine.PlayerLoop.Update );
//            }

//            AddSystemToType( ref loop, callback.System, targetType );
//        }

//        private static void AddSystemToType( ref PlayerLoopSystem system, PlayerLoopSystem systemToAdd, Type targetType )
//        {
//            if( system.type == targetType )
//            {
//                var list = system.subSystemList?.ToList() ?? new List<PlayerLoopSystem>();
//                list.Add( systemToAdd );
//                system.subSystemList = list.ToArray();
//                return;
//            }

//            if( system.subSystemList != null )
//            {
//                for( int i = 0; i < system.subSystemList.Length; i++ )
//                {
//                    AddSystemToType( ref system.subSystemList[i], systemToAdd, targetType );
//                }
//            }
//        }

//        private static string GetDefaultPhase()
//        {
//            return PlayerLoopConstants.Update;
//        }

//        public static void RegisterPhase( string phaseName, Type unityType )
//        {
//            _phaseToTypeMap[phaseName] = unityType;
//        }

//        public static PlayerLoopCallbackItem[] GetAllCallbacks()
//        {
//            return _registry.GetAllSortedCallbacks();
//        }

//        public static void Clear()
//        {
//            _registry.Clear();
//            RefreshPlayerLoop();
//        }
//    }
//}