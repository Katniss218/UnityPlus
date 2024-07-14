using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.OverridableEvents;
using UnityPlus.Serialization;

namespace OverridableEvents
{
    public class Blacklisting
    {
        [Test]
        public void Blacklisting___NoBlacklist()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", null, null ),
                new OverridableEventListener<string>( "B", null, null ),
                new OverridableEventListener<string>( "C", null, null ),
                new OverridableEventListener<string>( "D", null, null ),
            };

            // Act
            var sortedEvents = events.GetNonBlacklistedListeners().Select( l => l.ID );

            // Assert
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "A", "B", "C", "D" } ) );
        }

        [Test]
        public void Blacklisting___PartialBlacklist()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", new[] { "B" }, null ),
                new OverridableEventListener<string>( "B", null, null ),
                new OverridableEventListener<string>( "C", new[] { "D" }, null ),
                new OverridableEventListener<string>( "D", null, null ),
            };

            // Act
            var sortedEvents = events.GetNonBlacklistedListeners().Select( l => l.ID );

            // Assert
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "A", "C" } ) );
        }

        [Test]
        public void Blacklisting___CircularBlacklist___BothAreBlocked()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", new[] { "B" }, null ),
                new OverridableEventListener<string>( "B", new[] { "A" }, null ),
                new OverridableEventListener<string>( "C", null, null ),
                new OverridableEventListener<string>( "D", null, null ),
            };

            // Act
            var sortedEvents = events.GetNonBlacklistedListeners().Select( l => l.ID );

            // Assert
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "C", "D" } ) );
        }

        [Test]
        public void Blacklisting___MultipleBlacklist()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", new[] { "A", "B", "C" }, null ),
                new OverridableEventListener<string>( "B", null, null ),
                new OverridableEventListener<string>( "C", null, null ),
                new OverridableEventListener<string>( "D", null, null ),
            };

            // Act
            var sortedEvents = events.GetNonBlacklistedListeners().Select( l => l.ID );

            // Assert
            Assert.That( sortedEvents, Is.EqualTo( new string[] { "D" } ) );
        }

        [Test]
        public void Blacklisting___FullBlacklist()
        {
            // Arrange
            IEnumerable<OverridableEventListener<string>> events = new List<OverridableEventListener<string>>()
            {
                new OverridableEventListener<string>( "A", new[] { "B", "C", "D" }, null ),
                new OverridableEventListener<string>( "B", new[] { "A", "C", "D" }, null ),
                new OverridableEventListener<string>( "C", new[] { "A", "B", "D" }, null ),
                new OverridableEventListener<string>( "D", new[] { "A", "B", "C" }, null ),
            };

            // Act
            var sortedEvents = events.GetNonBlacklistedListeners().Select( l => l.ID );

            // Assert
            Assert.That( sortedEvents, Is.Empty );
        }
    }
}