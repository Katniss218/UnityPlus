using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Serialization.ComponentData;
using UnityEngine.Serialization.Json;
using UnityEngine.TestTools;

namespace Serialization
{
    public class SerializedObjectTests
    {
        [Test]
        public void AddedContentsRetrievedCorrectly()
        {
            // Arrange
            int i1o = 1;
            bool i3o = true;
            string i5o = "Hi!";

            // Act
            SerializedObject obj = new SerializedObject()
            {
                { "first", i1o },
                { "second", 2 },
                { "third", i3o },
                { "fourth", 4 },
                { "fifth", i5o }
            };

            int i1 = (int)obj["first"];
            bool i3 = (bool)obj["third"];
            string i5 = (string)obj["fifth"];

            // Assert
            Assert.That( i1, Is.EqualTo( i1o ) );
            Assert.That( i3, Is.EqualTo( i3o ) );
            Assert.That( i5, Is.EqualTo( i5o ) );
        }
    }
}