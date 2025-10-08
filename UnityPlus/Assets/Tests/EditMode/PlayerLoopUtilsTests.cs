using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine.LowLevel;

namespace UnityPlus.Tests
{
    // Note: tests modify the PlayerLoop. ResetToDefault is called in SetUp/TearDown.
    public class PlayerLoopUtilsTests
    {
        // simple distinct types used as "system types" in tests
        private class SysA { }
        private class SysB { }
        private class SysC { }
        private class SysParent { }
        private class SysChild { }
        private class SysPrev { }
        private class SysNext { }
        private class SysNew { }

        // distinct static methods so delegates can be compared
        static void DelA1() { }
        static void DelA2() { }
        static void DelB() { }
        static void DelParent() { }
        static void DelChild() { }

        [SetUp]
        public void SetUp()
        {
            PlayerLoopUtils.ResetToDefault();
        }

        [TearDown]
        public void TearDown()
        {
            PlayerLoopUtils.ResetToDefault();
        }

        PlayerLoopSystem MakeSystem( Type t, PlayerLoopSystem.UpdateFunction del = null )
        {
            return new PlayerLoopSystem { type = t, updateDelegate = del };
        }

        // finds index of a child type in the root PlayerLoop subSystemList (returns -1 if not found)
        int IndexInRoot( Type t )
        {
            var root = PlayerLoop.GetCurrentPlayerLoop();
            var arr = root.subSystemList ?? Array.Empty<PlayerLoopSystem>();
            for( int i = 0; i < arr.Length; ++i )
                if( arr[i].type == t ) return i;
            return -1;
        }

        // find subsystem by type at root (returns null if not found)
        PlayerLoopSystem? FindRootSystem( Type t )
        {
            var root = PlayerLoop.GetCurrentPlayerLoop();
            var arr = root.subSystemList ?? Array.Empty<PlayerLoopSystem>();
            foreach( var s in arr ) if( s.type == t ) return s;
            return null;
        }

        [Test]
        public void AddSystem_ReplacesExisting()
        {
            PlayerLoopUtils.ResetToDefault();

            // Add a system, then add another system with same type but different delegate -> must replace
            var sys1 = MakeSystem( typeof( SysA ), new PlayerLoopSystem.UpdateFunction( DelA1 ) );
            PlayerLoopUtils.AddSystem( ref sys1 );

            var sys2 = MakeSystem( typeof( SysA ), new PlayerLoopSystem.UpdateFunction( DelA2 ) );
            PlayerLoopUtils.AddSystem( ref sys2 );

            var found = FindRootSystem( typeof( SysA ) );
            Assert.IsTrue( found.HasValue, "SysA should be present in root after adds." );
            Assert.AreEqual( new PlayerLoopSystem.UpdateFunction( DelA2 ), found.Value.updateDelegate,
                "The later AddSystem should have replaced the earlier delegate." );
        }

        [Test]
        public void InsertSystem_BeforeExisting()
        {
            PlayerLoopUtils.ResetToDefault();

            // Add a system, then add another system with same type but different delegate -> must replace
            var sys2 = MakeSystem( typeof( SysChild ), new PlayerLoopSystem.UpdateFunction( DelChild ) );
            PlayerLoopUtils.AddSystem( ref sys2 );

            var sys3 = MakeSystem( typeof( SysNext ), new PlayerLoopSystem.UpdateFunction( DelChild ) );
            PlayerLoopUtils.InsertSystemBefore( ref sys3, typeof( SysChild ) );

            var indC = IndexInRoot( typeof( SysChild ) );
            var indN = IndexInRoot( typeof( SysNext ) );
            Assert.That( indC, Is.GreaterThan( indN ), "indC should be > indN." );
        }

