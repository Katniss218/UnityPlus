using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Json;

namespace Neoserialization
{
    public class MappingConstructionTests
    {
        public class BaseClass
        {
            public float baseMember;
        }

        public class DerivedClass : BaseClass
        {
            public string derivedMember;
        }

        public class Testclass<T>
        {
            public T member;
        }

        [SerializationMappingProvider( typeof( Testclass<> ) )]
        public static SerializationMapping TestclassMapping<T>()
        {
            return new CompoundMapping<Testclass<T>>()
            {
                ("member", new Member<Testclass<T>, T>( o => o.member ))
            };
        }
        
        [SerializationMappingProvider( typeof( BaseClass ) )]
        public static SerializationMapping BaseClassMapping()
        {
            return new CompoundMapping<BaseClass>()
            {
                ("base_member", new Member<BaseClass, float>( o => o.baseMember ))
            };
        }
        
        [SerializationMappingProvider( typeof( DerivedClass ) )]
        public static SerializationMapping DerivedClassMapping()
        {
            return new CompoundMapping<DerivedClass>()
            {
                ("derived_member", new Member<DerivedClass, string>( o => o.derivedMember ))
            };
        }

        [Test]
        public void GetMappingFor___Simple___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMapping.GetMappingFor<bool>( (bool)true );
            SerializationMapping mapping2 = SerializationMapping.GetMappingFor<bool>( true.GetType() );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( DirectMapping<bool> ) ) );
            Assert.That( mapping1, Is.EqualTo( mapping2 ) );
        }

        [Test]
        public void GetMappingFor___Generic___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMapping.GetMappingFor<Testclass<float>>( new Testclass<float>() );
            SerializationMapping mapping2 = SerializationMapping.GetMappingFor<Testclass<float>>( new Testclass<float>().GetType() );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( CompoundMapping<Testclass<float>> ) ) );
            Assert.That( mapping1, Is.EqualTo( mapping2 ) );
        }
        
        [Test]
        public void GetMappingFor___GenericArray___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMapping.GetMappingFor<int[]>( new int[] { 1 } );
            SerializationMapping mapping2 = SerializationMapping.GetMappingFor<int[]>( new int[] { 1 }.GetType() );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( DirectMapping<int[]> ) ) );
            Assert.That( mapping1, Is.EqualTo( mapping2 ) );
        }

        [Test]
        public void GetMappingFor___Derived___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMapping.GetMappingFor<BaseClass>( new DerivedClass() );
            SerializationMapping mapping2 = SerializationMapping.GetMappingFor<BaseClass>( new DerivedClass().GetType() );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( CompoundMapping<DerivedClass> ) ) );
            Assert.That( mapping1, Is.EqualTo( mapping2 ) );
        }
    }
}