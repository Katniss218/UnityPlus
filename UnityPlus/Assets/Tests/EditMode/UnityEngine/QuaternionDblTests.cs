using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEngineTests
{
    public class QuaternionDblTests
    {
        [Test]
        public void QuaternionDbl_AngleAxis___SameAsUnityQuaternion()
        {
            // Arrange
            double x = 43.234436458275351, y = 2.34232352342464352, z = -91.34423243554663455;
            double angle = -45.74562;

            // Act
            Quaternion reference = Quaternion.AngleAxis( (float)angle, new Vector3( (float)x, (float)y, (float)z ) );
            QuaternionDbl tested = QuaternionDbl.AngleAxis( angle, new Vector3Dbl( x, y, z ) );

            // Assert
            Assert.That( tested.x, Is.EqualTo( reference.x ).Within( 0.00001 ) );
            Assert.That( tested.y, Is.EqualTo( reference.y ).Within( 0.00001 ) );
            Assert.That( tested.z, Is.EqualTo( reference.z ).Within( 0.00001 ) );
            Assert.That( tested.w, Is.EqualTo( reference.w ).Within( 0.00001 ) );
        }
        
        [Test]
        public void QuaternionDbl_ToAngleAxis___SameAsUnityQuaternion()
        {
            // Arrange
            Quaternion reference = Quaternion.AngleAxis( -45.74562f, new Vector3( 43.234436458275351f, 2.34232352342464352f, -91.34423243554663455f ) );
            QuaternionDbl tested = QuaternionDbl.AngleAxis( -45.74562, new Vector3Dbl( 43.234436458275351, 2.34232352342464352, -91.34423243554663455 ) );

            // Act
            reference.ToAngleAxis( out float angle, out Vector3 referenceAxis );
            tested.ToAngleAxis( out double testedAngle, out Vector3Dbl testedAxis );

            // Assert
            Assert.That( testedAxis.x, Is.EqualTo( referenceAxis.x ).Within( 0.00001 ) );
            Assert.That( testedAxis.y, Is.EqualTo( referenceAxis.y ).Within( 0.00001 ) );
            Assert.That( testedAxis.z, Is.EqualTo( referenceAxis.z ).Within( 0.00001 ) );
            Assert.That( testedAngle, Is.EqualTo( angle ).Within( 0.00001 ) ); // for some reason they differ, even though my formula seems right (lolwut)
        }
        
        [Test]
        public void QuaternionDbl_Euler___SameAsUnityQuaternion()
        {
            // Arrange
            double x = 43.234436458275351, y = 2.34232352342464352, z = -91.34423243554663455;

            // Act
            Quaternion reference = Quaternion.Euler( (float)x, (float)y, (float)z );
            QuaternionDbl tested = QuaternionDbl.Euler( x, y, z );

            // Assert
            Assert.That( tested.x, Is.EqualTo( reference.x ).Within( 0.00001 ) );
            Assert.That( tested.y, Is.EqualTo( reference.y ).Within( 0.00001 ) );
            Assert.That( tested.z, Is.EqualTo( reference.z ).Within( 0.00001 ) );
            Assert.That( tested.w, Is.EqualTo( reference.w ).Within( 0.00001 ) );
        }

        [Test]
        public void QuaternionDbl_LookRotation___SameAsUnityQuaternion()
        {
            // Arrange

            // Act
            Quaternion reference = Quaternion.LookRotation( new Vector3( -55, 33, -54 ), new Vector3( -32, 76, 32 ) );
            QuaternionDbl tested = QuaternionDbl.LookRotation( new Vector3( -55, 33, -54 ), new Vector3( -32, 76, 32 ) );

            // Assert
            Assert.That( tested.x, Is.EqualTo( reference.x ).Within( 0.00001 ) );
            Assert.That( tested.y, Is.EqualTo( reference.y ).Within( 0.00001 ) );
            Assert.That( tested.z, Is.EqualTo( reference.z ).Within( 0.00001 ) );
            Assert.That( tested.w, Is.EqualTo( reference.w ).Within( 0.00001 ) );
        }

        [Test]
        public void QuaternionDbl_FromToRotation___SameAsUnityQuaternion()
        {
            // Arrange

            // Act
            Quaternion reference = Quaternion.FromToRotation( new Vector3( -55, 33, -54 ), new Vector3( -32, 76, 32 ) );
            QuaternionDbl tested = QuaternionDbl.FromToRotation( new Vector3( -55, 33, -54 ), new Vector3( -32, 76, 32 ) );

            // Assert
            Assert.That( tested.x, Is.EqualTo( reference.x ).Within( 0.00001 ) );
            Assert.That( tested.y, Is.EqualTo( reference.y ).Within( 0.00001 ) );
            Assert.That( tested.z, Is.EqualTo( reference.z ).Within( 0.00001 ) );
            Assert.That( tested.w, Is.EqualTo( reference.w ).Within( 0.00001 ) );
        }
    }
}