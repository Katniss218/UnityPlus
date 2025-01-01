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
    public enum AnEnum : int
    {
        First = 0,
        Second = -5,
        Third = 5
    }

    public interface IAnInterface
    {
        float interfaceMember { get; set; }
    }

    public interface IADerivedUnmappedInterface : IAnInterface
    {
        int interfaceMemberDerived { get; set; }
    }

    public class InterfaceClass : IADerivedUnmappedInterface
    {
        public float interfaceMember { get; set; }
        public int interfaceMemberDerived { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override bool Equals( object obj )
        {
            if( obj is not InterfaceClass other )
                return false;

            return this.interfaceMember == other.interfaceMember;
        }
    }

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

    public class ReferencingClass
    {
        public BaseClass refMember;
        public IAnInterface interfaceRefMember;

        public override bool Equals( object obj )
        {
            if( obj is not ReferencingClass other )
                return false;

            return this.refMember == other.refMember;
        }
    }

    public class OwningClass
    {
        public BaseClass refMember;

        public override bool Equals( object obj )
        {
            if( obj is not OwningClass other )
                return false;

            return this.refMember == other.refMember;
        }
    }

    public class UnmappedBaseClass
    {
        public float baseMember;

        public override bool Equals( object obj )
        {
            if( obj is not UnmappedBaseClass other )
                return false;

            return this.baseMember == other.baseMember;
        }
    }

    public class MappedDerivedClass : UnmappedBaseClass
    {
        public string derivedMember;

        public override bool Equals( object obj )
        {
            if( obj is not MappedDerivedClass other )
                return false;

            return this.baseMember == other.baseMember
                && this.derivedMember == other.derivedMember;
        }
    }

    public class FactoryClass
    {
        public readonly int fac1;
        public string nonFac2;

        public FactoryClass( int fac1 )
        {
            this.fac1 = fac1;
        }

        public override bool Equals( object obj )
        {
            if( obj is not FactoryClass other )
                return false;

            return this.fac1 == other.fac1
                && this.nonFac2 == other.nonFac2;
        }
    }

    public class ArrayReferenceClass
    {
        public object[] refs;

        public override bool Equals( object obj )
        {
            if( obj is not ArrayReferenceClass other )
                return false;

            return this.refs.SequenceEqual( other.refs );
        }
    }

    public class DictionaryReferenceClass
    {
        public Dictionary<object, object> refs;

        public override bool Equals( object obj )
        {
            if( obj is not DictionaryReferenceClass other )
                return false;

            return this.refs.SequenceEqual( other.refs );
        }
    }

    public class MappingRegistryTests
    {
        [MapsInheritingFrom( typeof( BaseClass ) )]
        public static SerializationMapping BaseClassMapping()
        {
            return new MemberwiseSerializationMapping<BaseClass>()
                .WithMember( "base_member", o => o.baseMember );
        }

        [MapsInheritingFrom( typeof( DerivedClass ) )]
        public static SerializationMapping DerivedClassMapping()
        {
            return new MemberwiseSerializationMapping<DerivedClass>()
                .WithMember( "derived_member", o => o.derivedMember );
        }

        [MapsInheritingFrom( typeof( GenericClass<> ) )]
        public static SerializationMapping TestclassMapping<T>()
        {
            return new MemberwiseSerializationMapping<GenericClass<T>>()
                .WithMember( "member", o => o.member );
        }

        [MapsInheritingFrom( typeof( MultiGenericClass<,,> ) )]
        public static SerializationMapping TestclassMapping<T1, T2, T3>()
        {
            return new MemberwiseSerializationMapping<MultiGenericClass<T1, T2, T3>>()
                .WithMember( "member1", o => o.member1 )
                .WithMember( "member2", o => o.member2 )
                .WithMember( "member3", o => o.member3 );
        }

        [MapsInheritingFrom( typeof( OwningClass ) )]
        public static SerializationMapping OwningMapping()
        {
            return new MemberwiseSerializationMapping<OwningClass>()
                .WithMember( "ref_member", o => o.refMember );
        }

        [MapsInheritingFrom( typeof( InterfaceClass ) )]
        public static SerializationMapping InterfaceClassMapping()
        {
            return new MemberwiseSerializationMapping<InterfaceClass>()
                .WithMember( "interface_member", o => o.interfaceMember );
        }

        [MapsImplementing( typeof( IAnInterface ) )]
        public static SerializationMapping IAnInterfaceMapping()
        {
            return new MemberwiseSerializationMapping<IAnInterface>()
                .WithMember( "interface_member", o => o.interfaceMember );
        }

        [MapsInheritingFrom( typeof( ReferencingClass ) )]
        public static SerializationMapping ReferencingClassMapping()
        {
            return new MemberwiseSerializationMapping<ReferencingClass>()
                .WithMember( "ref_member", ObjectContext.Ref, o => o.refMember )
                .WithMember( "interface_ref_member", ObjectContext.Ref, o => o.interfaceRefMember );
        }

        [MapsInheritingFrom( typeof( MappedDerivedClass ) )]
        public static SerializationMapping MappedDerivedClassMapping()
        {
            return new MemberwiseSerializationMapping<MappedDerivedClass>()
                .WithMember( "derived_member", o => o.derivedMember );
        }

        [MapsInheritingFrom( typeof( FactoryClass ) )]
        public static SerializationMapping FactoryClassMapping()
        {
            return new MemberwiseSerializationMapping<FactoryClass>()
                .WithMember( "fac1", o => o.fac1, ( o, value ) => { } )
                .WithFactory<int>( fac1 => new FactoryClass( fac1 ) )
                .WithMember( "non_fac2", o => o.nonFac2 );
        }

        [MapsInheritingFrom( typeof( ArrayReferenceClass ) )]
        public static SerializationMapping ArrayReferenceClassMapping()
        {
            return new MemberwiseSerializationMapping<ArrayReferenceClass>()
                .WithMember( "refs", ArrayContext.Refs, o => o.refs );
        }
        
        [MapsInheritingFrom( typeof( DictionaryReferenceClass ) )]
        public static SerializationMapping DictionaryReferenceClassMapping()
        {
            return new MemberwiseSerializationMapping<DictionaryReferenceClass>()
                .WithMember( "refs", KeyValueContext.RefToRef, o => o.refs );
        }

        //
        //  These tests test both the mapping registry search algorithm, and the construction of the mappings that's inside it.
        //

        [Test]
        public void GetMappingFor___Simple___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMapping<bool>( ObjectContext.Default, (bool)true );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMapping<bool>( ObjectContext.Default, true.GetType() );

            // Assert
            Assert.That( mapping2, Is.InstanceOf( typeof( PrimitiveSerializationMapping<bool> ) ) );
            Assert.That( mapping2, Is.InstanceOf( typeof( PrimitiveSerializationMapping<bool> ) ) );
        }

        [Test]
        public void GetMappingFor___Derived___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMapping<BaseClass>( ObjectContext.Default, new DerivedClass() );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMapping<BaseClass>( ObjectContext.Default, new DerivedClass().GetType() );

            // Assert
            Assert.That( mapping2, Is.InstanceOf( typeof( MemberwiseSerializationMapping<DerivedClass> ) ) );
            Assert.That( mapping2, Is.InstanceOf( typeof( MemberwiseSerializationMapping<DerivedClass> ) ) );
        }

        [Test]
        public void GetMappingFor___Generic___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMapping<GenericClass<float>>( ObjectContext.Default, new GenericClass<float>() );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMapping<GenericClass<float>>( ObjectContext.Default, new GenericClass<float>().GetType() );

            // Assert
            Assert.That( mapping2, Is.InstanceOf( typeof( MemberwiseSerializationMapping<GenericClass<float>> ) ) );
            Assert.That( mapping2, Is.InstanceOf( typeof( MemberwiseSerializationMapping<GenericClass<float>> ) ) );
        }

        [Test]
        public void GetMappingFor___MultipleGeneric___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMapping<MultiGenericClass<float, int, float>>( ObjectContext.Default, new MultiGenericClass<float, int, float>() );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMapping<MultiGenericClass<float, int, float>>( ObjectContext.Default, new MultiGenericClass<float, int, float>().GetType() );

            // Assert
            Assert.That( mapping2, Is.InstanceOf( typeof( MemberwiseSerializationMapping<MultiGenericClass<float, int, float>> ) ) );
            Assert.That( mapping2, Is.InstanceOf( typeof( MemberwiseSerializationMapping<MultiGenericClass<float, int, float>> ) ) );
        }

        [Test]
        public void GetMappingFor___Array___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMapping<int[]>( ObjectContext.Default, new int[] { 1 } );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMapping<int[]>( ObjectContext.Default, new int[] { 1 }.GetType() );

            // Assert
            Assert.That( mapping2, Is.InstanceOf( typeof( IndexedSerializationMapping<int[], int> ) ) );
            Assert.That( mapping2, Is.InstanceOf( typeof( IndexedSerializationMapping<int[], int> ) ) );
        }

        [Test]
        public void GetMappingFor___ImplementedInterface___ReturnsCorrectMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMapping<IAnInterface>( ObjectContext.Default, (IAnInterface)null );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMapping<IAnInterface>( ObjectContext.Default, typeof( IAnInterface ) );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( MemberwiseSerializationMapping<IAnInterface> ) ) );
            Assert.That( mapping2, Is.InstanceOf( typeof( MemberwiseSerializationMapping<IAnInterface> ) ) );
        }

        [Test]
        public void GetMappingFor___UnmappedBaseClassPolymorphic___ReturnsDerivedMapping()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMapping<UnmappedBaseClass>( ObjectContext.Default, typeof( MappedDerivedClass ) );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMapping<UnmappedBaseClass>( ObjectContext.Default, new MappedDerivedClass() );

            // Assert
            Assert.That( mapping1, Is.InstanceOf( typeof( MemberwiseSerializationMapping<MappedDerivedClass> ) ) );
            Assert.That( mapping2, Is.InstanceOf( typeof( MemberwiseSerializationMapping<MappedDerivedClass> ) ) );
        }

        [Test]
        public void GetMappingFor___Nonexistant___ReturnsNull()
        {
            // Arrange

            // Act
            SerializationMapping mapping1 = SerializationMappingRegistry.GetMapping<BaseClass>( 537865853, (BaseClass)null );
            SerializationMapping mapping2 = SerializationMappingRegistry.GetMapping<BaseClass>( 537865853, typeof( BaseClass ) );

            // Assert
            Assert.That( mapping1, Is.Null );
            Assert.That( mapping2, Is.Null );
        }
    }
}