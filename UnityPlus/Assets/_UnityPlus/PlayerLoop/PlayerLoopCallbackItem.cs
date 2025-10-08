//using System;
//using UnityEngine.LowLevel;

//namespace UnityPlus
//{
//    /// <summary>
//    /// Represents a callback that can be placed in the Unity player loop.
//    /// Implements topological sorting to ensure proper execution order.
//    /// </summary>
//    public class PlayerLoopCallbackItem : ITopologicallySortable<PlayerLoopCustomPathElement>
//    {
//        /// <summary>
//        /// The unique identifier for this callback.
//        /// </summary>
//        public PlayerLoopCustomPathElement ID { get; }

//        /// <summary>
//        /// The callback delegate to execute.
//        /// </summary>
//        public PlayerLoopSystem.UpdateFunction Callback { get; }

//        /// <summary>
//        /// The target phase where this callback should be placed.
//        /// </summary>
//        public PlayerLoopCustomPath Target { get; }

//        /// <summary>
//        /// Callbacks that should run BEFORE this callback.
//        /// </summary>
//        public PlayerLoopCustomPathElement[] Before { get; }

//        /// <summary>
//        /// Callbacks that should run AFTER this callback.
//        /// </summary>
//        public PlayerLoopCustomPathElement[] After { get; }

//        /// <summary>
//        /// The Unity PlayerLoopSystem that wraps this callback.
//        /// </summary>
//        public PlayerLoopSystem System { get; }

//        public PlayerLoopCallbackItem( string id, PlayerLoopSystem.UpdateFunction callback, PlayerLoopCustomPath targetIn, PlayerLoopCustomPathElement[] before, PlayerLoopCustomPathElement[] after )
//        {
//            if( id == null )
//                throw new ArgumentNullException( nameof( id ) );

//            ID = new PlayerLoopCustomPathElement( id );
//            Callback = callback;
//            Target = targetIn;
//            Before = before;
//            After = after;

//            System = new PlayerLoopSystem()
//            {
//                type = typeof( PlayerLoopCallbackItem ),
//                updateDelegate = callback
//            };
//        }
//    }
//}