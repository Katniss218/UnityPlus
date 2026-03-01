using NUnit.Framework;
using System;
using Node = UnityPlus.Serialization.Tests.V4.SerializationV4_StructureTests.Node;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_FailureTests
    {
        public class SimpleRef
        {
            public SimpleRef Next;

            [MapsInheritingFrom( typeof( SimpleRef ) )]
            public static IDescriptor Mapping()
            {
                return new ClassOrStructDescriptor<SimpleRef>()
                    .WithMember( "next", typeof( Ctx.Ref ), o => o.Next );
            }
        }

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
            UPSUnresolvableObjectException ex = Assert.Catch<UPSUnresolvableObjectException>( () =>
            {
                SerializationUnit.Deserialize<SimpleRef>( obj );
            } );
            Assert.That( ex.Log.HasFatalErrors, Is.True, "Expected fatal errors in the log" );
        }

        [Test]
        public void Deserialize_BadTypeHeader_IgnoredOrNull()
        {
            // Object with a type that doesn't exist
            var obj = new SerializedObject();
            obj[KeyNames.TYPE] = (SerializedPrimitive)"Bad.Type.Name, Assembly";
            obj["Value"] = (SerializedPrimitive)10;

            var result = SerializationUnit.Deserialize<object>( obj );
            Assert.That( result, Is.Not.Null ); // It creates *an* object, likely just new object() or map
        }

        [Test]
        public void Deserialize_MissingMember_Ignored()
        {
            var desc = new ClassOrStructDescriptor<SerializationV4_PrimitiveTests.TestEnum>(); // Dummy descriptor

            // WARNING: This registration pollutes the global registry for SerializationV4_StructureTests.Node
            // This is why Clear() in TearDown is essential.
            TypeDescriptorRegistry.Register( new ClassOrStructDescriptor<Node>()
                .WithMember( "Name", n => n.Name ) );

            // Data has extra field "Age"
            var data = new SerializedObject();
            data["Name"] = (SerializedPrimitive)"Node";
            data["Age"] = (SerializedPrimitive)99;

            var result = SerializationUnit.Deserialize<Node>( data );

            Assert.That( result.Name, Is.EqualTo( "Node" ) );
            // No crash on "Age"
            Assert.That( result.Neighbor, Is.EqualTo( new Node().Neighbor ) ); // stays whatever it was when instantiated.
        }
    }
}