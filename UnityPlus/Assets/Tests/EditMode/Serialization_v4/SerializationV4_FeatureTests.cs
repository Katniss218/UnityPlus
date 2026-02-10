
using NUnit.Framework;
using System;
using UnityEngine;
using UnityPlus.Serialization;

namespace UnityPlus.Serialization.Tests.V4
{
    public class SerializationV4_FeatureTests
    {
        public class ImmutableUser
        {
            public string Username { get; }
            public int Id { get; }

            public ImmutableUser( string username, int id )
            {
                Username = username;
                Id = id;
            }
        }

        public class CallbackTest : ISerializationCallbackReceiver
        {
            public int Value;
            [NonSerialized] public bool BeforeSerialized = false;
            [NonSerialized] public bool AfterDeserialized = false;

            public void OnBeforeSerialize() => BeforeSerialized = true;
            public void OnAfterDeserialize() => AfterDeserialized = true;
        }

        [Test]
        public void Serialize_ImmutableObject_Constructor()
        {
            // We need to register the constructor mapping manually since Reflection can't infer parameter names reliably without metadata
            TypeDescriptorRegistry.Register(
                new ClassOrStructDescriptor<ImmutableUser>()
                    .WithConstructor(
                        args => new ImmutableUser( (string)args[0], (int)args[1] ),
                        ("username", typeof( string )),
                        ("id", typeof( int ))
                    )
                    .WithReadonlyMember( "username", u => u.Username ) // Maps to constructor param "username"
                    .WithReadonlyMember( "id", u => u.Id )             // Maps to constructor param "id"
            );

            var original = new ImmutableUser( "Admin", 1 );
            var data = SerializationUnit.Serialize( original );
            var result = SerializationUnit.Deserialize<ImmutableUser>( data );

            Assert.That( result.Username, Is.EqualTo( "Admin" ) );
            Assert.That( result.Id, Is.EqualTo( 1 ) );
        }

        [Test]
        public void Serialize_Callbacks()
        {
            var obj = new CallbackTest { Value = 10 };
            var data = SerializationUnit.Serialize( obj );

            Assert.That( obj.BeforeSerialized, Is.True );

            var result = SerializationUnit.Deserialize<CallbackTest>( data );
            Assert.That( result.AfterDeserialized, Is.True );
        }
    }
}
