using System;
using NUnit.Framework;
using UnityEngine;

namespace UnityPlus.Serialization.Tests
{
    public class DelegateTests
    {
        [SetUp]
        public void Setup()
        {
            // Reset registry to pick up the test provider
            TypeDescriptorRegistry.Clear();
        }

        [TearDown]
        public void Teardown()
        {
            TypeDescriptorRegistry.Clear();
        }

        // --- Test Types ---

        private class DelegateContainer
        {
            public Action MyAction;
            public Func<int, int> MyFunc;
        }

        private static void StaticMethod()
        {
            // No-op
        }

        private static int StaticFunc( int x ) => x * 2;

        private class InstanceTarget
        {
            public int Value;
            public void Method() { Value++; }
            public int Func( int x ) => x + Value;
        }

        // --- Tests ---

        [Test]
        public void Delegate_Static_Action_RoundTrips()
        {
            var container = new DelegateContainer
            {
                MyAction = StaticMethod
            };

            var data = SerializationUnit.Serialize( container );
            var result = SerializationUnit.Deserialize<DelegateContainer>( data );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.MyAction, Is.Not.Null );
            Assert.That( result.MyAction, Is.EqualTo( (Action)StaticMethod ) );

            // Verify invocation works (no exception)
            result.MyAction.Invoke();
        }

        [Test]
        public void Delegate_Static_Func_RoundTrips()
        {
            var container = new DelegateContainer
            {
                MyFunc = StaticFunc
            };

            var data = SerializationUnit.Serialize( container );
            Debug.Log( data.DumpToString() );
            var result = SerializationUnit.Deserialize<DelegateContainer>( data );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.MyFunc, Is.Not.Null );
            Assert.That( result.MyFunc( 5 ), Is.EqualTo( 10 ) );
        }

        [Test]
        public void Delegate_Multicast_Static_RoundTrips()
        {
            var container = new DelegateContainer();
            container.MyAction += StaticMethod;
            container.MyAction += StaticMethod; // Add twice

            var data = SerializationUnit.Serialize( container );
            var result = SerializationUnit.Deserialize<DelegateContainer>( data );

            Assert.That( result, Is.Not.Null );
            Assert.That( result.MyAction, Is.Not.Null );
            Assert.That( result.MyAction.GetInvocationList().Length, Is.EqualTo( 2 ) );
        }

        // Helper class for instance tests
        public class TestScriptableTarget : ScriptableObject
        {
            public int Value;
            public void Method() { Value++; }
        }
    }
}
