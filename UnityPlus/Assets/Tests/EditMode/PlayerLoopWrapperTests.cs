//using NUnit.Framework;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityPlus;

//namespace HSP_Tests_EditMode
//{
//    /// <summary>
//    /// EditMode tests for PlayerLoopWrapper core functionality.
//    /// Tests the API, registry, and topological sorting without requiring runtime execution.
//    /// </summary>
//    public class PlayerLoopWrapperTests
//    {
//        private PlayerLoopCallbackRegistry _registry;
//        private List<string> _executionOrder;
//        private Dictionary<string, int> _executionCounts;

//        [SetUp]
//        public void SetUp()
//        {
//            _registry = new PlayerLoopCallbackRegistry();
//            _executionOrder = new List<string>();
//            _executionCounts = new Dictionary<string, int>();
//            PlayerLoopWrapper.Clear(); // Ensure clean state
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            PlayerLoopWrapper.Clear();
//        }

//        #region Basic API Tests

//        [Test]
//        public void PlaceIn_WithValidPhase_AddsCallback()
//        {
//            // Arrange
//            string callbackId = "TestCallback";
//            bool callbackExecuted = false;

//            // Act
//            PlayerLoopWrapper.PlaceIn( callbackId, () => callbackExecuted = true, PlayerLoopConstants.Update );

//            // Assert
//            Assert.IsTrue( HasCallback( callbackId ) );
//            Assert.IsTrue( callbackExecuted == false ); // Should not execute immediately
//        }

//        [Test]
//        public void PlaceBefore_WithTargets_SetsCorrectDependencies()
//        {
//            // Arrange
//            string callbackId = "BeforeCallback";
//            string targetId = "TargetCallback";

//            // Act
//            PlayerLoopWrapper.PlaceBefore( callbackId, () => { }, targetId );

//            // Assert
//            Assert.IsTrue( HasCallback( callbackId ) );
//        }

//        [Test]
//        public void PlaceAfter_WithTargets_SetsCorrectDependencies()
//        {
//            // Arrange
//            string callbackId = "AfterCallback";
//            string targetId = "TargetCallback";

//            // Act
//            PlayerLoopWrapper.PlaceAfter( callbackId, () => { }, targetId );
//            PlayerLoopWrapper.PlaceIn( targetId, () => { }, PlayerLoopConstants.LateUpdate );

//            // Assert
//            Assert.IsTrue( HasCallback( callbackId ) );
//        }

//        [Test]
//        public void Place_WithMissingConstraints_DoesntExecute()
//        {
//            // Arrange
//            string callbackId = "ComplexCallback";
//            string[] beforeTargets = { "Target1", "Target2" };
//            string[] afterTargets = { "Target3", "Target4" };

//            // Act
//            PlayerLoopWrapper.Place( callbackId, () => { }, beforeTargets, afterTargets, PlayerLoopConstants.LateUpdate );

//            // Assert
//            Assert.IsTrue( !HasCallback( callbackId ) );
//        }

//        [Test]
//        public void Remove_ExistingCallback_RemovesCallback()
//        {
//            // Arrange
//            string callbackId = "TestCallback";
//            PlayerLoopWrapper.PlaceIn( callbackId, () => { }, PlayerLoopConstants.Update );

//            // Act
//            bool removed = PlayerLoopWrapper.Remove( callbackId );

//            // Assert
//            Assert.IsTrue( removed );
//            Assert.IsFalse( HasCallback( callbackId ) );
//        }

//        [Test]
//        public void Remove_NonExistentCallback_ReturnsFalse()
//        {
//            // Act
//            bool removed = PlayerLoopWrapper.Remove( "NonExistentCallback" );

//            // Assert
//            Assert.IsFalse( removed );
//        }

//        #endregion

//        #region Topological Sorting Tests

//        [Test]
//        public void TopologicalSorting_SimpleBeforeAfter_OrdersCorrectly()
//        {
//            // Arrange
//            var callback1 = new PlayerLoopCallbackItem( "Callback1", () => { }, PlayerLoopConstants.Update, after: new[] { "Callback2" } );
//            var callback2 = new PlayerLoopCallbackItem( "Callback2", () => { }, PlayerLoopConstants.Update, before: new[] { "Callback1" } );

//            // Act
//            _registry.AddCallback( callback1 );
//            _registry.AddCallback( callback2 );
//            var sortedCallbacks = _registry.GetSortedCallbacksForPhase( PlayerLoopConstants.Update );

//            // Assert
//            Assert.AreEqual( 2, sortedCallbacks.Length );
//            Assert.AreEqual( "Callback2", sortedCallbacks[0].ID );
//            Assert.AreEqual( "Callback1", sortedCallbacks[1].ID );
//        }

//        [Test]
//        public void TopologicalSorting_ComplexDependencies_OrdersCorrectly()
//        {
//            // Arrange - Create a dependency chain: A -> B -> C -> D
//            var callbackA = new PlayerLoopCallbackItem( "A", () => { }, PlayerLoopConstants.Update, after: new[] { "B" } );
//            var callbackB = new PlayerLoopCallbackItem( "B", () => { }, PlayerLoopConstants.Update, after: new[] { "C" } );
//            var callbackC = new PlayerLoopCallbackItem( "C", () => { }, PlayerLoopConstants.Update, after: new[] { "D" } );
//            var callbackD = new PlayerLoopCallbackItem( "D", () => { }, PlayerLoopConstants.Update );

