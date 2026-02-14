using NUnit.Framework;
using System.Collections.Generic;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_CollectionTests
    {
        [Test]
        public void Serialize_IntArray()
        {
            int[] arr = { 1, 2, 3 };
            var data = SerializationUnit.Serialize( arr );
            var result = SerializationUnit.Deserialize<int[]>( data );

            Assert.That( result, Is.EqualTo( arr ) );
        }

        [Test]
        public void Serialize_List_Strings()
        {
            List<string> list = new List<string> { "A", "B", "C" };
            var data = SerializationUnit.Serialize( list );
            var result = SerializationUnit.Deserialize<List<string>>( data );

            Assert.That( result, Is.EqualTo( list ) );
        }

        [Test]
        public void Serialize_Dictionary_StringInt()
        {
            var dict = new Dictionary<string, int>
            {
                { "HP", 100 },
                { "MP", 50 }
            };

            var data = SerializationUnit.Serialize( dict );
            var result = SerializationUnit.Deserialize<Dictionary<string, int>>( data );

            Assert.That( result["HP"], Is.EqualTo( 100 ) );
            Assert.That( result["MP"], Is.EqualTo( 50 ) );
        }

        [Test]
        public void Serialize_Dictionary_ObjectKey()
        {
            // Testing DictionaryDescriptor with complex keys (requires KeyValuePair serialization)
            var key1 = new SerializationV4_StructureTests.Node { Name = "Key1" };
            var key2 = new SerializationV4_StructureTests.Node { Name = "Key2" };

            var dict = new Dictionary<SerializationV4_StructureTests.Node, int>
            {
                { key1, 1 },
                { key2, 2 }
            };

            // Register specific descriptor for this generic combo to ensure it works
            // (Though JIT should handle it)
            // TypeDescriptorRegistry.GetDescriptor(typeof(Dictionary<Node, int>)); 

            var data = SerializationUnit.Serialize( dict );
            var result = SerializationUnit.Deserialize<Dictionary<SerializationV4_StructureTests.Node, int>>( data );

            Assert.That( result.Count, Is.EqualTo( 2 ) );

            // Keys are new objects, so we search by content
            bool foundKey1 = false;
            foreach( var kvp in result )
            {
                if( kvp.Key.Name == "Key1" )
                {
                    Assert.That( kvp.Value, Is.EqualTo( 1 ) );
                    foundKey1 = true;
                }
            }
            Assert.That( foundKey1, Is.True );
        }

        [Test]
        public void Deserialize_List_Resizing()
        {
            // Case: Target list has existing items, data has different count.
            var existing = new List<int> { 1, 2, 3, 4, 5 };

            // Data has 2 items
            var data = new SerializedArray { (SerializedPrimitive)10, (SerializedPrimitive)20 };

            SerializationUnit.Populate( existing, data );

            Assert.That( existing.Count, Is.EqualTo( 2 ) );
            Assert.That( existing[0], Is.EqualTo( 10 ) );
            Assert.That( existing[1], Is.EqualTo( 20 ) );
        }

        [Test]
        public void Deserialize_Dictionary_Append()
        {
            var existing = new Dictionary<string, int> { { "A", 1 } };

            // Data has B
            // Format: [ { key: "B", value: 2 } ]
            var kvpData = new SerializedObject { ["key"] = (SerializedPrimitive)"B", ["value"] = (SerializedPrimitive)2 };
            var data = new SerializedArray { kvpData };

            // Dictionary population should reset the items and add the new ones.
            SerializationUnit.Populate( existing, data );

            Assert.That( existing.ContainsKey( "A" ), Is.False ); // Removed
            Assert.That( existing.ContainsKey( "B" ), Is.True ); // Added
            Assert.That( existing["B"], Is.EqualTo( 2 ) );
        }
    }
}