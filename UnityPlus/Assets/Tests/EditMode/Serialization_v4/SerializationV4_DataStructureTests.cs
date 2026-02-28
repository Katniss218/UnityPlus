using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_DataStructureTests
    {
        [SetUp]
        public void Init()
        {
            TypeDescriptorRegistry.Clear();
        }

        private SerializedData Serialize<T>( T obj )
        {
            return SerializationUnit.Serialize<T>( obj );
        }

        // --- Primitives ---

        [Test]
        public void Structure_Primitive_Int()
        {
            var data = Serialize( 123 );
            Assert.That( data, Is.InstanceOf<SerializedPrimitive>() );
            Assert.That( (int)data, Is.EqualTo( 123 ) );
        }

        [Test]
        public void Structure_Primitive_String()
        {
            var data = Serialize( "Hello" );
            Assert.That( data, Is.InstanceOf<SerializedPrimitive>() );
            Assert.That( (string)data, Is.EqualTo( "Hello" ) );
        }
        
        [Test]
        public void Structure_Null_String()
        {
            var data = Serialize( (string)null );
            Assert.That( data, Is.Null );
        }

        [Test]
        public void Structure_Primitive_Bool()
        {
            var data = Serialize( true );
            Assert.That( data, Is.InstanceOf<SerializedPrimitive>() );
            Assert.That( (bool)data, Is.EqualTo( true ) );
        }

        // --- Unity Primitives ---

        [Test]
        public void Structure_Unity_Vector3()
        {
            var data = Serialize( new Vector3( 1, 2, 3 ) );

            Assert.That( data, Is.InstanceOf<SerializedObject>() );
            var obj = (SerializedObject)data;

            Assert.That( obj.Count, Is.EqualTo( 3 ) );
            Assert.That( (float)obj["x"], Is.EqualTo( 1f ) );
            Assert.That( (float)obj["y"], Is.EqualTo( 2f ) );
            Assert.That( (float)obj["z"], Is.EqualTo( 3f ) );
        }

        [Test]
        public void Structure_Unity_Quaternion()
        {
            var q = new Quaternion( 0.1f, 0.2f, 0.3f, 0.4f );
            var data = Serialize( q );

            Assert.That( data, Is.InstanceOf<SerializedObject>() );
            var obj = (SerializedObject)data;

            Assert.That( (float)obj["x"], Is.EqualTo( 0.1f ) );
            Assert.That( (float)obj["y"], Is.EqualTo( 0.2f ) );
            Assert.That( (float)obj["z"], Is.EqualTo( 0.3f ) );
            Assert.That( (float)obj["w"], Is.EqualTo( 0.4f ) );
        }

        [Test]
        public void Structure_Unity_Color()
        {
            var c = new Color( 0.1f, 0.2f, 0.3f, 0.4f );
            var data = Serialize( c );

            Assert.That( data, Is.InstanceOf<SerializedObject>() );
            var obj = (SerializedObject)data;

            Assert.That( (float)obj["r"], Is.EqualTo( 0.1f ) );
            Assert.That( (float)obj["g"], Is.EqualTo( 0.2f ) );
            Assert.That( (float)obj["b"], Is.EqualTo( 0.3f ) );
            Assert.That( (float)obj["a"], Is.EqualTo( 0.4f ) );
        }

        // --- Collections ---

        [Test]
        public void Structure_Array_Int()
        {
            var data = Serialize( new int[] { 1, 2, 3 } );

            Assert.That( data, Is.InstanceOf<SerializedArray>() );
            var arr = (SerializedArray)data;

            Assert.That( arr.Count, Is.EqualTo( 3 ) );
            Assert.That( (int)arr[0], Is.EqualTo( 1 ) );
            Assert.That( (int)arr[1], Is.EqualTo( 2 ) );
            Assert.That( (int)arr[2], Is.EqualTo( 3 ) );
        }

        [Test]
        public void Structure_List_String()
        {
            var list = new List<string> { "A", "B" };
            var data = Serialize( list );

            Assert.That( data, Is.InstanceOf<SerializedArray>() );
            var arr = (SerializedArray)data;

            Assert.That( arr.Count, Is.EqualTo( 2 ) );
            Assert.That( (string)arr[0], Is.EqualTo( "A" ) );
            Assert.That( (string)arr[1], Is.EqualTo( "B" ) );
        }

        [Test]
        public void Structure_Dictionary()
        {
            var dict = new Dictionary<string, int> { { "A", 1 }, { "B", 2 } };
            var data = Serialize( dict );

            Assert.That( data, Is.InstanceOf<SerializedArray>() );
            var arr = (SerializedArray)data;

            Assert.That( arr.Count, Is.EqualTo( 2 ) );

            // Dictionaries are serialized as array of entries { k, v }
            var entry0 = (SerializedObject)arr[0];
            Assert.That( (string)entry0["k"], Is.EqualTo( "A" ) );
            Assert.That( (int)entry0["v"], Is.EqualTo( 1 ) );

            var entry1 = (SerializedObject)arr[1];
            Assert.That( (string)entry1["k"], Is.EqualTo( "B" ) );
            Assert.That( (int)entry1["v"], Is.EqualTo( 2 ) );
        }

        // --- Classes & Polymorphism ---

        public class SimpleClass
        {
            public int ID;
            public string Name;
        }

        [Test]
        public void Structure_SimpleClass()
        {
            var obj = new SimpleClass { ID = 10, Name = "Test" };
            var data = Serialize( obj );

            Assert.That( data, Is.InstanceOf<SerializedObject>() );
            var sobj = (SerializedObject)data;

            Assert.That( (int)sobj["ID"], Is.EqualTo( 10 ) );
            Assert.That( (string)sobj["Name"], Is.EqualTo( "Test" ) );
        }

        public interface IAnimal { }
        public class Dog : IAnimal { public string Breed; }

        [Test]
        public void Structure_Polymorphism_TypeMarker()
        {
            // When serializing a polymorphic type, it should include _type
            IAnimal animal = new Dog { Breed = "Pug" };
            var data = Serialize( animal );

            Assert.That( data, Is.InstanceOf<SerializedObject>() );
            var sobj = (SerializedObject)data;

            Assert.That( sobj.ContainsKey( KeyNames.TYPE ), Is.True );
            // The type name might vary depending on assembly, but it should be present
            Assert.That( (string)sobj[KeyNames.TYPE], Contains.Substring( "Dog" ) );
            Assert.That( (string)sobj["Breed"], Is.EqualTo( "Pug" ) );
        }

        // --- References ---

        public class Node
        {
            public Node Next;
        }

        [Test]
        public void Structure_Reference_RefMarker()
        {
            var node = new Node();
            node.Next = node; // Circular

            var data = Serialize( node );

            Assert.That( data, Is.InstanceOf<SerializedObject>() );
            var sobj = (SerializedObject)data;

            // First level is the object itself
            Assert.That( sobj.ContainsKey( "Next" ), Is.True );

            var next = (SerializedObject)sobj["Next"];

            // Second level should be a reference
            Assert.That( next.ContainsKey( KeyNames.REF ), Is.True );
            // Since it's the first object, ref ID is likely 0 or 1 depending on implementation details, 
            // but we just check existence here.
        }

        // --- Null ---

        [Test]
        public void Structure_Null()
        {
            var data = Serialize( (object)null );
            Assert.That( data, Is.Null );
        }

        // --- Extended Types ---

        [Test]
        public void Structure_Guid()
        {
            var guid = Guid.NewGuid();
            var data = Serialize( guid );
            Assert.That( data, Is.InstanceOf<SerializedPrimitive>() );
            Assert.That( (string)data, Is.Not.Null.And.Not.Empty );
        }

        [Test]
        public void Structure_DateTime()
        {
            var dt = DateTime.UtcNow;
            var data = Serialize( dt );
            Assert.That( data, Is.InstanceOf<SerializedPrimitive>() );
            // Should be ISO 8601 string
            Assert.That( (string)data, Is.Not.Null.And.Not.Empty );
        }

        public enum TestEnum { A = 1, B = 2 }

        [Test]
        public void Structure_Enum()
        {
            var data = Serialize( TestEnum.B );
            Assert.That( data, Is.InstanceOf<SerializedPrimitive>() );
            // Defaults to integer serialization (long)
            Assert.That( (long)data, Is.EqualTo( 2 ) );
        }
    }
}