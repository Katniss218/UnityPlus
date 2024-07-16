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
            bool wasCircular = false;

            // Act
            var sortedEvents = events.SortDependencies( out wasCircular ).Select( l => l.ID );

            // Assert
            Assert.That( wasCircular, Is.False );
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
            bool wasCircular = false;

            // Act
            var sortedEvents = events.SortDependencies( out wasCircular ).Select( l => l.ID );

            // Assert
            Assert.That( wasCircular, Is.False );
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
            bool wasCircular = false;

            // Act
            var sortedEvents = events.SortDependencies( out wasCircular ).Select( l => l.ID );

            // Assert
            Assert.That( wasCircular, Is.False );
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
            bool wasCircular = false;

            // Act
            var sortedEvents = events.SortDependencies( out wasCircular ).Select( l => l.ID );

            // Assert
            Assert.That( wasCircular, Is.True );
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
            bool wasCircular = false;

            // Act
            var sortedEvents = events.SortDependencies( out wasCircular ).Select( l => l.ID );

            // Assert
            Assert.That( wasCircular, Is.True );
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
            bool wasCircular = false;

            // Act
            var sortedEvents = events.SortDependencies( out wasCircular ).Select( l => l.ID );

            // Assert
            Assert.That( wasCircular, Is.False );
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
            bool wasCircular = false;

            // Act
            var sortedEvents = events.SortDependencies( out wasCircular ).Select( l => l.ID );

            // Assert
            Assert.That( wasCircular, Is.False );
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "A", "B", "C", "D", "E" } ) );
        }

    }
}