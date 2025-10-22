using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus;
using UnityPlus.OverridableEvents;
using UnityPlus.Serialization;

namespace OverridableEvents
{
    public class SortOrder
    {
        [Test]
        public void TopologicalSort___SelfDependency_IsCircular()
        {
            // Arrange
            // Node "A" depends on itself (before A)
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, new[] { "A" }, null, null ),
            };

            // Act
            var sortedEvents = events.SortDependencies( out var circular ).Select( l => l.ID );

            // Assert
            // Self-dependency is a cycle, expect circular detected and no safe ordering returned.
            Assert.That( circular.Any(), Is.True );
            Assert.That( sortedEvents, Is.Empty );
        }

        [Test]
        public void TopologicalSort___UnknownDependency_Ignored_NoCircles()
        {
            // Arrange
            // "A" refers to "Z" which does not exist among nodes. It should not create a cycle.
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, new[] { "Z" }, null, null ),
                new OverridableEventListener<string>( "B", null, null, null, null ),
            };

            // Act
            var sortedEvents = events.SortDependencies( out var circular ).Select( l => l.ID ).ToArray();

            // Assert
            // Unknown dependencies should not produce a circular dependency among provided nodes.
            // Both A and B should still appear in the output
            Assert.That( circular.Any(), Is.False );
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "A", "B" } ) ); // Also checks that nodes are not reordered.
        }

        [Test]
        public void TopologicalSort___DiamondDependency_NoCircles()
        {
            // Arrange
            // Diamond:
            //   A
            //  / \
            // B   C
            //  \ /
            //   D
            //
            // A must come before B and C, both B and C must come before D.
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, null, null, null ),
                new OverridableEventListener<string>( "B", null, null, new[] { "A" }, null ), // B after A
                new OverridableEventListener<string>( "C", null, null, new[] { "A" }, null ), // C after A
                new OverridableEventListener<string>( "D", null, null, new[] { "B", "C" }, null ), // D after B and C
            };

            // Act
            var sortedEvents = events.SortDependencies( out var circular ).Select( l => l.ID ).ToArray();

            // Assert
            Assert.That( circular.Any(), Is.False );

            // Check relative ordering constraints:
            // - A must appear before both B and C.
            // - B and C must appear before D.
            int idxA = Array.IndexOf( sortedEvents, "A" );
            int idxB = Array.IndexOf( sortedEvents, "B" );
            int idxC = Array.IndexOf( sortedEvents, "C" );
            int idxD = Array.IndexOf( sortedEvents, "D" );

            Assert.That( idxA, Is.LessThan( idxB ) );
            Assert.That( idxA, Is.LessThan( idxC ) );
            Assert.That( idxB, Is.LessThan( idxD ) );
            Assert.That( idxC, Is.LessThan( idxD ) );
        }

        [Test]
        public void TopologicalSort___MultipleIndependentChains_PreservedWithinChains()
        {
            // Arrange
            // Two independent chains:
            // Chain1: A -> B -> C  (i.e., B after A, C after B)
            // Chain2: X -> Y      (Y after X)
            // Interleaving initial list should not break per-chain ordering.
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, null, null, null ),
                new OverridableEventListener<string>( "X", null, null, null, null ),
                new OverridableEventListener<string>( "B", null, null, new[] { "A" }, null ),
                new OverridableEventListener<string>( "Y", null, null, new[] { "X" }, null ),
                new OverridableEventListener<string>( "C", null, null, new[] { "B" }, null ),
            };

            // Act
            var sorted = events.SortDependencies( out var circular ).Select( l => l.ID ).ToArray();

            // Assert
            Assert.That( circular.Any(), Is.False );

            // Check chain ordering preserved
            Assert.That( Array.IndexOf( sorted, "A" ), Is.LessThan( Array.IndexOf( sorted, "B" ) ) );
            Assert.That( Array.IndexOf( sorted, "B" ), Is.LessThan( Array.IndexOf( sorted, "C" ) ) );

            Assert.That( Array.IndexOf( sorted, "X" ), Is.LessThan( Array.IndexOf( sorted, "Y" ) ) );

            // Ensure all nodes are present
            Assert.That( sorted, Is.EquivalentTo( new string[] { "A", "B", "C", "X", "Y" } ) );
        }

        [Test]
        public void TopologicalSort___AllUnrelated_OriginalOrderPreserved()
        {
            // Arrange
            // No dependencies at all; expect the algorithm to not unnecessarily reorder nodes.
            // We assert that the returned order equals original order (stable behavior).
            var eventsList = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "first",  null, null, null, null ),
                new OverridableEventListener<string>( "second", null, null, null, null ),
                new OverridableEventListener<string>( "third",  null, null, null, null ),
            };

            // Act
            var sorted = eventsList.SortDependencies( out var circular ).Select( l => l.ID ).ToArray();

            // Assert
            Assert.That( circular.Any(), Is.False );
            Assert.That( sorted, Is.EqualTo( new string[] { "first", "second", "third" } ) );
        }


        [Test]
        public void TopologicalSort___LargeConnectedCycle_Detected()
        {
            // Arrange
            // Create a cycle across several nodes: A -> B -> C -> D -> A
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, null, new[] { "D" }, null ), // A after D
                new OverridableEventListener<string>( "B", null, null, new[] { "A" }, null ), // B after A
                new OverridableEventListener<string>( "C", null, null, new[] { "B" }, null ), // C after B
                new OverridableEventListener<string>( "D", null, null, new[] { "C" }, null ), // D after C
            };

            // Act
            var sorted = events.SortDependencies( out var circular ).Select( l => l.ID ).ToArray();

            // Assert
            // Connected cycle => should indicate a circular dependency and (based on the style of previous tests)
            // return no safe ordering for the connected component.
            Assert.That( circular.Any(), Is.True );
            Assert.That( sorted, Is.Empty );
        }

        [Test]
        public void TopologicalSort___OnlyAfter_NoCircles()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "B", null, null, new[] { "A" }, null ),
                new OverridableEventListener<string>( "A", null, null, null, null ),
                new OverridableEventListener<string>( "D", null, null, new[] { "C" }, null ),
                new OverridableEventListener<string>( "C", null, null, new[] { "B" }, null ),
            };

            // Act
            var sortedEvents = events.SortDependencies( out var circular ).Select( l => l.ID );

            // Assert
            Assert.That( circular.Any(), Is.False );
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "A", "B", "C", "D" } ) );
        }

        [Test]
        public void TopologicalSort___OnlyBefore_NoCircles()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "B", null, new[] { "C" }, null, null ),
                new OverridableEventListener<string>( "A", null, new[] { "B" }, null, null ),
                new OverridableEventListener<string>( "D", null, null, null, null ),
                new OverridableEventListener<string>( "C", null, new[] { "D" }, null, null ),
            };

            // Act
            var sortedEvents = events.SortDependencies( out var circular ).Select( l => l.ID );

            // Assert
            Assert.That( circular.Any(), Is.False );
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "A", "B", "C", "D" } ) );
        }

        [Test]
        public void TopologicalSort___BeforeAndAfter_NoCircles()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "H", null, null, new[] { "G" }, null ),
                new OverridableEventListener<string>( "F", null, null, new[] { "E" }, null ),
                new OverridableEventListener<string>( "E", null, null, new[] { "D" }, null ),
                new OverridableEventListener<string>( "B", null, new[] { "C" }, null, null ),
                new OverridableEventListener<string>( "A", null, new[] { "B" }, null, null ),
                new OverridableEventListener<string>( "D", null, null, null, null ),
                new OverridableEventListener<string>( "G", null, null, new[] { "F" }, null ),
                new OverridableEventListener<string>( "C", null, new[] { "D" }, null, null ),
            };

            // Act
            var sortedEvents = events.SortDependencies( out var circular ).Select( l => l.ID );

            // Assert
            Assert.That( circular.Any(), Is.False );
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "A", "B", "C", "D", "E", "F", "G", "H" } ) );
        }

        [Test]
        public void TopologicalSort___WithDisconnectedCircle()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, new[] { "B" }, null, null ),
                new OverridableEventListener<string>( "B", null, new[] { "A" }, null, null ),
                new OverridableEventListener<string>( "C", null, null, null, null ),
                new OverridableEventListener<string>( "D", null, null, new[] { "C" }, null ),
            };

            // Act
            var sortedEvents = events.SortDependencies( out var circular ).Select( l => l.ID );

            // Assert
            Assert.That( circular.Any(), Is.True );
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "C", "D" } ) );
        }
        
        [Test]
        public void TopologicalSort___WithConnectedCircle()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, new[] { "B" }, null, null ),
                new OverridableEventListener<string>( "B", null, new[] { "A" }, null, null ),
                new OverridableEventListener<string>( "C", null, null, new[] { "B" }, null ), // C -> D is connected to (A <--> B)
                new OverridableEventListener<string>( "D", null, null, new[] { "C" }, null ),
            };

            // Act
            var sortedEvents = events.SortDependencies( out var circular ).Select( l => l.ID );

            // Assert
            Assert.That( circular.Any(), Is.True );
            Assert.That( sortedEvents, Is.Empty );
        }

        [Test]
        public void TopologicalSort___MultipleDependency_NoCircles()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, null, null, null ),
                new OverridableEventListener<string>( "B", null, null, null, null ),
                new OverridableEventListener<string>( "C", null, null, new[] { "A", "B" }, null ),
                new OverridableEventListener<string>( "D", null, null, new[] { "C" }, null ),
            };

            // Act
            var sortedEvents = events.SortDependencies( out var circular ).Select( l => l.ID );

            // Assert
            Assert.That( circular.Any(), Is.False );
            Assert.That( sortedEvents.Skip( 2 ), Is.EqualTo( new string[] { "C", "D" } ) );
        }

        [Test]
        public void TopologicalSort___OnlyAfter_AlreadySorted()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, null, null, null ),
                new OverridableEventListener<string>( "B", null, null, new[] { "A" }, null ),
                new OverridableEventListener<string>( "C", null, null, new[] { "B" }, null ),
                new OverridableEventListener<string>( "D", null, null, new[] { "C" }, null ),
                new OverridableEventListener<string>( "E", null, null, new[] { "D" }, null ),
            };

            // Act
            var sortedEvents = events.SortDependencies( out var circular ).Select( l => l.ID );

            // Assert
            Assert.That( circular.Any(), Is.False );
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "A", "B", "C", "D", "E" } ) );
        }

    }
}