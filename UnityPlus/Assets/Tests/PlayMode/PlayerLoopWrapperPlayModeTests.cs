//using NUnit.Framework;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.TestTools;
//using UnityPlus;

//namespace HSP_Tests_PlayMode
//{
//    /// <summary>
//    /// PlayMode tests for PlayerLoopWrapper runtime behavior.
//    /// Tests actual execution order, timing, and integration with Unity's player loop.
//    /// </summary>
//    public class PlayerLoopWrapperPlayModeTests
//    {
//        private List<string> _executionOrder;
//        private Dictionary<string, int> _executionCounts;
//        private Dictionary<string, float> _executionTimes;

//        [SetUp]
//        public void SetUp()
//        {
//            _executionOrder = new List<string>();
//            _executionCounts = new Dictionary<string, int>();
//            _executionTimes = new Dictionary<string, float>();
//            PlayerLoopWrapper.Clear(); // Ensure clean state
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            PlayerLoopWrapper.Clear();
//        }

//        #region Basic Execution Tests

//        [UnityTest]
//        public IEnumerator PlaceIn_UpdatePhase_ExecutesInUpdate()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange
//            bool callbackExecuted = false;
//            PlayerLoopWrapper.PlaceIn( "UpdateCallback", () => callbackExecuted = true, PlayerLoopConstants.Update );

//            // Act - Wait for Update to execute
//            yield return new WaitForEndOfFrame();

//            // Assert
//            Assert.IsTrue( callbackExecuted, "Update callback should have executed" );
//        }

//        [UnityTest]
//        public IEnumerator PlaceIn_FixedUpdatePhase_ExecutesInFixedUpdate()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange
//            bool callbackExecuted = false;
//            PlayerLoopWrapper.PlaceIn( "FixedUpdateCallback", () => callbackExecuted = true, PlayerLoopConstants.FixedUpdate );

//            // Act - Wait for FixedUpdate to execute
//            yield return new WaitForFixedUpdate();
//            yield return new WaitForEndOfFrame();

//            // Assert
//            Assert.IsTrue( callbackExecuted, "FixedUpdate callback should have executed" );
//        }

//        [UnityTest]
//        public IEnumerator PlaceIn_LateUpdatePhase_ExecutesInLateUpdate()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange
//            bool callbackExecuted = false;
//            PlayerLoopWrapper.PlaceIn( "PreLateUpdateCallback", () => callbackExecuted = true, PlayerLoopConstants.LateUpdate );

//            // Act - Wait for LateUpdate to execute
//            yield return new WaitForEndOfFrame();

//            // Assert
//            Assert.IsTrue( callbackExecuted, "LateUpdate callback should have executed" );
//        }

//        #endregion

//        #region Execution Order Tests

//        [UnityTest]
//        public IEnumerator TopologicalSorting_SimpleBeforeAfter_ExecutesInCorrectOrder()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange
//            PlayerLoopWrapper.PlaceBefore( "FirstCallback", () => RecordExecution( "FirstCallback" ), "SecondCallback" );
//            PlayerLoopWrapper.PlaceAfter( "SecondCallback", () => RecordExecution( "SecondCallback" ), "FirstCallback" );

//            // Act - Wait for execution
//            yield return new WaitForEndOfFrame();

//            // Assert
//            Assert.AreEqual( 2, _executionOrder.Count );
//            Assert.AreEqual( "FirstCallback", _executionOrder[0] );
//            Assert.AreEqual( "SecondCallback", _executionOrder[1] );
//        }

//        [UnityTest]
//        public IEnumerator TopologicalSorting_ComplexDependencies_ExecutesInCorrectOrder()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange - Create chain: A -> B -> C -> D
//            PlayerLoopWrapper.PlaceAfter( "A", () => RecordExecution( "A" ), "B" );
//            PlayerLoopWrapper.PlaceAfter( "B", () => RecordExecution( "B" ), "C" );
//            PlayerLoopWrapper.PlaceAfter( "C", () => RecordExecution( "C" ), "D" );
//            PlayerLoopWrapper.PlaceIn( "D", () => RecordExecution( "D" ), PlayerLoopConstants.Update );

//            // Act - Wait for execution
//            yield return new WaitForEndOfFrame();

//            // Assert
//            Assert.AreEqual( 4, _executionOrder.Count );
//            Assert.AreEqual( "D", _executionOrder[0] );
//            Assert.AreEqual( "C", _executionOrder[1] );
//            Assert.AreEqual( "B", _executionOrder[2] );
//            Assert.AreEqual( "A", _executionOrder[3] );
//        }

//        [UnityTest]
//        public IEnumerator TopologicalSorting_RegistrationOrder_DoesNotAffectExecutionOrder()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange - Register in random order
//            PlayerLoopWrapper.PlaceAfter( "C", () => RecordExecution( "C" ), "D" );
//            PlayerLoopWrapper.PlaceAfter( "A", () => RecordExecution( "A" ), "B" );
//            PlayerLoopWrapper.PlaceAfter( "B", () => RecordExecution( "B" ), "C" );
//            PlayerLoopWrapper.PlaceIn( "D", () => RecordExecution( "D" ), PlayerLoopConstants.Update );

