using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace Serialization
{
    public class PersistentComponentTests
    {
        [Test]
        public void Transform___RoundTrip()
        {
            // Arrange
            Vector3 localPosition = new Vector3( 1, 2, 3 );
            Quaternion localRotation = Quaternion.Euler( 4, 5, 6 );
            Vector3 localScale = new Vector3( 7, 8, 9 );

            GameObject gameObject = new GameObject();
            Transform transform = gameObject.transform;
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale = localScale;

            // Act
            SerializedData data = ((object)transform).GetData( new BidirectionalReferenceStore() );
            transform.localPosition = default;
            transform.localRotation = default;
            transform.localScale = default;
            transform.SetData( data, new BidirectionalReferenceStore() );

            // Assert
            Assert.That( transform.localPosition, Is.EqualTo( localPosition ) );
            Assert.That( transform.localRotation, Is.EqualTo( localRotation ) );
            Assert.That( transform.localScale, Is.EqualTo( localScale ) );
        }
    }
}