//            // Act - Add in random order
//            _registry.AddCallback( callbackC );
//            _registry.AddCallback( callbackA );
//            _registry.AddCallback( callbackD );
//            _registry.AddCallback( callbackB );
//            var sortedCallbacks = _registry.GetSortedCallbacksForPhase( PlayerLoopConstants.Update );

//            // Assert
//            Assert.AreEqual( 4, sortedCallbacks.Length );
//            Assert.AreEqual( "D", sortedCallbacks[0].ID );
//            Assert.AreEqual( "C", sortedCallbacks[1].ID );
//            Assert.AreEqual( "B", sortedCallbacks[2].ID );
//            Assert.AreEqual( "A", sortedCallbacks[3].ID );
//        }

//        [Test]
//        public void TopologicalSorting_ForwardReferences_HandlesCorrectly()
//        {
//            // Arrange - Reference callbacks that don't exist yet
//            var callback1 = new PlayerLoopCallbackItem( "Callback1", () => { }, PlayerLoopConstants.Update, after: new[] { "Callback2", "Callback3" } );
//            var callback2 = new PlayerLoopCallbackItem( "Callback2", () => { }, PlayerLoopConstants.Update, before: new[] { "Callback1" } );
//            var callback3 = new PlayerLoopCallbackItem( "Callback3", () => { }, PlayerLoopConstants.Update, before: new[] { "Callback1" } );

//            // Act
//            _registry.AddCallback( callback1 );
//            _registry.AddCallback( callback2 );
//            _registry.AddCallback( callback3 );
//            var sortedCallbacks = _registry.GetSortedCallbacksForPhase( PlayerLoopConstants.Update );

//            // Assert
//            Assert.AreEqual( 3, sortedCallbacks.Length );
//            // Callback1 should come after Callback2 and Callback3
//            int callback1Index = Array.FindIndex( sortedCallbacks, c => c.ID == "Callback1" );
//            int callback2Index = Array.FindIndex( sortedCallbacks, c => c.ID == "Callback2" );
//            int callback3Index = Array.FindIndex( sortedCallbacks, c => c.ID == "Callback3" );

//            Assert.Greater( callback1Index, callback2Index );
//            Assert.Greater( callback1Index, callback3Index );
//        }

//        [Test]
//        public void TopologicalSorting_MultiplePhases_HandlesCorrectly()
//        {
//            // Arrange
//            var updateCallback = new PlayerLoopCallbackItem( "UpdateCallback", () => { }, PlayerLoopConstants.Update );
//            var fixedUpdateCallback = new PlayerLoopCallbackItem( "FixedUpdateCallback", () => { }, PlayerLoopConstants.FixedUpdate );
//            var preLateUpdateCallback = new PlayerLoopCallbackItem( "PreLateUpdateCallback", () => { }, PlayerLoopConstants.LateUpdate );

//            // Act
//            _registry.AddCallback( updateCallback );
//            _registry.AddCallback( fixedUpdateCallback );
//            _registry.AddCallback( preLateUpdateCallback );

//            // Assert
//            var updateCallbacks = _registry.GetSortedCallbacksForPhase( PlayerLoopConstants.Update );
//            var fixedUpdateCallbacks = _registry.GetSortedCallbacksForPhase( PlayerLoopConstants.FixedUpdate );
//            var preLateUpdateCallbacks = _registry.GetSortedCallbacksForPhase( PlayerLoopConstants.LateUpdate );

//            Assert.AreEqual( 1, updateCallbacks.Length );
//            Assert.AreEqual( 1, fixedUpdateCallbacks.Length );
//            Assert.AreEqual( 1, preLateUpdateCallbacks.Length );
//            Assert.AreEqual( "UpdateCallback", updateCallbacks[0].ID );
//            Assert.AreEqual( "FixedUpdateCallback", fixedUpdateCallbacks[0].ID );
//            Assert.AreEqual( "PreLateUpdateCallback", preLateUpdateCallbacks[0].ID );
//        }

//        #endregion

//        #region Registry Tests

//        [Test]
//        public void Registry_AddCallback_DuplicateId_ReturnsFalse()
//        {
//            // Arrange
//            var callback1 = new PlayerLoopCallbackItem( "TestId", () => { }, PlayerLoopConstants.Update );
//            var callback2 = new PlayerLoopCallbackItem( "TestId", () => { }, PlayerLoopConstants.Update );

//            // Act
//            bool firstAdd = _registry.AddCallback( callback1 );
//            bool secondAdd = _registry.AddCallback( callback2 );

//            // Assert
//            Assert.IsTrue( firstAdd );
//            Assert.IsFalse( secondAdd );
//        }