        [Test]
        public void InsertSystem_BeforeNonExistent()
        {
            PlayerLoopUtils.ResetToDefault();

            // Add a system, then add another system with same type but different delegate -> must replace
            var sys3 = MakeSystem( typeof( SysNext ), new PlayerLoopSystem.UpdateFunction( DelChild ) );
            PlayerLoopUtils.InsertSystemBefore( ref sys3, typeof( SysChild ) );

            var sys2 = MakeSystem( typeof( SysChild ), new PlayerLoopSystem.UpdateFunction( DelChild ) );
            PlayerLoopUtils.AddSystem( ref sys2 );

            var indC = IndexInRoot( typeof( SysChild ) );
            var indN = IndexInRoot( typeof( SysNext ) );
            Assert.That( indC, Is.GreaterThan( indN ), "indC should be > indN." );
        }

        [Test]
        public void AddChildBeforeParent_IsOrphanedAndReattachedWhenParentAdded()
        {
            PlayerLoopUtils.ResetToDefault();

            // Add child under a parent that doesn't exist yet -> child is orphaned; when parent is added it should be reattached.
            var child = MakeSystem( typeof( SysChild ), new PlayerLoopSystem.UpdateFunction( DelChild ) );
            PlayerLoopUtils.AddSystem<SysParent>( ref child ); // parent not present -> should become orphaned

            // child should NOT be present at root
            Assert.IsFalse( IndexInRoot( typeof( SysChild ) ) >= 0, "Orphaned child must not appear at the root." );

            var parent = MakeSystem( typeof( SysParent ), new PlayerLoopSystem.UpdateFunction( DelParent ) );
            PlayerLoopUtils.AddSystem( ref parent );

            // now parent should exist and should contain the child
            var parentSys = FindRootSystem( typeof( SysParent ) );
            Assert.IsTrue( parentSys.HasValue, "Parent should be present after AddSystem." );
            Assert.IsNotNull( parentSys.Value.subSystemList, "Parent should have subSystemList after reattaching orphaned child." );
            Assert.IsTrue( parentSys.Value.subSystemList.Any( s => s.type == typeof( SysChild ) ),
                "Child should be reattached under parent when parent is added." );
        }

        [Test]
        public void RemoveParent_StoresChildrenAndReattachWhenParentAddedAgain()
        {
            PlayerLoopUtils.ResetToDefault();

            // Add parent then add a child under it, remove parent, then re-add parent and ensure child reattached.
            var parent = MakeSystem( typeof( SysParent ), new PlayerLoopSystem.UpdateFunction( DelParent ) );
            PlayerLoopUtils.AddSystem( ref parent );

            var child = MakeSystem( typeof( SysChild ), new PlayerLoopSystem.UpdateFunction( DelChild ) );
            PlayerLoopUtils.AddSystem<SysParent>( ref child );

            // verify attached first
            var parentSys = FindRootSystem( typeof( SysParent ) );
            Assert.IsTrue( parentSys.HasValue && parentSys.Value.subSystemList != null
                && parentSys.Value.subSystemList.Any( s => s.type == typeof( SysChild ) ) );

            // remove parent -> child should be saved as orphan
            PlayerLoopUtils.RemoveSystem( ref parent );

            // parent should not exist in root now
            Assert.IsFalse( IndexInRoot( typeof( SysParent ) ) >= 0, "Parent should have been removed from the root." );

            // re-add parent and check child reattached
            var parentNew = MakeSystem( typeof( SysParent ), new PlayerLoopSystem.UpdateFunction( DelParent ) );
            PlayerLoopUtils.AddSystem( ref parentNew );

            var parentAfter = FindRootSystem( typeof( SysParent ) );
            Assert.IsTrue( parentAfter.HasValue && parentAfter.Value.subSystemList != null
                && parentAfter.Value.subSystemList.Any( s => s.type == typeof( SysChild ) ),
                "Child should be reattached when parent is re-added." );
        }

