using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace Neoserialization
{
    public class MappingTests
    {
        public class BaseClass
        {
            public float baseMember;
        }

        public class DerivedClass : BaseClass
        {
            public string derivedMember;
        }

        public class GenericClass<T>
        {
            public T member;
        }

        public class MultiGenericClass<T1, T2, T3>
        {
            public T1 member1;
            public T2 member2;
            public T3 member3;
        }

        [SerializationMappingProvider( typeof( BaseClass ) )]
        public static SerializationMapping BaseClassMapping()
        {
            return new MemberwiseSerializationMapping<BaseClass>()
            {
                ("base_member", new Member<BaseClass, float>( o => o.baseMember ))
            };
        }

        [SerializationMappingProvider( typeof( DerivedClass ) )]
        public static SerializationMapping DerivedClassMapping()
        {
            return new MemberwiseSerializationMapping<DerivedClass>()
            {
                ("derived_member", new Member<DerivedClass, string>( o => o.derivedMember ))
            };
        }

        [SerializationMappingProvider( typeof( GenericClass<> ) )]
        public static SerializationMapping TestclassMapping<T>()
        {
            return new MemberwiseSerializationMapping<GenericClass<T>>()
            {
                ("member", new Member<GenericClass<T>, T>( o => o.member ))
            };
        }

        [SerializationMappingProvider( typeof( MultiGenericClass<,,> ) )]
        public static SerializationMapping TestclassMapping<T1, T2, T3>()
        {
            return new MemberwiseSerializationMapping<MultiGenericClass<T1, T2, T3>>()
            {
                ("member1", new Member<MultiGenericClass<T1, T2, T3>, T1>( o => o.member1 )),
                ("member2", new Member<MultiGenericClass<T1, T2, T3>, T2>( o => o.member2 )),
                ("member3", new Member<MultiGenericClass<T1, T2, T3>, T3>( o => o.member3 ))
            };
        }

        [Test]
        public void Mapping___Bool___SavesCorrectly()
        {
            // Arrange
            // Act
            var data = SerializationUnit.Serialize( true );

            // Assert
            Assert.That( data, Is.EqualTo( (SerializedPrimitive)true ) );
        }
        [Test]
        public void Mapping___Bool___LoadsCorrectly()
        {
            // Arrange
            // Act
            var value = SerializationUnit.Deserialize<bool>( (SerializedPrimitive)true );

            // Assert
            Assert.That( value, Is.EqualTo( true ) );
        }
    }
}