//        [Test]
//        public void Registry_RemoveCallback_ExistingCallback_ReturnsTrue()
//        {
//            // Arrange
//            var callback = new PlayerLoopCallbackItem( "TestId", () => { }, PlayerLoopConstants.Update );
//            _registry.AddCallback( callback );

//            // Act
//            bool removed = _registry.RemoveCallback( "TestId" );

//            // Assert
//            Assert.IsTrue( removed );
//            Assert.AreEqual( 0, _registry.Count );
//        }

//        [Test]
//        public void Registry_RemoveCallback_NonExistentCallback_ReturnsFalse()
//        {
//            // Act
//            bool removed = _registry.RemoveCallback( "NonExistentId" );

//            // Assert
//            Assert.IsFalse( removed );
//        }

//        [Test]
//        public void Registry_GetPhasesWithCallbacks_ReturnsCorrectPhases()
//        {
//            // Arrange
//            _registry.AddCallback( new PlayerLoopCallbackItem( "Update1", () => { }, PlayerLoopConstants.Update ) );
//            _registry.AddCallback( new PlayerLoopCallbackItem( "Update2", () => { }, PlayerLoopConstants.Update ) );
//            _registry.AddCallback( new PlayerLoopCallbackItem( "FixedUpdate1", () => { }, PlayerLoopConstants.FixedUpdate ) );

//            // Act
//            var phases = _registry.GetPhasesWithCallbacks();

//            // Assert
//            Assert.AreEqual( 2, phases.Length );
//            Assert.Contains( PlayerLoopConstants.Update, phases );
//            Assert.Contains( PlayerLoopConstants.FixedUpdate, phases );
//        }

//        [Test]
//        public void Registry_Clear_RemovesAllCallbacks()
//        {
//            // Arrange
//            _registry.AddCallback( new PlayerLoopCallbackItem( "Callback1", () => { }, PlayerLoopConstants.Update ) );
//            _registry.AddCallback( new PlayerLoopCallbackItem( "Callback2", () => { }, PlayerLoopConstants.FixedUpdate ) );

//            // Act
//            _registry.Clear();

//            // Assert
//            Assert.AreEqual( 0, _registry.Count );
//            Assert.AreEqual( 0, _registry.GetPhasesWithCallbacks().Length );
//        }

//        #endregion

//        #region Phase Mapping Tests

//        [Test]
//        public void RegisterPhase_CustomPhase_CanBeUsed()
//        {
//            // Arrange
//            string customPhase = "MyCustomPhase";
//            Type customType = typeof( UnityEngine.PlayerLoop.Update );

//            // Act
//            PlayerLoopWrapper.RegisterPhase( customPhase, customType );
//            PlayerLoopWrapper.PlaceIn( "CustomCallback", () => { }, customPhase );

//            // Assert
//            Assert.IsTrue( HasCallback( "CustomCallback" ) );
//        }

//        #endregion

//        #region Error Handling Tests

//        [Test]
//        public void PlaceIn_NullId_ThrowsArgumentNullException()
//        {
//            // Act & Assert
//            Assert.Throws<ArgumentNullException>( () =>
//                PlayerLoopWrapper.PlaceIn( null, () => { }, PlayerLoopConstants.Update ) );
//        }

//        [Test]
//        public void PlaceIn_NullCallback_ThrowsArgumentNullException()
//        {
//            // Act & Assert
//            Assert.Throws<ArgumentNullException>( () =>
//                PlayerLoopWrapper.PlaceIn( "TestId", null, PlayerLoopConstants.Update ) );
//        }

//        [Test]
//        public void PlaceIn_NullPhase_ThrowsArgumentNullException()
//        {
//            // Act & Assert
//            Assert.Throws<ArgumentNullException>( () =>
//                PlayerLoopWrapper.PlaceIn( "TestId", () => { }, null ) );
//        }

//        [Test]
//        public void PlayerLoopCallbackItem_NullId_ThrowsArgumentNullException()
//        {
//            // Act & Assert
//            Assert.Throws<ArgumentNullException>( () =>
//                new PlayerLoopCallbackItem( null, () => { }, PlayerLoopConstants.Update ) );
//        }

//        [Test]
//        public void PlayerLoopCallbackItem_NullCallback_ThrowsArgumentNullException()
//        {
//            // Act & Assert
//            Assert.Throws<ArgumentNullException>( () =>
//                new PlayerLoopCallbackItem( "TestId", null, PlayerLoopConstants.Update ) );
//        }

//        [Test]
//        public void PlayerLoopCallbackItem_NullPhase_ThrowsArgumentNullException()
//        {
//            // Act & Assert
//            Assert.Throws<ArgumentNullException>( () =>
//                new PlayerLoopCallbackItem( "TestId", () => { }, null ) );
//        }

//        #endregion

//        #region Helper Methods

//        private void RecordExecution( string callbackId )
//        {
//            _executionOrder.Add( callbackId );
//            _executionCounts[callbackId] = _executionCounts.GetValueOrDefault( callbackId, 0 ) + 1;
//        }

//        private static bool HasCallback( string id )
//        {
//            return PlayerLoopWrapper.GetAllCallbacks().Any( c => c.ID == id );
//        }

//        #endregion
//    }
//}
