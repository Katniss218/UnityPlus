using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;

namespace UnityPlus.Serialization.Tests
{
    public class GameObjectSerializationTests
    {
        [UnityTest]
        public IEnumerator GameObject_SerializeAndDeserialize_RestoresState()
        {
            // 1. Setup
            var go = new GameObject( "TestGO" );
            go.transform.position = new Vector3( 1, 2, 3 );
            go.tag = "Untagged"; // Default
            go.layer = 0; // Default

            var child = new GameObject( "Child" );
            child.transform.SetParent( go.transform );
            child.transform.localPosition = new Vector3( 0, 1, 0 );

            // Add a component
            var box = go.AddComponent<BoxCollider>();
            box.size = new Vector3( 2, 2, 2 );
            box.isTrigger = true;

            // 2. Serialize
            var serialized = SerializationUnit.Serialize( go );

            Debug.Log( serialized.DumpToString() );

            // 3. Destroy original
            Object.DestroyImmediate( go );

            // 4. Deserialize
            var deserialized = SerializationUnit.Deserialize<GameObject>( serialized );

            // 5. Assert
            Assert.IsNotNull( deserialized );
            Assert.AreEqual( "TestGO", deserialized.name );
            Assert.AreEqual( new Vector3( 1, 2, 3 ), deserialized.transform.position );

            Assert.AreEqual( 1, deserialized.transform.childCount );
            var deserializedChild = deserialized.transform.GetChild( 0 );
            Assert.AreEqual( "Child", deserializedChild.name );
            Assert.AreEqual( new Vector3( 0, 1, 0 ), deserializedChild.localPosition );

            var deserializedBox = deserialized.GetComponent<BoxCollider>();
            Assert.IsNotNull( deserializedBox );
            Assert.AreEqual( new Vector3( 2, 2, 2 ), deserializedBox.size );
            Assert.IsTrue( deserializedBox.isTrigger );

            // Cleanup
            Object.DestroyImmediate( deserialized );
            yield return null;
        }
    }
}
