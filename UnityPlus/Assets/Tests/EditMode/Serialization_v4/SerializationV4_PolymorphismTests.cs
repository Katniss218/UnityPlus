
using NUnit.Framework;
using System.Collections.Generic;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_PolymorphismTests
    {
        public interface IAnimal { string Speak(); }
        public class Dog : IAnimal { public string Name; public string Speak() => "Woof"; }
        public class Cat : IAnimal { public int Lives; public string Speak() => "Meow"; }

        public class Zoo
        {
            public List<IAnimal> Animals = new List<IAnimal>();
        }

        [Test]
        public void Serialize_PolymorphicList()
        {
            var zoo = new Zoo();
            zoo.Animals.Add( new Dog { Name = "Buddy" } );
            zoo.Animals.Add( new Cat { Lives = 9 } );

            var data = SerializationUnit.Serialize( zoo );
            var result = SerializationUnit.Deserialize<Zoo>( data );

            Assert.That( result.Animals.Count, Is.EqualTo( 2 ) );
            Assert.That( result.Animals[0], Is.InstanceOf<Dog>() );
            Assert.That( result.Animals[1], Is.InstanceOf<Cat>() );
            Assert.That( ((Dog)result.Animals[0]).Name, Is.EqualTo( "Buddy" ) );
        }

        [Test]
        public void Serialize_RootPolymorphism()
        {
            // Serialize a Dog as an IAnimal
            IAnimal myPet = new Dog { Name = "Rex" };

            // We tell the serializer we are serializing 'IAnimal', but the object is 'Dog'.
            // It MUST write the $type header for the root.
            var desc = TypeDescriptorRegistry.GetDescriptor( typeof( IAnimal ) );

            // Driver initialization manually to simulate strict type call
            var ctx = new SerializationContext { ReverseMap = new ReferenceMaps.BidirectionalReferenceStore() };
            var driver = new StackMachineDriver( ctx );
            driver.Initialize( myPet, desc, new SerializationStrategy() );
            while( !driver.IsFinished ) driver.Tick( 100 );

            var data = (SerializedObject)driver.Result;

            Assert.That( data.ContainsKey( KeyNames.TYPE ), Is.True, "Root object did not contain $type header." );

            // Deserialize back as IAnimal
            var result = SerializationUnit.Deserialize<IAnimal>( data );
            Assert.That( result, Is.InstanceOf<Dog>() );
        }
    }
}
