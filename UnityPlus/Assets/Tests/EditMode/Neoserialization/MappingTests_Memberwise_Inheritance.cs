using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace Neoserialization
{
    public class MappingTests_Memberwise_Inheritance
    {
        public interface IMarkerInterface { }

        public class BaseClass : IMarkerInterface
        {
            public int m1;

            public override bool Equals( object obj )
            {
                if( obj is not BaseClass other )
                    return false;

                return this.m1 == other.m1;
            }

            [MapsInheritingFrom( typeof( BaseClass ) )]
            public static SerializationMapping Mapping()
            {
                return new MemberwiseSerializationMapping<BaseClass>()
                    .WithMember( "m1", o => o.m1 );
            }
        }

        public class DerivedClass : BaseClass
        {
            public string m2;

            public override bool Equals( object obj )
            {
                if( obj is not DerivedClass other )
                    return false;

                return this.m1 == other.m1
                    && this.m2 == other.m2;
            }

            [MapsInheritingFrom( typeof( DerivedClass ) )]
            public static SerializationMapping Mapping()
            {
                return new MemberwiseSerializationMapping<DerivedClass>()
                    .WithMember( "m2", o => o.m2 );
            }
        }

        public class BaseImmutableClass
        {
            public readonly int m1;

            public BaseImmutableClass( int m1 )
            {
                this.m1 = m1;
            }

            public override bool Equals( object obj )
            {
                if( obj is not BaseImmutableClass other )
                    return false;

                return this.m1 == other.m1;
            }

            [MapsInheritingFrom( typeof( BaseImmutableClass ) )]
            public static SerializationMapping Mapping()
            {
                return new MemberwiseSerializationMapping<BaseImmutableClass>()
                    .WithReadonlyMember( "m1", o => o.m1 )
                    .WithFactory<int>( m1 => new BaseImmutableClass( m1 ) );
            }
        }

        public class DerivedPartiallyImmutableClass : BaseImmutableClass
        {
            public string m2;

            public DerivedPartiallyImmutableClass( int m1 ) : base( m1 ) { }

            public override bool Equals( object obj )
            {
                if( obj is not DerivedPartiallyImmutableClass other )
                    return false;

                return this.m1 == other.m1
                    && this.m2 == other.m2;
            }

            [MapsInheritingFrom( typeof( DerivedPartiallyImmutableClass ) )]
            public static SerializationMapping Mapping()
            {
                return new MemberwiseSerializationMapping<DerivedPartiallyImmutableClass>()
                    .WithFactory<int>( m1 => new DerivedPartiallyImmutableClass( m1 ) )
                    .WithMember( "m2", o => o.m2 );
            }
        }

        public class DerivedFullyImmutableClass : BaseImmutableClass
        {
            public readonly string m2;

            public DerivedFullyImmutableClass( int m1, string m2 ) : base( m1 )
            {
                this.m2 = m2;
            }

            public override bool Equals( object obj )
            {
                if( obj is not DerivedFullyImmutableClass other )
                    return false;

                return this.m1 == other.m1
                    && this.m2 == other.m2;
            }

            [MapsInheritingFrom( typeof( DerivedFullyImmutableClass ) )]
            public static SerializationMapping Mapping()
            {
                return new MemberwiseSerializationMapping<DerivedFullyImmutableClass>()
                    .WithReadonlyMember( "m2", o => o.m2 )
                    .WithFactory<int, string>( (m1, m2) => new DerivedFullyImmutableClass( m1, m2 ) );
            }
        }






        [Test]
        public void Mapping___BaseClass_Null___RoundTrip()
        {
            // Arrange
            var initialValue = (BaseClass)null;

            // Act
            var data = SerializationUnit.Serialize<BaseClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }
        
        [Test]
        public void Mapping___BaseClass___RoundTrip()
        {
            // Arrange
            var initialValue = new BaseClass() { m1 = 5 };

            // Act
            var data = SerializationUnit.Serialize<BaseClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___DerivedClass_Null___RoundTrip()
        {
            // Arrange
            var initialValue = (DerivedClass)null;

            // Act
            var data = SerializationUnit.Serialize<DerivedClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<DerivedClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___DerivedClass___RoundTrip()
        {
            // Arrange
            var initialValue = new DerivedClass() { m1 = 5, m2 = "42" };

            // Act
            var data = SerializationUnit.Serialize<DerivedClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<DerivedClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___DerivedClass_Null_Polymorphic___RoundTrip()
        {
            // Arrange
            var initialValue = (DerivedClass)null;

            // Act
            var data = SerializationUnit.Serialize<BaseClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___DerivedClass_Polymorphic___RoundTrip()
        {
            // Arrange
            BaseClass initialValue = new DerivedClass() { m1 = 5, m2 = "42" };

            // Act
            var data = SerializationUnit.Serialize<BaseClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }
        
        [Test]
        public void Mapping___BaseClass_Null_Polymorphic_FromMarkerInterface___RoundTrip()
        {
            // Arrange
            BaseClass initialValue = null;

            // Act
            var data = SerializationUnit.Serialize<IMarkerInterface>( initialValue );
            var finalValue = SerializationUnit.Deserialize<IMarkerInterface>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }
        
        [Test]
        public void Mapping___BaseClass_Polymorphic_FromMarkerInterface___RoundTrip()
        {
            // Arrange
            IMarkerInterface initialValue = new BaseClass() { m1 = 5 };

            // Act
            var data = SerializationUnit.Serialize<IMarkerInterface>( initialValue );
            var finalValue = SerializationUnit.Deserialize<IMarkerInterface>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }
        
        [Test]
        public void Mapping___DerivedClass_Polymorphic_FromMarkerInterface___RoundTrip()
        {
            // Arrange
            IMarkerInterface initialValue = new DerivedClass() { m1 = 5, m2 = "42" };

            // Act
            var data = SerializationUnit.Serialize<IMarkerInterface>( initialValue );
            var finalValue = SerializationUnit.Deserialize<IMarkerInterface>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }


        [Test]
        public void Mapping___BaseImmutableClass_Null___RoundTrip()
        {
            // Arrange
            var initialValue = (BaseImmutableClass)null;

            // Act
            var data = SerializationUnit.Serialize<BaseImmutableClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseImmutableClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }
        
        [Test]
        public void Mapping___BaseImmutableClass___RoundTrip()
        {
            // Arrange
            var initialValue = new BaseImmutableClass( 5 );

            // Act
            var data = SerializationUnit.Serialize<BaseImmutableClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseImmutableClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___DerivedPartiallyImmutableClass___RoundTrip()
        {
            // Arrange
            var initialValue = new DerivedPartiallyImmutableClass( 5 ) { m2 = "42" };

            // Act
            var data = SerializationUnit.Serialize<DerivedPartiallyImmutableClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<DerivedPartiallyImmutableClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___DerivedPartiallyImmutableClass_Polymorphic___RoundTrip()
        {
            // Arrange
            BaseImmutableClass initialValue = new DerivedPartiallyImmutableClass( 5 ) { m2 = "42" };

            // Act
            var data = SerializationUnit.Serialize<BaseImmutableClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseImmutableClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___DerivedFullyImmutableClass___RoundTrip()
        {
            // Arrange
            var initialValue = new DerivedFullyImmutableClass( 5, "42" );

            // Act
            var data = SerializationUnit.Serialize<DerivedFullyImmutableClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<DerivedFullyImmutableClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }

        [Test]
        public void Mapping___DerivedFullyImmutableClass_Polymorphic___RoundTrip()
        {
            // Arrange
            BaseImmutableClass initialValue = new DerivedFullyImmutableClass( 5, "42" );

            // Act
            var data = SerializationUnit.Serialize<BaseImmutableClass>( initialValue );
            var finalValue = SerializationUnit.Deserialize<BaseImmutableClass>( data );

            // Assert
            Assert.That( finalValue, Is.EqualTo( initialValue ) );
        }
    }
}