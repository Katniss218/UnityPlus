
using NUnit.Framework;
using System;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_FailureTests
    {
        public class SimpleRef { public SimpleRef Next; }

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
            obj["Next"] = new SerializedObject { { KeyNames.REF, (SerializedPrimitive)Guid.NewGuid().ToString( "D" ) } };

            // We expect a specific exception type defined in v4
            Assert.Throws<UPSMissingReferenceException>( () =>
            {
                SerializationUnit.Deserialize<SimpleRef>( obj );
            } );
        }

        [Test]
        public void Deserialize_BadTypeHeader_IgnoredOrNull()
        {
            // Object with a type that doesn't exist
            var obj = new SerializedObject();
            obj[KeyNames.TYPE] = (SerializedPrimitive)"Bad.Type.Name, Assembly";
            obj["Value"] = (SerializedPrimitive)10;

            // Should fall back to the requested type (object) or return null/default if totally incompatible?
            // DeserializerStrategy logic: If ResolveType returns null, it usually keeps the default descriptor or fails.

            // In v4 DeserializerStrategy.PhasePreProcessing:
            // Type actualType = resolver.ResolveType(...);
            // if (actualType != null) cursor.Descriptor = GetDescriptor(actualType);

            // So if type is bad, it ignores it and uses the descriptor for <object>.
            // Descriptor for <object> is empty/minimal.

            var result = SerializationUnit.Deserialize<object>( obj );
            Assert.That( result, Is.Not.Null ); // It creates *an* object, likely just new object() or map
        }

        [Test]
        public void Deserialize_MissingMember_Ignored()
        {
            var desc = new ClassOrStructDescriptor<SerializationV4_PrimitiveTests.TestEnum>(); // Dummy descriptor

            // WARNING: This registration pollutes the global registry for SerializationV4_StructureTests.Node
            // This is why Clear() in TearDown is essential.
            TypeDescriptorRegistry.Register( new ClassOrStructDescriptor<SerializationV4_StructureTests.Node>()
                .WithMember( "Name", n => n.Name ) );

            // Data has extra field "Age"
            var data = new SerializedObject();
            data["Name"] = (SerializedPrimitive)"Node";
            data["Age"] = (SerializedPrimitive)99;

            var result = SerializationUnit.Deserialize<SerializationV4_StructureTests.Node>( data );

            Assert.That( result.Name, Is.EqualTo( "Node" ) );
            // No crash on "Age"
        }
    }
}
