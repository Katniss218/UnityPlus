
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization.ReferenceMaps;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4Tests
    {
        private SerializationContext _context;
        private StackMachineDriver _driver;

        [SetUp]
        public void Setup()
        {
            _context = new SerializationContext
            {
                ForwardMap = new ForwardReferenceStore(),
                ReverseMap = new ReverseReferenceStore()
            };
            _driver = new StackMachineDriver( _context );
        }

        private void RunDriver()
        {
            // Fail-safe to prevent infinite loops in tests if logic breaks
            int maxTicks = 10000;
            int ticks = 0;
            while( !_driver.IsFinished && ticks++ < maxTicks )
            {
                _driver.Tick( 100 );
            }

            if( !_driver.IsFinished )
            {
                Assert.Fail( "StackMachineDriver failed to finish within tick limit." );
            }
        }

        [Test]
        public void Serialize_Int_Primitive()
        {
            int value = 42;
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( int ) );

            _driver.Initialize( value, desc, new SerializationStrategy() );
            RunDriver();

            var result = (SerializedPrimitive)_driver.Result;
            Assert.That( (int)result, Is.EqualTo( 42 ) );
        }

        [Test]
        public void Serialize_Deserialize_Vector3()
        {
            Vector3 original = new Vector3( 1.5f, 2.5f, 3.5f );
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( Vector3 ) );

            // Serialize
            _driver.Initialize( original, desc, new SerializationStrategy() );
            RunDriver();
            var data = (SerializedData)_driver.Result;

            // Deserialize
            _driver.Initialize( null, desc, new DeserializationStrategy(), data );
            RunDriver();
            var result = (Vector3)_driver.Result;

            Assert.That( result, Is.EqualTo( original ) );
        }

        [Test]
        public void Serialize_Deserialize_Class_Reflection()
        {
            var original = new TestClass { Name = "Test", Value = 123 };
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( TestClass ) );

            // Serialize
            _driver.Initialize( original, desc, new SerializationStrategy() );
            RunDriver();
            var data = (SerializedData)_driver.Result;

            // Deserialize
            _driver.Initialize( null, desc, new DeserializationStrategy(), data );
            RunDriver();
            var result = (TestClass)_driver.Result;

            Assert.That( result, Is.Not.Null );
            Assert.That( result.Name, Is.EqualTo( original.Name ) );
            Assert.That( result.Value, Is.EqualTo( original.Value ) );
        }

        [Test]
        public void Serialize_Deserialize_Circular_Dependency()
        {
            var a = new Node { Name = "A" };
            var b = new Node { Name = "B" };
            a.Neighbor = b;
            b.Neighbor = a;

            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( Node ) );

            // Serialize A (should include B)
            _driver.Initialize( a, desc, new SerializationStrategy() );
            RunDriver();
            var data = (SerializedData)_driver.Result;

            // Deserialize
            // We expect the circular reference to be resolved via Deferred Queue
            _driver.Initialize( null, desc, new DeserializationStrategy(), data );
            RunDriver();
            var resultA = (Node)_driver.Result;

            Assert.That( resultA, Is.Not.Null );
            Assert.That( resultA.Name, Is.EqualTo( "A" ) );
            Assert.That( resultA.Neighbor, Is.Not.Null );
            Assert.That( resultA.Neighbor.Name, Is.EqualTo( "B" ) );
            // Check referential integrity
            Assert.That( resultA.Neighbor.Neighbor, Is.SameAs( resultA ) );
        }

        [Test]
        public void Serialize_Deserialize_Array()
        {
            int[] original = new int[] { 1, 2, 3, 4, 5 };
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( int[] ) );

            // Serialize
            _driver.Initialize( original, desc, new SerializationStrategy() );
            RunDriver();
            var data = (SerializedData)_driver.Result;

            // Deserialize
            _driver.Initialize( null, desc, new DeserializationStrategy(), data );
            RunDriver();
            var result = (int[])_driver.Result;

            Assert.That( result, Is.Not.Null );
            Assert.That( result, Is.EqualTo( original ) );
        }

        [Test]
        public void Serialize_Deserialize_List()
        {
            List<string> original = new List<string> { "Hello", "World" };
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( List<string> ) );

            // Serialize
            _driver.Initialize( original, desc, new SerializationStrategy() );
            RunDriver();
            var data = (SerializedData)_driver.Result;

            // Deserialize
            _driver.Initialize( null, desc, new DeserializationStrategy(), data );
            RunDriver();
            var result = (List<string>)_driver.Result;

            Assert.That( result, Is.Not.Null );
            Assert.That( result, Is.EqualTo( original ) );
        }

        [Test]
        public void Serialize_Deserialize_Dictionary()
        {
            Dictionary<string, int> original = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( Dictionary<string, int> ) );

            // Serialize
            _driver.Initialize( original, desc, new SerializationStrategy() );
            RunDriver();
            var data = (SerializedData)_driver.Result;

            // Deserialize
            _driver.Initialize( null, desc, new DeserializationStrategy(), data );
            RunDriver();
            var result = (Dictionary<string, int>)_driver.Result;

            Assert.That( result, Is.Not.Null );
            Assert.That( result, Is.EquivalentTo( original ) );
        }

        [Test]
        public void Lifecycle_Callbacks_Invoked()
        {
            var obj = new CallbackTestClass();
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( CallbackTestClass ) );

            // Serialize
            _driver.Initialize( obj, desc, new SerializationStrategy() );
            RunDriver();
            Assert.That( obj.Serialized, Is.True, "OnBeforeSerialize should be called." );

            // Deserialize
            var data = (SerializedData)_driver.Result;
            _driver.Initialize( null, desc, new DeserializationStrategy(), data );
            RunDriver();
            var result = (CallbackTestClass)_driver.Result;

            Assert.That( result.Deserialized, Is.True, "OnAfterDeserialize should be called." );
        }

        // --- Mocks ---

        private class TestClass
        {
            public string Name;
            public int Value;
        }

        private class Node
        {
            public string Name;
            public Node Neighbor;
        }

        private class CallbackTestClass : ISerializationCallbackReceiver
        {
            public bool Serialized = false;
            public bool Deserialized = false;

            public void OnBeforeSerialize() => Serialized = true;
            public void OnAfterDeserialize() => Deserialized = true;
        }
    }
}
