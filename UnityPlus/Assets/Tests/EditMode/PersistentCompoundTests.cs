using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace Serialization
{
	public class PersistentCompoundTests
	{
        [Test]
        public void Vector2___All()
        {
            // Arrange
            Vector2 dataSource = new Vector2( 5.1f, -5.1f );

            // Act
            SerializedData data = dataSource.GetData();
            Vector2 withDataApplied = new Vector2();
            withDataApplied.SetData( data );
            Vector2 deserialized = data.ToVector2();

            // Assert
            Assert.That( withDataApplied, Is.EqualTo( dataSource ) );
            Assert.That( deserialized, Is.EqualTo( dataSource ) );
        }

        [Test]
        public void Vector3___All()
        {
            // Arrange
            Vector3 dataSource = new Vector3( 5.1f, -5.1f, 2f );

            // Act
            SerializedData data = dataSource.GetData();
            Vector3 withDataApplied = new Vector3();
            withDataApplied.SetData( data );
            Vector3 deserialized = data.ToVector3();

            // Assert
            Assert.That( withDataApplied, Is.EqualTo( dataSource ) );
            Assert.That( deserialized, Is.EqualTo( dataSource ) );
        }

        [Test]
        public void Quaternion___All()
        {
            // Arrange
            Quaternion dataSource = new Quaternion( 5.1f, -5.1f, 2f, 5f ).normalized;

            // Act
            SerializedData data = dataSource.GetData();
            Quaternion withDataApplied = new Quaternion();
            withDataApplied.SetData( data );
            Quaternion deserialized = data.ToQuaternion();

            // Assert
            Assert.That( withDataApplied, Is.EqualTo( dataSource ) );
            Assert.That( deserialized, Is.EqualTo( dataSource ) );
        }

        [Test]
        public void Guid___All()
        {
            // Arrange
            Guid dataSource = Guid.NewGuid();

            // Act
            SerializedData data = dataSource.GetData();
            Guid withDataApplied = new Guid();
            withDataApplied.SetData( data );
            Guid deserialized = data.ToGuid();

            // Assert
            Assert.That( withDataApplied, Is.EqualTo( dataSource ) );
            Assert.That( deserialized, Is.EqualTo( dataSource ) );
        }

        [Test]
        public void Type___RoundTrip()
        {
            // Arrange
            Type dataSource = typeof( Dictionary<string, List<int>[]> );

            // Act
            SerializedData data = dataSource.GetData();
            Type deserialized = data.ToType();

            // Assert
            Assert.That( deserialized, Is.EqualTo( dataSource ) );
        }
	}
}