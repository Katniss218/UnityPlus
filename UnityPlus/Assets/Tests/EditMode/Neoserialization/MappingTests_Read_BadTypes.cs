using NUnit.Framework;
using System;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;

namespace Neoserialization
{
    public class MemberClass
    {
        public object objMember1;
        public object objMember2;

        public override bool Equals( object obj )
        {
            if( obj is not MemberClass other )
                return false;

            return this.objMember1 == other.objMember1
                && this.objMember2 == other.objMember2;
        }

        [MapsInheritingFrom( typeof( MemberClass ) )]
        public static SerializationMapping GetMapping()
        {
            return new MemberwiseSerializationMapping<MemberClass>()
                .WithMember( "obj_member1", o => o.objMember1 )
                .WithMember( "obj_member2", o => o.objMember2 );
        }
    }

    public class MappingTests_Read_BadTypes
    {
        [Test]
        public void Reading___BadType___DeserializedAsDefault()
        {
            // Arrange
            var initialValue = new SerializedObject()
            {
                { "$type", "UnityEngine.GameObjectNonExistent, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                { "$id", "1cfbb788-5602-4b03-bc62-39e1ecb0e6b6" },
                { "value", (SerializedPrimitive)true }
            };
            var refMap = new BidirectionalReferenceStore();

            // Act
            var data = SerializationUnit.Deserialize<object>( ObjectContext.Value, initialValue, refMap );

            // Assert
            Assert.That( data, Is.EqualTo( default( object ) ) );
        }

        [Test]
        public void Reading___BadType_Member___LeftAsDefault()
        {
            // Arrange
            var initialValue = new SerializedObject()
            {
                { "$type", "Neoserialization.MemberClass, EditMode, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                { "$id", "1cfbb788-5602-4b03-bc62-39e1ecb0e6b6" },
                { "obj_member1", new SerializedObject()
                {
                    { "$type", "UnityEngine.GameObjectNonExistent, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                    { "$id", "1cfbb788-5602-4b03-bc62-000000000000" },
                    { "value", (SerializedPrimitive)true }
                } },
                { "obj_member2", new SerializedObject() // good member as control case
                {
                    { "$type", "System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null" },
                    { "$id", "1cfbb788-5602-4b03-bc62-39e1ecb0e6b6" },
                    { "value", (SerializedPrimitive)true }
                } }
            };
            var refMap = new BidirectionalReferenceStore();

            // Act
            var data = SerializationUnit.Deserialize<MemberClass>( ObjectContext.Value, initialValue, refMap );

            // Assert
            Assert.That( refMap.GetID( data ), Is.EqualTo( Guid.Parse( "1cfbb788-5602-4b03-bc62-39e1ecb0e6b6" ) ) );
            Assert.That( data.objMember1, Is.EqualTo( default( BaseClass ) ) );
            Assert.That( data.objMember2, Is.EqualTo( true ) );
        }

        [Test]
        public void Reading___BadType_Array___LeftAsDefault()
        {
            // Arrange
            var initialValue = new SerializedObject()
            {
                { "$type", "System.Object[], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null" },
                { "$id", "1cfbb788-5602-4b03-bc62-000000000000" },
                { "value", new SerializedArray()
                {
                    new SerializedObject() // good element as control case
                    {
                        { "$type", "System.Boolean, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=null" },
                        { "$id", "1cfbb788-5602-4b03-bc62-39e1ecb0e6b9" },
                        { "value", (SerializedPrimitive)true }
                    },
                    new SerializedObject()
                    {
                        { "$type", "UnityEngine.GameObjectNonExistent, UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null" },
                        { "$id", "1cfbb788-5602-4b03-bc62-39e1ecb0e6b6" },
                        { "value", (SerializedPrimitive)true }
                    }
                } }
            };
            var refMap = new BidirectionalReferenceStore();

            // Act
            var data = SerializationUnit.Deserialize<object[]>( ArrayContext.Values, initialValue, refMap );

            // Assert
            Assert.That( refMap.GetID( data ), Is.EqualTo( Guid.Parse( "1cfbb788-5602-4b03-bc62-000000000000" ) ) );
            Assert.That( data[0], Is.EqualTo( true ) );
            Assert.That( data[1], Is.EqualTo( default( object ) ) );
        }
    }
}