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
    public class BaseClass
    {
        public float baseMember;

        public override bool Equals( object obj )
        {
            if( obj is not BaseClass other )
                return false;

            return this.baseMember == other.baseMember;
        }
    }

    public class DerivedClass : BaseClass
    {
        public string derivedMember;

        public override bool Equals( object obj )
        {
            if( obj is not DerivedClass other )
                return false;

            return this.baseMember == other.baseMember
                && this.derivedMember == other.derivedMember;
        }
    }

    public class GenericClass<T>
    {
        public T member;

        public override bool Equals( object obj )
        {
            if( obj is not GenericClass<T> other )
                return false;

            return this.member.Equals( other.member );
        }
    }

    public class MultiGenericClass<T1, T2, T3>
    {
        public T1 member1;
        public T2 member2;
        public T3 member3;

        public override bool Equals( object obj )
        {
            if( obj is not MultiGenericClass<T1, T2, T3> other )
                return false;

            return this.member1.Equals( other.member1 )
                && this.member2.Equals( other.member2 )
                && this.member3.Equals( other.member3 );
        }
    }

    public class MappingRegistryTests
    {
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

        //
        //  These tests test both the mapping registry search algorithm, and the construction of the mappings that's inside it.
        //

        [Test]
        public void GetMappingFor___Simple___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMappingOrDefault<bool>( ObjectContext.Default, (bool)true );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMappingOrDefault<bool>( ObjectContext.Default, true.GetType() );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( PrimitiveObjectSerializationMapping<bool> ) ) );
            Assert.That( mapping1, Is.EqualTo( mapping2 ) );
        }

        [Test]
        public void GetMappingFor___Derived___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMappingOrDefault<BaseClass>( ObjectContext.Default, new DerivedClass() );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMappingOrDefault<BaseClass>( ObjectContext.Default, new DerivedClass().GetType() );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( MemberwiseSerializationMapping<DerivedClass> ) ) );
            Assert.That( mapping1, Is.EqualTo( mapping2 ) );
        }

        [Test]
        public void GetMappingFor___Generic___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMappingOrDefault<GenericClass<float>>( ObjectContext.Default, new GenericClass<float>() );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMappingOrDefault<GenericClass<float>>( ObjectContext.Default, new GenericClass<float>().GetType() );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( MemberwiseSerializationMapping<GenericClass<float>> ) ) );
            Assert.That( mapping1, Is.EqualTo( mapping2 ) );
        }

        [Test]
        public void GetMappingFor___MultipleGeneric___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMappingOrDefault<MultiGenericClass<float, int, float>>( ObjectContext.Default, new MultiGenericClass<float, int, float>() );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMappingOrDefault<MultiGenericClass<float, int, float>>( ObjectContext.Default, new MultiGenericClass<float, int, float>().GetType() );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( MemberwiseSerializationMapping<MultiGenericClass<float, int, float>> ) ) );
            Assert.That( mapping1, Is.EqualTo( mapping2 ) );
        }

        [Test]
        public void GetMappingFor___Array___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMappingOrDefault<int[]>( ObjectContext.Default, new int[] { 1 } );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMappingOrDefault<int[]>( ObjectContext.Default, new int[] { 1 }.GetType() );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( NonPrimitiveSerializationMapping<int[]> ) ) );
            Assert.That( mapping1, Is.EqualTo( mapping2 ) );
        }
    }
}