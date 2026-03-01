using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_ExtendedTests
    {
        class Hello
        {
            public string a;
        }

        [SetUp]
        public void Init()
        {
            TypeDescriptorRegistry.Clear();
        }

        [TearDown]
        public void Cleanup()
        {
            TypeDescriptorRegistry.Clear();
        }

        // --- Unity Primitives ---

        [Test]
        public void Structure_Unity_Bounds()
        {
            var b = new Bounds( new Vector3( 1, 2, 3 ), new Vector3( 4, 5, 6 ) );
            var data = SerializationUnit.Serialize( b );

            Assert.That( data, Is.InstanceOf<SerializedObject>() );
            var obj = (SerializedObject)data;

            // Bounds uses a ClassOrStructDescriptor, so it keys by name
            Assert.That( obj.ContainsKey( "center" ), Is.True );
            Assert.That( obj.ContainsKey( "extents" ), Is.True );

            var center = (SerializedArray)obj["center"];
            Assert.That( (float)center[0], Is.EqualTo( 1f ) );

            var result = SerializationUnit.Deserialize<Bounds>( data );
            Assert.That( result, Is.EqualTo( b ) );
        }

        [Test]
        public void Structure_Unity_Rect()
        {
            var r = new Rect( 10, 20, 100, 200 );
            var data = SerializationUnit.Serialize( r );

            Assert.That( data, Is.InstanceOf<SerializedObject>() );
            var obj = (SerializedObject)data;

            Assert.That( (float)obj["x"], Is.EqualTo( 10f ) );
            Assert.That( (float)obj["width"], Is.EqualTo( 100f ) );

            var result = SerializationUnit.Deserialize<Rect>( data );
            Assert.That( result, Is.EqualTo( r ) );
        }

        // --- Collections ---

        [Test]
        public void Collection_NullElements()
        {
            var list = new List<string> { "A", null, "C" };
            var data = SerializationUnit.Serialize( list );

            var obj = (SerializedObject)data;
            var arr = (SerializedArray)obj[KeyNames.VALUE];

            Assert.That( arr.Count, Is.EqualTo( 3 ) );
            Assert.That( (string)arr[0], Is.EqualTo( "A" ) );
            Assert.That( arr[1], Is.Null );
            Assert.That( (string)arr[2], Is.EqualTo( "C" ) );

            var result = SerializationUnit.Deserialize<List<string>>( data );
            Assert.That( result.Count, Is.EqualTo( 3 ) );
            Assert.That( result[1], Is.Null );
        }

        [Test]
        public void Collection_NullElements2()
        {
            var list = new List<Hello> { null, null };
            var data = SerializationUnit.Serialize( list );

            var obj = (SerializedObject)data;
            var arr = (SerializedArray)obj[KeyNames.VALUE];

            Assert.That( arr.Count, Is.EqualTo( 2 ) );
            Assert.That( arr[0], Is.Null );
            Assert.That( arr[1], Is.Null );

            var result = SerializationUnit.Deserialize<List<Hello>>( data );
            Assert.That( result.Count, Is.EqualTo( 2 ) );
            Assert.That( result[0], Is.Null );
            Assert.That( result[1], Is.Null );
        }

        [Test]
        public void Collection_NullElements3()
        {
            var list = new List<Hello> { null, null };
            var data = SerializationUnit.Serialize( list );

            var obj = (SerializedObject)data;
            var arr = (SerializedArray)obj[KeyNames.VALUE];

            Assert.That( arr.Count, Is.EqualTo( 2 ) );
            Assert.That( arr[0], Is.Null );
            Assert.That( arr[1], Is.Null );

            list[0] = new Hello();
            list[1] = new Hello();

            SerializationUnit.Populate( list, data );

            Assert.That( list.Count, Is.EqualTo( 2 ) );
            Assert.That( list[0], Is.Null );
            Assert.That( list[1], Is.Null );
        }

        [Test]
        public void Collection_CircularReference()
        {
            var list = new List<object>();
            list.Add( list ); // Add self

            var data = SerializationUnit.Serialize( list );

            // Structure: Wrapped Object -> Values Array -> Element 0 is Ref to Wrapped Object
            var wrapper = (SerializedObject)data;
            var values = (SerializedArray)wrapper[KeyNames.VALUE];
            var element = (SerializedObject)values[0];

            Assert.That( element.ContainsKey( KeyNames.REF ), Is.True );

            var result = SerializationUnit.Deserialize<List<object>>( data );
            Assert.That( result.Count, Is.EqualTo( 1 ) );
            Assert.That( result[0], Is.SameAs( result ) );
        }

        // --- Configuration ---

        [Test]
        public void Configuration_ForceStandardJson()
        {
            var list = new List<int> { 1, 2, 3 };

            // Manually configure context
            var config = new SerializationConfiguration { ForceStandardJson = true };
            var data = SerializationUnit.Serialize( list, config );

            // Should be a direct array, NO wrapper
            Assert.That( data, Is.InstanceOf<SerializedArray>() );
            var arr = (SerializedArray)data;
            Assert.That( arr.Count, Is.EqualTo( 3 ) );
        }

        // --- Context ---

        public class ContextTestClass { public int Value; }

        [Test]
        public void Context_BasicUsage()
        {
            // Define a custom context key
            int customId = 1000;
            var myContext = new ContextKey( customId );
            ContextRegistry.RegisterName( customId, "MyContext" );

            // Register a special descriptor ONLY for this context
            // We'll map "Value" to a constant 999 for testing
            var customDesc = new MemberwiseDescriptor<ContextTestClass>()
                .WithMember( "Value", o => 999, ( ref ContextTestClass o, int v ) => o.Value = v );

            TypeDescriptorRegistry.Register( customDesc, myContext );

            var obj = new ContextTestClass { Value = 42 };

            // 1. Default Context Serialization
            var defaultData = SerializationUnit.Serialize( obj );
            var defaultObj = (SerializedObject)defaultData;
            Assert.That( (int)defaultObj["Value"], Is.EqualTo( 42 ) );

            // 2. Custom Context Serialization
            var customData = SerializationUnit.Serialize( myContext, obj );
            var customObj = (SerializedObject)customData;
            Assert.That( (int)customObj["Value"], Is.EqualTo( 999 ) );
        }

        [Test]
        public void Populate_IgnoreMissing()
        {
            var obj = new SerializationV4_DataStructureTests.SimpleClass { ID = 10, Name = "Original" };

            // Data only has ID
            var data = new SerializedObject { ["ID"] = (SerializedPrimitive)20 };

            SerializationUnit.Populate( obj, data );

            Assert.That( obj.ID, Is.EqualTo( 20 ) );
            Assert.That( obj.Name, Is.EqualTo( "Original" ) ); // Should NOT be null
        }
    }
}