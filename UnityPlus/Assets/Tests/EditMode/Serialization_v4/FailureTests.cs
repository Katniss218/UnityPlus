using NUnit.Framework;
using System;
using UnityPlus.Serialization;
using Ctx = UnityPlus.Serialization.Ctx;

namespace Neoserialization.V4
{
    public class FailureTests
    {
        [TearDown]
        public void Cleanup()
        {
            // Reset the registry to remove any manually registered descriptors (like the partial Node descriptor)
            TypeDescriptorRegistry.Clear();
        }

        [Test]
        public void Deserialize_MissingReference_ThrowsSpecificException()
        {
            // Construct data with a broken reference
            var obj = new SerializedObject();
            obj["next"] = new SerializedObject()
            {
                { KeyNames.REF, (SerializedPrimitive)Guid.NewGuid().ToString( "D" ) }
            };

            // We expect a specific exception type defined in v4
            Assert.Catch<UPSMemberResolutionException>( () =>
            {
                SerializationUnit.Deserialize<SimpleRef>( obj );
            } );
        }

        [Test]
        public void Deserialize_WrongDataType_ThrowsException()
        {
            // Expecting an int, but getting a SerializedObject
            var data = new SerializedObject { { "value", (SerializedPrimitive)10 } };

            Assert.Catch<Exception>( () =>
            {
                SerializationUnit.Deserialize<int>( data );
            } );
        }

        [Test]
        public void Deserialize_MalformedArray_ThrowsException()
        {
            // Expecting an array, but getting a primitive
            var data = (SerializedPrimitive)42;

            Assert.Catch<Exception>( () =>
            {
                SerializationUnit.Deserialize<int[]>( data );
            } );
        }

        [Test]
        public void Deserialize_InvalidEnumValue_ThrowsAndDefaults()
        {
            // Enum value that doesn't exist
            var data = (SerializedPrimitive)999;

            Assert.Catch<Exception>( () =>
            {
                SerializationUnit.Deserialize<TestEnumStr>( data ); // string throws
            } );

            var result = SerializationUnit.Deserialize<TestEnumInt>( data ); // int accpets, but value is out of defined range
            Assert.That( (int)result, Is.EqualTo( 999 ) );
        }

        [Test]
        public void Deserialize_CircularReference_WhenNotAllowed_Throws()
        {
            // Construct a circular reference in data
            var obj1 = new SerializedObject();
            obj1[KeyNames.ID] = (SerializedPrimitive)"1";
            obj1["next"] = new SerializedObject { { KeyNames.REF, (SerializedPrimitive)"1" } };

            var config = new SerializationConfiguration { CycleHandling = CycleHandling.Throw };

            Assert.Catch<Exception>( () =>
            {
                SerializationUnit.Deserialize<SimpleRef>( obj1, default, config );
            } );
        }

        [Test]
        public void Deserialize_StrictWrapper_MissingMetadata_Throws()
        {
            // WrapperHandling.Strict requires $id or $type if a "value" wrapper is used.
            var data = new SerializedObject { { KeyNames.VALUE, (SerializedPrimitive)10 } };
            var config = new SerializationConfiguration { WrapperHandling = WrapperHandling.Strict };

            Assert.Catch<Exception>( () =>
            {
                SerializationUnit.Deserialize<int>( data, default, config );
            } );
        }

        [Test]
        public void Deserialize_MultiDimensionalArray_WrongLengths_Throws()
        {
            // Expecting 2x2, but providing 3 lengths
            var data = new SerializedObject {
                { "lengths", new SerializedArray { (SerializedPrimitive)2, (SerializedPrimitive)2, (SerializedPrimitive)2 } },
                { "values", new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2, (SerializedPrimitive)3, (SerializedPrimitive)4 } }
            };

            Assert.Catch<Exception>( () =>
            {
                SerializationUnit.Deserialize<int[,]>( data );
            } );
        }

        [Test]
        public void Deserialize_MissingAsset_Throws()
        {
            var data = new SerializedObject() 
            {
                { KeyNames.ASSETREF, (SerializedPrimitive)"non-existent" } 
            };

            Assert.Catch<UPSMemberResolutionException>( () =>
            {
                SerializationUnit.Deserialize<MockAsset>( typeof( Ctx.Asset ), data );
            } );
        }

        [Test]
        public void Deserialize_BadTypeHeader_IgnoredOrNull()
        {
            // Object with a type that doesn't exist
            var data = new SerializedObject()
            {
                { KeyNames.TYPE, (SerializedPrimitive)"Bad.Type.Name, Assembly" },
                { "val", (SerializedPrimitive)10 }
            };

            Assert.Catch<UPSTypeResolutionException>( () =>
            {
                SerializationUnit.Deserialize<object>( data );
            } );
        }

        [Test]
        public void Deserialize_MissingMember_Ignored()
        {
            // Data has extra field "Age"
            var data = new SerializedObject();
            data["name"] = (SerializedPrimitive)"Node";
            data["age"] = (SerializedPrimitive)99;

            var result = SerializationUnit.Deserialize<Node>( data );

            Assert.That( result.Name, Is.EqualTo( "Node" ) );
            // No crash on "Age"
            Assert.That( result.Next, Is.EqualTo( new Node().Next ) ); // stays whatever it was when instantiated.
        }
    }
}