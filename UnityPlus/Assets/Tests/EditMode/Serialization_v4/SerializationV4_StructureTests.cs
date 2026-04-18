using NUnit.Framework;
using UnityPlus.Serialization;

namespace Neoserialization.V4
{
    public class SerializationV4_StructureTests
    {
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
        public void Serialize_SelfReference()
        {
            var node = new Node { Name = "Self" };
            node.Next = node;

            var data = SerializationUnit.Serialize( node, new SerializationConfiguration() { CycleHandling = CycleHandling.AutoRef } );
            var result = SerializationUnit.Deserialize<Node>( data );

            Assert.That( result.Next, Is.SameAs( result ) );
        }
    }
}