//            // Act - Wait for execution
//            yield return new WaitForEndOfFrame();

//            // Assert - Should still be in correct order: D -> C -> B -> A
//            Assert.AreEqual( 4, _executionOrder.Count );
//            Assert.AreEqual( "D", _executionOrder[0] );
//            Assert.AreEqual( "C", _executionOrder[1] );
//            Assert.AreEqual( "B", _executionOrder[2] );
//            Assert.AreEqual( "A", _executionOrder[3] );
//        }

//        [UnityTest]
//        public IEnumerator TopologicalSorting_ForwardReferences_HandlesCorrectly()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange - Reference callbacks that don't exist yet
//            PlayerLoopWrapper.PlaceAfter( "Callback1", () => RecordExecution( "Callback1" ), "Callback2", "Callback3" );
//            PlayerLoopWrapper.PlaceBefore( "Callback2", () => RecordExecution( "Callback2" ), "Callback1" );
//            PlayerLoopWrapper.PlaceBefore( "Callback3", () => RecordExecution( "Callback3" ), "Callback1" );

//            // Act - Wait for execution
//            yield return new WaitForEndOfFrame();

//            // Assert
//            Assert.AreEqual( 3, _executionOrder.Count );
//            // Callback1 should come after Callback2 and Callback3
//            int callback1Index = _executionOrder.IndexOf( "Callback1" );
//            int callback2Index = _executionOrder.IndexOf( "Callback2" );
//            int callback3Index = _executionOrder.IndexOf( "Callback3" );

//            Assert.Greater( callback1Index, callback2Index );
//            Assert.Greater( callback1Index, callback3Index );
//        }

//        #endregion

//        #region Multiple Phases Tests

//        [UnityTest]
//        public IEnumerator MultiplePhases_ExecutesInCorrectPhaseOrder()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange
//            PlayerLoopWrapper.PlaceIn( "FixedUpdateCallback", () => RecordExecution( "FixedUpdateCallback" ), PlayerLoopConstants.FixedUpdate );
//            PlayerLoopWrapper.PlaceIn( "UpdateCallback", () => RecordExecution( "UpdateCallback" ), PlayerLoopConstants.Update );
//            PlayerLoopWrapper.PlaceIn( "PreLateUpdateCallback", () => RecordExecution( "PreLateUpdateCallback" ), PlayerLoopConstants.LateUpdate );

//            // Act - Wait for all phases to execute
//            yield return new WaitForEndOfFrame();

//            // Assert - Should execute in phase order: FixedUpdate -> Update -> LateUpdate
//            Assert.IsTrue( _executionOrder.Contains( "FixedUpdateCallback" ) );
//            Assert.IsTrue( _executionOrder.Contains( "UpdateCallback" ) );
//            Assert.IsTrue( _executionOrder.Contains( "LateUpdateCallback" ) );

//            int fixedUpdateIndex = _executionOrder.IndexOf( "FixedUpdateCallback" );
//            int updateIndex = _executionOrder.IndexOf( "UpdateCallback" );
//            int lateUpdateIndex = _executionOrder.IndexOf( "LateUpdateCallback" );

//            Assert.Less( fixedUpdateIndex, updateIndex );
//            Assert.Less( updateIndex, lateUpdateIndex );
//        }

//        [UnityTest]
//        public IEnumerator CustomPhase_ExecutesCorrectly()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange
//            PlayerLoopWrapper.RegisterPhase( "MyCustomPhase", typeof( UnityEngine.PlayerLoop.Update ) );
//            bool callbackExecuted = false;
//            PlayerLoopWrapper.PlaceIn( "CustomCallback", () => callbackExecuted = true, "MyCustomPhase" );

//            // Act - Wait for execution
//            yield return new WaitForEndOfFrame();

//            // Assert
//            Assert.IsTrue( callbackExecuted, "Custom phase callback should have executed" );
//        }

//        #endregion

//        #region Callback Management Tests

//        [UnityTest]
//        public IEnumerator Remove_Callback_StopsExecution()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange
//            bool callbackExecuted = false;
//            PlayerLoopWrapper.PlaceIn( "TestCallback", () => callbackExecuted = true, PlayerLoopConstants.Update );

//            // Act - Wait for first execution
//            yield return new WaitForEndOfFrame();
//            bool firstExecution = callbackExecuted;

//            // Remove callback
//            PlayerLoopWrapper.Remove( "TestCallback" );
//            callbackExecuted = false; // Reset flag

//            // Wait for another frame
//            yield return new WaitForEndOfFrame();
//            bool secondExecution = callbackExecuted;

//            // Assert
//            Assert.IsTrue( firstExecution, "Callback should have executed before removal" );
//            Assert.IsFalse( secondExecution, "Callback should not execute after removal" );
//        }

