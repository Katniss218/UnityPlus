
using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_StructureTests
    {
        // --- Mocks ---
        public class Node
        {
            public string Name;
            public Node Neighbor;
        }

        public struct DeepStruct
        {
            public int Value;
        }

        public struct MiddleStruct
        {
            public DeepStruct Deep;
            public string Tag;
        }

        public class RootClass
        {
            public MiddleStruct Mid;
            public string Name;
        }

        [SetUp]
        public void Init()
        {
            // Ensure we start with a clean slate, free from pollution by other tests (e.g. FailureTests)
            TypeDescriptorRegistry.Clear();
        }

        [Test]
        public void Serialize_DeepStruct_WriteBack()
        {
            // This tests the critical StackMachine "Write-Back" logic for value types.
            // Stack: [Root (Class)] -> [Mid (Boxed Struct)] -> [Deep (Boxed Struct)] -> [Value (Int)]
            // When Value pops, it writes to Boxed Deep.
            // When Boxed Deep pops, it writes to Boxed Mid.
            // When Boxed Mid pops, it writes to Root.

            var root = new RootClass
            {
                Name = "Root",
                Mid = new MiddleStruct
                {
                    Tag = "Mid",
                    Deep = new DeepStruct { Value = 999 }
                }
            };

            // Round trip
            var data = SerializationUnit.Serialize( root );
            var result = SerializationUnit.Deserialize<RootClass>( data );

            Assert.That( result.Name, Is.EqualTo( "Root" ) );
            Assert.That( result.Mid.Tag, Is.EqualTo( "Mid" ) );
            Assert.That( result.Mid.Deep.Value, Is.EqualTo( 999 ) );
        }

        [Test]
        public void Serialize_CircularDependency_DeferredResolution()
        {
            // A <-> B
            var a = new Node { Name = "A" };
            var b = new Node { Name = "B" };
            a.Neighbor = b;
            b.Neighbor = a;

            var data = SerializationUnit.Serialize( a );

            // Check Data Structure: A should contain B, B should contain Ref(A)
            var objA = (SerializedObject)data;
            var objB = (SerializedObject)objA["Neighbor"];
            var refA = (SerializedObject)objB["Neighbor"];

            Assert.That( refA.ContainsKey( KeyNames.REF ), Is.True );

            // Deserialize
            var resultA = SerializationUnit.Deserialize<Node>( data );

            Assert.That( resultA.Name, Is.EqualTo( "A" ) );
            Assert.That( resultA.Neighbor.Name, Is.EqualTo( "B" ) );
            Assert.That( resultA.Neighbor.Neighbor, Is.SameAs( resultA ), "Circular reference was not resolved correctly." );
        }

        [Test]
        public void Serialize_SelfReference()
        {
            var node = new Node { Name = "Self" };
            node.Neighbor = node;

            var data = SerializationUnit.Serialize( node );
            var result = SerializationUnit.Deserialize<Node>( data );

            Assert.That( result.Neighbor, Is.SameAs( result ) );
        }
    }
}
