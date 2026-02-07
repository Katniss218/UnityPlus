using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace SerializationV4Tests
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

        [Test]
        public void Serialize_Int_Primitive()
        {
            int value = 42;
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( int ) );

            _driver.Initialize( value, desc, SerializationOperation.Serialize );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Serialize );

            var result = (SerializedPrimitive)_driver.Result;
            Assert.AreEqual( 42, (int)result );
        }

        [Test]
        public void Serialize_Deserialize_Vector3()
        {
            Vector3 original = new Vector3( 1.5f, 2.5f, 3.5f );
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( Vector3 ) );

            // Serialize
            _driver.Initialize( original, desc, SerializationOperation.Serialize );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Serialize );
            var data = (SerializedData)_driver.Result;

            // Deserialize
            _driver.Initialize( null, desc, SerializationOperation.Deserialize, data );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Deserialize );
            var result = (Vector3)_driver.Result;

            Assert.AreEqual( original, result );
        }

        [Test]
        public void Serialize_Deserialize_Class_Reflection()
        {
            var original = new TestClass { Name = "Test", Value = 123 };
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( TestClass ) );

            // Serialize
            _driver.Initialize( original, desc, SerializationOperation.Serialize );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Serialize );
            var data = (SerializedData)_driver.Result;

            // Deserialize
            _driver.Initialize( null, desc, SerializationOperation.Deserialize, data );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Deserialize );
            var result = (TestClass)_driver.Result;

            Assert.IsNotNull( result );
            Assert.AreEqual( original.Name, result.Name );
            Assert.AreEqual( original.Value, result.Value );
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
            _driver.Initialize( a, desc, SerializationOperation.Serialize );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Serialize );
            var data = (SerializedData)_driver.Result;

            // Deserialize
            // We expect the circular reference to be resolved via Deferred Queue
            _driver.Initialize( null, desc, SerializationOperation.Deserialize, data );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Deserialize );
            var resultA = (Node)_driver.Result;

            Assert.IsNotNull( resultA );
            Assert.AreEqual( "A", resultA.Name );
            Assert.IsNotNull( resultA.Neighbor );
            Assert.AreEqual( "B", resultA.Neighbor.Name );
            Assert.AreSame( resultA, resultA.Neighbor.Neighbor );
        }

        [Test]
        public void Serialize_Deserialize_Array()
        {
            int[] original = new int[] { 1, 2, 3, 4, 5 };
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( int[] ) );

            // Serialize
            _driver.Initialize( original, desc, SerializationOperation.Serialize );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Serialize );
            var data = (SerializedData)_driver.Result;

            // Deserialize
            _driver.Initialize( null, desc, SerializationOperation.Deserialize, data );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Deserialize );
            var result = (int[])_driver.Result;

            Assert.AreEqual( original.Length, result.Length );
            for( int i = 0; i < original.Length; i++ ) Assert.AreEqual( original[i], result[i] );
        }

        [Test]
        public void Serialize_Deserialize_List()
        {
            List<string> original = new List<string> { "Hello", "World" };
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( List<string> ) );

            // Serialize
            _driver.Initialize( original, desc, SerializationOperation.Serialize );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Serialize );
            var data = (SerializedData)_driver.Result;

            // Deserialize
            _driver.Initialize( null, desc, SerializationOperation.Deserialize, data );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Deserialize );
            var result = (List<string>)_driver.Result;

            Assert.AreEqual( original.Count, result.Count );
            Assert.AreEqual( original[0], result[0] );
            Assert.AreEqual( original[1], result[1] );
        }

        [Test]
        public void Serialize_Deserialize_Dictionary()
        {
            Dictionary<string, int> original = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( Dictionary<string, int> ) );

            // Serialize
            _driver.Initialize( original, desc, SerializationOperation.Serialize );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Serialize );
            var data = (SerializedData)_driver.Result;

            // Deserialize
            _driver.Initialize( null, desc, SerializationOperation.Deserialize, data );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Deserialize );
            var result = (Dictionary<string, int>)_driver.Result;

            Assert.AreEqual( original.Count, result.Count );
            Assert.AreEqual( 1, result["A"] );
            Assert.AreEqual( 2, result["B"] );
        }

        [Test]
        public void Lifecycle_Callbacks_Invoked()
        {
            var obj = new CallbackTestClass();
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( CallbackTestClass ) );

            // Serialize
            _driver.Initialize( obj, desc, SerializationOperation.Serialize );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Serialize );
            Assert.IsTrue( obj.Serialized, "OnBeforeSerialize not called" );

            // Deserialize
            var data = (SerializedData)_driver.Result;
            _driver.Initialize( null, desc, SerializationOperation.Deserialize, data );
            while( !_driver.IsFinished ) _driver.Tick( 100, SerializationOperation.Deserialize );
            var result = (CallbackTestClass)_driver.Result;

            Assert.IsTrue( result.Deserialized, "OnAfterDeserialize not called" );
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