using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;
using UnityEngine.TestTools;

namespace Serialization
{
    public class SerializedArrayTests
    {
        [Test]
        public void AddedContentsRoundTrippedCorrectly()
        {
            // Arrange
            int i1o = 2;
            int i5o = 11;
            int i9o = 23;

            // Act
            SerializedArray arr = new SerializedArray()
            {
                1, i1o, 3, 5, 7, i5o, 13, 17, 19, i9o
            };

            int i1 = (int)arr[1];
            int i5 = (int)arr[5];
            int i9 = (int)arr[9];

            // Assert
            Assert.That( i1, Is.EqualTo( i1o ) );
            Assert.That( i5, Is.EqualTo( i5o ) );
            Assert.That( i9, Is.EqualTo( i9o ) );
        }
    }
}