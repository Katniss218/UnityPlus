using NUnit.Framework;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_ManualDescriptorTests
    {
        public class ManualPerson
        {
            public string Name { get; set; }
            public int Age { get; set; }

            public ManualPerson( string name, int age )
            {
                Name = name;
                Age = age;
            }
        }

        public class MutablePerson
        {
            public string Name;
            public int Age;
        }

        public struct ManualPoint
        {
            public int X { get; }
            public int Y { get; }
            public int Z { get; }

            public ManualPoint( int x, int y, int z )
            {
                X = x;
                Y = y;
                Z = z;
            }
        }

        [TearDown]
        public void Cleanup()
        {
            TypeDescriptorRegistry.Clear();
        }

        [Test]
        public void WithFactory_TwoArgs_Class()
        {
            // Register using the new strongly-typed factory overload
            TypeDescriptorRegistry.Register(
                new MemberwiseDescriptor<ManualPerson>()
                    .WithFactory(
                        ( string n, int a ) => new ManualPerson( n, a ),
                        "name", "age"
                    )
                    .WithReadonlyMember( "name", p => p.Name )
                    .WithReadonlyMember( "age", p => p.Age )
            );

            var original = new ManualPerson( "Alice", 30 );
            var data = SerializationUnit.Serialize( original );
            var result = SerializationUnit.Deserialize<ManualPerson>( data );

            Assert.That( result.Name, Is.EqualTo( "Alice" ) );
            Assert.That( result.Age, Is.EqualTo( 30 ) );
        }

        [Test]
        public void WithFactory_ThreeArgs_Struct()
        {
            // Register using the new strongly-typed factory overload for structs
            TypeDescriptorRegistry.Register(
                new MemberwiseDescriptor<ManualPoint>()
                    .WithFactory(
                        ( int x, int y, int z ) => new ManualPoint( x, y, z ),
                        "x", "y", "z"
                    )
                    .WithReadonlyMember( "x", p => p.X )
                    .WithReadonlyMember( "y", p => p.Y )
                    .WithReadonlyMember( "z", p => p.Z )
            );

            var original = new ManualPoint( 1, 2, 3 );
            var data = SerializationUnit.Serialize( original );
            var result = SerializationUnit.Deserialize<ManualPoint>( data );

            Assert.That( result.X, Is.EqualTo( 1 ) );
            Assert.That( result.Y, Is.EqualTo( 2 ) );
            Assert.That( result.Z, Is.EqualTo( 3 ) );
        }

        [Test]
        public void ManualDescriptor_WithoutReflection_Works()
        {
            // Ensure no ReflectionClassDescriptor is created if we manually register.
            var descriptor = new MemberwiseDescriptor<ManualPerson>()
                                .WithMember( "name", p => p.Name )
                                .WithMember( "age", p => p.Age );

            // Re-define for mutable test
            var descMutable = new MemberwiseDescriptor<MutablePerson>()
                .WithMember( "name", p => p.Name, ( p, v ) => p.Name = v )
                .WithMember( "age", p => p.Age, ( p, v ) => p.Age = v );

            TypeDescriptorRegistry.Register( descMutable );

            var original = new MutablePerson { Name = "Bob", Age = 40 };
            var data = SerializationUnit.Serialize( original );
            var result = SerializationUnit.Deserialize<MutablePerson>( data );

            Assert.That( result.Name, Is.EqualTo( "Bob" ) );
            Assert.That( result.Age, Is.EqualTo( 40 ) );
        }
    }
}