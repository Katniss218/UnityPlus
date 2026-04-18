using NUnit.Framework;
using System.Collections.Generic;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Descriptors;

namespace Neoserialization.V4
{
    public class SerializationV4_ExtendedTests
    {
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

        private class TestWithInitialized
        {
            public object Value { get; set; } = new Dictionary<string, object>();


            [MapsInheritingFrom( typeof( TestWithInitialized ) )]
            public static IDescriptor InitialMapping()
            {
                return new MemberwiseDescriptor<TestWithInitialized>()
                    .WithMember( "value", o => o.Value );
            }
        }

        private interface ITest
        {
            object GetObj();
        }

        private abstract class Test
        {
        }

        private class TestClass : Test, ITest
        {
            public object Value { get; set; }
            public object GetObj()
            {
                return Value;
            }

            [MapsInheritingFrom( typeof( TestClass ) )]
            public static IDescriptor InitialMapping()
            {
                return new MemberwiseDescriptor<TestClass>()
                    .WithMember( "value", o => o.Value );
            }
        }

        private class TestClass2 : Test, ITest
        {
            public int Num { get; set; }
            public object GetObj()
            {
                return Num;
            }

            [MapsInheritingFrom( typeof( TestClass2 ) )]
            public static IDescriptor InitialMapping()
            {
                return new MemberwiseDescriptor<TestClass2>()
                    .WithMember( "num", o => o.Num );
            }
        }

        private class TestWithInitializedInterface
        {
            public ITest Inter { get; set; } = new TestClass();


            [MapsInheritingFrom( typeof( TestWithInitializedInterface ) )]
            public static IDescriptor InitialMapping()
            {
                return new MemberwiseDescriptor<TestWithInitializedInterface>()
                    .WithMember( "inter", o => o.Inter );
            }
        }
        
        private class TestWithInitializedBaseClass
        {
            public Test Inter { get; set; } = new TestClass();


            [MapsInheritingFrom( typeof( TestWithInitializedBaseClass ) )]
            public static IDescriptor InitialMapping()
            {
                return new MemberwiseDescriptor<TestWithInitializedBaseClass>()
                    .WithMember( "inter", o => o.Inter );
            }
        }

        [Test]
        public void TestWithInitializedInterface_DifferentSubtype()
        {
            var data = new SerializedObject()
            {
                { "inter", new SerializedObject()
                {
                    { KeyNames.TYPE, typeof( TestClass2 ).SerializeType() },
                    { "num", (SerializedPrimitive)5 }
                } }
            };

            var deserialized = SerializationUnit.Deserialize<TestWithInitializedInterface>( data );
            Assert.That( deserialized.Inter, Is.InstanceOf<TestClass2>() );
        }
        [Test]
        public void TestWithInitializedClass_DifferentSubtype()
        {
            var data = new SerializedObject()
            {
                { "inter", new SerializedObject()
                {
                    { KeyNames.TYPE, typeof( TestClass2 ).SerializeType() },
                    { "num", (SerializedPrimitive)5 }
                } }
            };

            var deserialized = SerializationUnit.Deserialize<TestWithInitializedBaseClass>( data );
            Assert.That( deserialized.Inter, Is.InstanceOf<TestClass2>() );
        }

        [Test]
        public void TestWithInitialized_NullMember()
        {
            var obj = new TestWithInitialized { Value = null };
            var data = SerializationUnit.Serialize( obj );

            var deserialized = SerializationUnit.Deserialize<TestWithInitialized>( data );
            Assert.That( deserialized.Value, Is.Null );
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

            var data = SerializationUnit.Serialize( list, new SerializationConfiguration() { CycleHandling = CycleHandling.AutoRef } );

            // Structure: Wrapped Object -> Values Array -> Element 0 is Ref to Wrapped Object
            var wrapper = (SerializedObject)data;
            var values = (SerializedArray)wrapper[KeyNames.VALUE];
            var element = (SerializedObject)values[0];

            Assert.That( element.ContainsKey( KeyNames.REF ), Is.True );

            var result = SerializationUnit.Deserialize<List<object>>( data ); // Should not throw, should resolve reference, even when the member's context is not a reference.
                                                                              // The serializer should be able to resolve references that were created when cycle handling was set to AutoRef.
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

        [Test]
        public void Populate_IgnoreMissing()
        {
            var obj = new SimpleStruct { Value = 10, Text = "Original" };

            // Data only has ID
            var data = new SerializedObject { ["Value"] = (SerializedPrimitive)20 };

            SerializationUnit.Populate( ref obj, data );

            Assert.That( obj.Value, Is.EqualTo( 20 ) );
            Assert.That( obj.Text, Is.EqualTo( "Original" ) ); // Should NOT be null
        }
    }
}