
using NUnit.Framework;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_PrimitiveTests
    {
        public enum TestEnum { First, Second, Third }

        [TearDown]
        public void Cleanup()
        {
            TypeDescriptorRegistry.Clear();
        }

        [Test]
        public void Serialize_Int_RoundTrip()
        {
            int val = -123;
            var data = SerializationUnit.Serialize( val );
            var result = SerializationUnit.Deserialize<int>( data );
            Assert.That( result, Is.EqualTo( val ) );
        }

        [Test]
        public void Serialize_String_RoundTrip()
        {
            string val = "Hello World \n \"escaped\"";
            var data = SerializationUnit.Serialize( val );
            var result = SerializationUnit.Deserialize<string>( data );
            Assert.That( result, Is.EqualTo( val ) );
        }

        [Test]
        public void Serialize_Enum_Default_AsInt()
        {
            // Default EnumDescriptor uses Integer mode
            var val = TestEnum.Second;
            var data = SerializationUnit.Serialize( val );

            Assert.That( data, Is.InstanceOf<SerializedPrimitive>() );
           // Assert.That( ((SerializedPrimitive)data)._type, Is.EqualTo( SerializedPrimitive.DataType.Int64 ) );

            var result = SerializationUnit.Deserialize<TestEnum>( data );
            Assert.That( result, Is.EqualTo( val ) );
        }

        [Test]
        public void Serialize_Enum_AsString_WithCustomDescriptor()
        {
            // Register explicit descriptor for this test context
            TypeDescriptorRegistry.Register( new EnumDescriptor<TestEnum>( EnumSerializationMode.String ) );

            var val = TestEnum.Third;
            var data = SerializationUnit.Serialize( val );

            Assert.That( data, Is.InstanceOf<SerializedPrimitive>() );
           // Assert.That( ((SerializedPrimitive)data)._type, Is.EqualTo( SerializedPrimitive.DataType.String ) );
            Assert.That( (string)(SerializedPrimitive)data, Is.EqualTo( "Third" ) );

            var result = SerializationUnit.Deserialize<TestEnum>( data );
            Assert.That( result, Is.EqualTo( val ) );
        }

        [Test]
        public void Deserialize_TypeMismatch_Coercion()
        {
            // Serialize as float
            var data = SerializationUnit.Serialize( 123.45f );

            // Deserialize as int (PrimitiveDescriptor should try to coerce or cast)
            // Note: Standard JSON behavior usually allows this. 
            // The v4 IntDescriptor currently casts directly. 
            // If the underlying data is Float64, a direct cast (int)object might fail if unboxing.
            // Let's verify standard behavior:

            // Manually construct 'bad' data: float stored, want int.
            SerializedData badData = (SerializedPrimitive)123.9f;

            // The IntDescriptor uses (int)data cast operator which handles SerializedPrimitive conversions internally.
            var result = SerializationUnit.Deserialize<int>( badData );

            Assert.That( result, Is.EqualTo( 123 ) ); // Truncation expected
        }
    }
}
