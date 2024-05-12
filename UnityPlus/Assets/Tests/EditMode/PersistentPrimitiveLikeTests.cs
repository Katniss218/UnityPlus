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
    /*
     * 
     * 
     * removed because neoserialization.
     * 
     * 
    public class PersistentPrimitiveLikeTests
    {
        [Test]
        public void RoundTrip___DateTime()
        {
            // Arrange
            DateTime original = new DateTime( 2024, 04, 05 );

            // Act
            DateTime roundTripped = original.GetData().ToDateTime();

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___TimeSpan()
        {
            // Arrange
            TimeSpan original = new TimeSpan( 999, 59, 59 );

            // Act
            TimeSpan roundTripped = original.GetData().ToTimeSpan();

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Guid()
        {
            // Arrange
            Guid original = Guid.NewGuid();

            // Act
            Guid roundTripped = original.SerializeGuid().DeserializeGuid();

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Type()
        {
            // Arrange
            Type original = typeof( Dictionary<string, List<int>[]> );

            // Act
            Type roundTripped = original.GetData().ToType();

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        public class TestClass
        {
            public Func<int, int> del;

            // tests correct bindings by having overloads.

            public int TestMethod( int value ) { return value * 2; }
            public int TestMethod( ref int value ) { return value * 2; }
            public int TestMethod( int value, bool THIS_IS_AN_OVERLOAD ) { return value * 2; }

            private int PrivateTestMethod( int value ) { return value * 2; }
            private int PrivateTestMethod( int value, bool THIS_IS_AN_OVERLOAD ) { return value * 2; }

            public static int StaticTestMethod( int value ) { return value * 2; }
            public static int StaticTestMethod( int value, bool THIS_IS_AN_OVERLOAD ) { return value * 2; }

            public TestClass()
            {
                del = PrivateTestMethod;
            }
        }

        public delegate int ActionDelegate( ref int value );

        [Test]
        public void RoundTrip___Delegate()
        {
            // Arrange
            var target = new TestClass();
            var refStore = new BidirectionalReferenceStore();
            Func<int, int> original = target.TestMethod;

            // Act
            Func<int, int> roundTripped = (Func<int, int>)original.GetData( refStore ).ToDelegate( refStore );

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Delegate_Static()
        {
            // Arrange
            var refStore = new BidirectionalReferenceStore();
            Func<int, int> original = TestClass.StaticTestMethod;

            // Act
            Func<int, int> roundTripped = (Func<int, int>)original.GetData( refStore ).ToDelegate( refStore );

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Delegate_Private()
        {
            // Arrange
            var target = new TestClass();
            var refStore = new BidirectionalReferenceStore();
            Func<int, int> original = target.del;

            // Act
            Func<int, int> roundTripped = (Func<int, int>)original.GetData( refStore ).ToDelegate( refStore );

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Delegate_WithRefParam()
        {
            // Arrange
            var target = new TestClass();
            var refStore = new BidirectionalReferenceStore();
            ActionDelegate original = target.TestMethod;

            // Act
            ActionDelegate roundTripped = (ActionDelegate)original.GetData( refStore ).ToDelegate( refStore );

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Delegate_Lambda()
        {
            // Arrange
            var refStore = new BidirectionalReferenceStore();
            Func<int, int> original = n => n * 2;

            // Act
            Func<int, int> roundTripped = (Func<int, int>)original.GetData( refStore ).ToDelegate( refStore );

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Delegate_Multicasted()
        {
            // Arrange
            var refStore = new BidirectionalReferenceStore();
            var target = new TestClass();
            Func<int, int> original = TestClass.StaticTestMethod;
            original += target.TestMethod;

            // Act
            Func<int, int> roundTripped = (Func<int, int>)original.GetData( refStore ).ToDelegate( refStore );

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Vector2()
        {
            // Arrange
            Vector2 original = new Vector2( 5.1f, -5.1f );

            // Act
            Vector2 roundTripped = original.GetData().ToVector2();

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Vector3()
        {
            // Arrange
            Vector3 original = new Vector3( 5.1f, -5.1f, 2f );

            // Act
            Vector3 roundTripped = original.GetData().ToVector3();

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Vector3Dbl()
        {
            // Arrange
            Vector3Dbl original = new Vector3Dbl( 5.1, -5.1, 2 );

            // Act
            Vector3Dbl roundTripped = original.GetData().ToVector3Dbl();

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___Quaternion()
        {
            // Arrange
            Quaternion original = new Quaternion( 5.1f, -5.1f, 2f, 5f ).normalized;

            // Act
            Quaternion roundTripped = original.GetData().ToQuaternion();

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }

        [Test]
        public void RoundTrip___QuaternionDbl()
        {
            // Arrange
            QuaternionDbl original = new QuaternionDbl( 5.1, -5.1, 2, 5 ).normalized;

            // Act
            QuaternionDbl roundTripped = original.GetData().ToQuaternionDbl();

            // Assert
            Assert.That( roundTripped, Is.EqualTo( original ) );
        }
    }*/
}