//        [UnityTest]
//        public IEnumerator MultipleCallbacks_SamePhase_ExecuteInOrder()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange
//            PlayerLoopWrapper.PlaceIn( "Callback1", () => RecordExecution( "Callback1" ), PlayerLoopConstants.Update );
//            PlayerLoopWrapper.PlaceIn( "Callback2", () => RecordExecution( "Callback2" ), PlayerLoopConstants.Update );
//            PlayerLoopWrapper.PlaceIn( "Callback3", () => RecordExecution( "Callback3" ), PlayerLoopConstants.Update );

//            // Act - Wait for execution
//            yield return new WaitForEndOfFrame();

//            // Assert
//            Assert.AreEqual( 3, _executionOrder.Count );
//            Assert.Contains( "Callback1", _executionOrder );
//            Assert.Contains( "Callback2", _executionOrder );
//            Assert.Contains( "Callback3", _executionOrder );
//        }

//        [UnityTest]
//        public IEnumerator CallbackExecution_CountsCorrectly()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange
//            PlayerLoopWrapper.PlaceIn( "CountCallback", () => RecordExecution( "CountCallback" ), PlayerLoopConstants.Update );

//            // Act - Wait for multiple frames
//            yield return new WaitForEndOfFrame();
//            yield return new WaitForEndOfFrame();
//            yield return new WaitForEndOfFrame();

//            // Assert
//            Assert.AreEqual( 3, _executionCounts["CountCallback"] );
//        }

//        #endregion

//        #region Performance Tests

//        [UnityTest]
//        public IEnumerator ManyCallbacks_Performance_ExecutesEfficiently()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange - Create many callbacks
//            const int callbackCount = 100;
//            for( int i = 0; i < callbackCount; i++ )
//            {
//                int index = i; // Capture for closure
//                PlayerLoopWrapper.PlaceIn( $"Callback{index}", () => RecordExecution( $"Callback{index}" ), PlayerLoopConstants.Update );
//            }

//            // Act - Wait for execution
//            float startTime = Time.realtimeSinceStartup;
//            yield return new WaitForEndOfFrame();
//            float endTime = Time.realtimeSinceStartup;

//            // Assert
//            Assert.AreEqual( callbackCount, _executionOrder.Count );
//            Assert.Less( endTime - startTime, 1.0f, "Execution should complete within reasonable time" );
//        }

//        [UnityTest]
//        public IEnumerator ComplexDependencies_Performance_ExecutesEfficiently()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange - Create complex dependency graph
//            const int callbackCount = 50;
//            for( int i = 0; i < callbackCount; i++ )
//            {
//                int index = i;
//                if( i > 0 )
//                {
//                    PlayerLoopWrapper.PlaceAfter( $"Callback{index}", () => RecordExecution( $"Callback{index}" ), $"Callback{index - 1}" );
//                }
//                else
//                {
//                    PlayerLoopWrapper.PlaceIn( $"Callback{index}", () => RecordExecution( $"Callback{index}" ), PlayerLoopConstants.Update );
//                }
//            }

//            // Act - Wait for execution
//            float startTime = Time.realtimeSinceStartup;
//            yield return new WaitForEndOfFrame();
//            float endTime = Time.realtimeSinceStartup;

//            // Assert
//            Assert.AreEqual( callbackCount, _executionOrder.Count );
//            Assert.Less( endTime - startTime, 1.0f, "Complex dependencies should execute efficiently" );
//        }

//        #endregion

//        #region Edge Cases Tests

//        [UnityTest]
//        public IEnumerator EmptyRegistry_DoesNotCrash()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange - Empty registry (already set up in SetUp)

//            // Act & Assert - Should not crash
//            yield return new WaitForEndOfFrame();
//            Assert.Pass( "Empty registry should not cause crashes" );
//        }

//        [UnityTest]
//        public IEnumerator CircularDependencies_HandlesGracefully()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange - Create circular dependency
//            PlayerLoopWrapper.PlaceAfter( "A", () => RecordExecution( "A" ), "B" );
//            PlayerLoopWrapper.PlaceAfter( "B", () => RecordExecution( "B" ), "A" );

//            // Act - Wait for execution
//            yield return new WaitForEndOfFrame();

//            // Assert - Should handle gracefully (may execute in any order or skip)
//            // The important thing is that it doesn't crash
//            Assert.Pass( "Circular dependencies should be handled gracefully" );
//        }

//        [UnityTest]
//        public IEnumerator SelfReference_HandlesGracefully()
//        {
//            yield return new WaitForEndOfFrame();

//            // Arrange - Self-referencing callback
//            PlayerLoopWrapper.PlaceAfter( "SelfRef", () => RecordExecution( "SelfRef" ), "SelfRef" );

//            // Act - Wait for execution
//            yield return new WaitForEndOfFrame();

//            // Assert - Should handle gracefully
//            Assert.Pass( "Self-referencing callbacks should be handled gracefully" );
//        }

//        #endregion

//        #region Helper Methods

//        private void RecordExecution( string callbackId )
//        {
//            _executionOrder.Add( callbackId );
//            _executionCounts[callbackId] = _executionCounts.GetValueOrDefault( callbackId, 0 ) + 1;
//            _executionTimes[callbackId] = Time.realtimeSinceStartup;
//        }

//        #endregion
//    }
//}
