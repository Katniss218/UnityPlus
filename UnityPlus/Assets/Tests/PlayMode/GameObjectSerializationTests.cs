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

        [UnityTest]
        public IEnumerator Deserialize_GameObjectHierarchy_PreservesActiveState()
        {
            // Setup hierarchy
            // Root (Active)
            //   - Child1 (Active)
            //   - Child2 (Inactive)
            //     - GrandChild (Active, but effectively inactive)

            var root = new GameObject( "Root" );
            var child1 = new GameObject( "Child1" );
            child1.transform.SetParent( root.transform );

            var child2 = new GameObject( "Child2" );
            child2.transform.SetParent( root.transform );
            child2.SetActive( false );

            var grandChild = new GameObject( "GrandChild" );
            grandChild.transform.SetParent( child2.transform );
            // GrandChild is activeSelf=true by default, but inactive in hierarchy

            // Serialize
            var data = SerializationUnit.Serialize( root );

            // Cleanup original
            Object.DestroyImmediate( root );

            // Deserialize
            var deserializedRoot = SerializationUnit.Deserialize<GameObject>( data );

            // Verify
            Assert.IsNotNull( deserializedRoot );
            Assert.IsTrue( deserializedRoot.activeSelf, "Root should be active" );

            var dChild1 = deserializedRoot.transform.Find( "Child1" ).gameObject;
            Assert.IsTrue( dChild1.activeSelf, "Child1 should be active" );
            Assert.IsTrue( dChild1.activeInHierarchy, "Child1 should be active in hierarchy" );

            var dChild2 = deserializedRoot.transform.Find( "Child2" ).gameObject;
            Assert.IsFalse( dChild2.activeSelf, "Child2 should be inactive" );
            Assert.IsFalse( dChild2.activeInHierarchy, "Child2 should be inactive in hierarchy" );

            var dGrandChild = dChild2.transform.Find( "GrandChild" ).gameObject;
            Assert.IsTrue( dGrandChild.activeSelf, "GrandChild should be activeSelf (even if parent is inactive)" );
            Assert.IsFalse( dGrandChild.activeInHierarchy, "GrandChild should be inactive in hierarchy" );

            // Cleanup
            Object.DestroyImmediate( deserializedRoot );
            yield return null;
        }
    }
}