        [Test]
        public void RemoveOrphanedChild_RemovesFromOrphanedAndNotReattached()
        {
            PlayerLoopUtils.ResetToDefault();

            // Add a child before its parent (orphaned), then remove the orphan. When parent is added later, child should NOT be reattached.
            var child = MakeSystem( typeof( SysChild ), new PlayerLoopSystem.UpdateFunction( DelChild ) );
            PlayerLoopUtils.AddSystem<SysParent>( ref child ); // orphaned

            // remove orphaned child (should not throw)
            PlayerLoopUtils.RemoveSystem<SysParent>( ref child );

            // now add the parent; child should NOT be reattached
            var parent = MakeSystem( typeof( SysParent ), new PlayerLoopSystem.UpdateFunction( DelParent ) );
            PlayerLoopUtils.AddSystem( ref parent );

            var parentSys = FindRootSystem( typeof( SysParent ) );
            if( parentSys.HasValue && parentSys.Value.subSystemList != null )
            {
                Assert.IsFalse( parentSys.Value.subSystemList.Any( s => s.type == typeof( SysChild ) ),
                    "Removed orphaned child must not be reattached when parent is later added." );
            }
            else
            {
                // No subsystems -- that's fine: child certainly not attached.
                Assert.Pass();
            }
        }

        [Test]
        public void InsertAfter_WhenReferencedMissing_IsAdjustedWhenReferencedAdded()
        {
            PlayerLoopUtils.ResetToDefault();

            // Insert A after B when B does not yet exist; A will be present and when B is added later B must end up before A.
            var sysA = MakeSystem( typeof( SysA ), new PlayerLoopSystem.UpdateFunction( DelA1 ) );
            PlayerLoopUtils.InsertSystemAfter( ref sysA, typeof( SysB ) ); // B missing currently

            Assert.IsTrue( IndexInRoot( typeof( SysA ) ) >= 0, "A should be inserted immediately even though its reference B is missing." );

            var sysB = MakeSystem( typeof( SysB ), new PlayerLoopSystem.UpdateFunction( DelB ) );
            PlayerLoopUtils.AddSystem( ref sysB ); // add the reference later

            var idxA = IndexInRoot( typeof( SysA ) );
            var idxB = IndexInRoot( typeof( SysB ) );
            Assert.Greater( idxA, idxB, "After adding B later, B must appear before A (A requested to be after B)." );
        }

        [Test]
        public void InsertBefore_WhenReferencedMissing_IsAdjustedWhenReferencedAdded()
        {
            PlayerLoopUtils.ResetToDefault();

            // Insert A before B when B does not yet exist; A will be present and when B is added later B must end up after A.
            var sysA = MakeSystem( typeof( SysA ), new PlayerLoopSystem.UpdateFunction( DelA1 ) );
            PlayerLoopUtils.InsertSystemBefore( ref sysA, typeof( SysB ) ); // B missing currently

            Assert.IsTrue( IndexInRoot( typeof( SysA ) ) >= 0, "A should be inserted immediately even though its reference B is missing." );

            var sysB = MakeSystem( typeof( SysB ), new PlayerLoopSystem.UpdateFunction( DelB ) );
            PlayerLoopUtils.AddSystem( ref sysB ); // add the reference later

            var idxA = IndexInRoot( typeof( SysA ) );
            var idxB = IndexInRoot( typeof( SysB ) );
            Assert.Less( idxA, idxB, "After adding B later, B must appear after A (A requested to be before B)." );
        }

        [Test]
        public void InsertConstraintOverwrite_LastCallWins()
        {
            PlayerLoopUtils.ResetToDefault();

            // Insert A after B, then insert A after C -> the last insert should win, so A must appear after C once C is added.
            var sysA = MakeSystem( typeof( SysA ), new PlayerLoopSystem.UpdateFunction( DelA1 ) );
            PlayerLoopUtils.InsertSystemAfter( ref sysA, typeof( SysB ) );
            // overwrite constraint
            PlayerLoopUtils.InsertSystemAfter( ref sysA, typeof( SysC ) );

            // add C and B (order doesn't matter); ensure A ends up after C (because last constraint asked for after C)
            var sysC = MakeSystem( typeof( SysC ) );
            PlayerLoopUtils.AddSystem( ref sysC );

            var sysB = MakeSystem( typeof( SysB ) );
            PlayerLoopUtils.AddSystem( ref sysB );

            var idxA = IndexInRoot( typeof( SysA ) );
            var idxC = IndexInRoot( typeof( SysC ) );
            Assert.Greater( idxA, idxC, "After overwriting constraint, A must be after C (last constraint)." );
        }
    }
}
