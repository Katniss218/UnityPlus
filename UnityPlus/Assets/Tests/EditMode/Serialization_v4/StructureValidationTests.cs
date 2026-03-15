using Neoserialization.Tests;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization;
using UnityPlus.Serialization.ReferenceMaps;
using Ctx = UnityPlus.Serialization.Ctx;

namespace Neoserialization
{
    public class StructureValidationTests
    {
        [TearDown]
        public void Cleanup()
        {
            TypeDescriptorRegistry.Clear();
            AssetRegistry.Clear();
        }

        public enum TestEnumStr { First = 0, Second = 1, Third = 2 }
        public enum TestEnumInt { First = 0, Second = 1, Third = 2 }


        [MapsInheritingFrom( typeof( TestEnumStr ) )]
        private static IDescriptor ProvideTestEnumStr() => new EnumDescriptor<TestEnumStr>( EnumSerializationMode.String );
        [MapsInheritingFrom( typeof( TestEnumInt ) )]
        private static IDescriptor ProvideTestEnumInt() => new EnumDescriptor<TestEnumInt>( EnumSerializationMode.Integer );

        [Test]
        public void Primitives_RoundTrip()
        {
            SerializationTestUtils.AssertRoundTrip( 123, (SerializedPrimitive)123 );
            SerializationTestUtils.AssertRoundTrip( -123.45f, (SerializedPrimitive)(-123.45f) );
            SerializationTestUtils.AssertRoundTrip( true, (SerializedPrimitive)true );
            SerializationTestUtils.AssertRoundTrip( "Hello World", (SerializedPrimitive)"Hello World" );
            SerializationTestUtils.AssertRoundTrip( TestEnumStr.Second, (SerializedPrimitive)"Second" );
            SerializationTestUtils.AssertRoundTrip( TestEnumInt.Second, (SerializedPrimitive)1 );

            SerializationTestUtils.AssertRoundTrip( (byte)255, (SerializedPrimitive)(byte)255 );
            SerializationTestUtils.AssertRoundTrip( (sbyte)-128, (SerializedPrimitive)(sbyte)-128 );
            SerializationTestUtils.AssertRoundTrip( (short)-32768, (SerializedPrimitive)(short)-32768 );
            SerializationTestUtils.AssertRoundTrip( (ushort)65535, (SerializedPrimitive)(ushort)65535 );
            SerializationTestUtils.AssertRoundTrip( (uint)4294967295, (SerializedPrimitive)(uint)4294967295 );
            SerializationTestUtils.AssertRoundTrip( (long)-9223372036854775808, (SerializedPrimitive)(long)-9223372036854775808 );
            SerializationTestUtils.AssertRoundTrip( (ulong)18446744073709551615, (SerializedPrimitive)(ulong)18446744073709551615 );
            SerializationTestUtils.AssertRoundTrip( 123.456d, (SerializedPrimitive)123.456d );
            SerializationTestUtils.AssertRoundTrip( 123.456m, (SerializedPrimitive)123.456m );
            SerializationTestUtils.AssertRoundTrip( 'A', (SerializedPrimitive)"A" );

            SerializationTestUtils.AssertRoundTrip( new Guid( "12345678-1234-1234-1234-123456789012" ), (SerializedPrimitive)"12345678-1234-1234-1234-123456789012" );
            SerializationTestUtils.AssertRoundTrip( new DateTime( 2023, 1, 1, 12, 0, 0, DateTimeKind.Utc ), (SerializedPrimitive)"2023-01-01T12:00:00.0000000Z" );
            SerializationTestUtils.AssertRoundTrip( new TimeSpan( days: 1, hours: 2, minutes: 3, seconds: 4, milliseconds: 5 ), (SerializedPrimitive)"1.02:03:04.0050000" );
            SerializationTestUtils.AssertRoundTrip( new TimeSpan( days: 0, hours: 2, minutes: 3, seconds: 4, milliseconds: 5 ), (SerializedPrimitive)"02:03:04.0050000" );
            SerializationTestUtils.AssertRoundTrip( new TimeSpan( ticks: 1 ), (SerializedPrimitive)"00:00:00.0000001" );
        }

        [Test]
        public void NullablePrimitives_RoundTrip()
        {
            int? val1 = 123;
            int? val2 = null;

            SerializationTestUtils.AssertRoundTrip( val1, (SerializedPrimitive)123 );
            SerializationTestUtils.AssertRoundTrip( val2, null );
        }

        [Test]
        public void BoxedPrimitives_RoundTrip()
        {
            object val1 = 123;
            object val2 = "Hello World";
            object val3 = TestEnumInt.Third;

            SerializationTestUtils.AssertRoundTrip( val1, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" }, // boxed structs can have a reference to themselves.
                { KeyNames.TYPE, (SerializedPrimitive)(typeof( int ).AssemblyQualifiedName.ToString()) }, 
                { KeyNames.VALUE, (SerializedPrimitive)123 }
            } );
            SerializationTestUtils.AssertRoundTrip( val2, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" }, // boxed structs can have a reference to themselves.
                { KeyNames.TYPE, (SerializedPrimitive)(typeof( string ).AssemblyQualifiedName.ToString()) }, 
                { KeyNames.VALUE, (SerializedPrimitive)"Hello World" } 
            } );
            SerializationTestUtils.AssertRoundTrip( val3, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" }, // boxed structs can have a reference to themselves.
                { KeyNames.TYPE, (SerializedPrimitive)(typeof( TestEnumInt ).AssemblyQualifiedName.ToString()) }, 
                { KeyNames.VALUE, (SerializedPrimitive)2 } 
            } );
        }

        public struct SimpleStruct
        {
            public int Value;
            public string Text;
        }

        [Test]
        public void BoxedStructs_RoundTrip()
        {
            object val = new SimpleStruct { Value = 42, Text = "Test" };
            SerializationTestUtils.AssertRoundTrip( val, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" }, // boxed structs can have a reference to themselves.
                { KeyNames.TYPE, (SerializedPrimitive)(typeof( SimpleStruct ).AssemblyQualifiedName.ToString()) }, 
                { "Value", (SerializedPrimitive)42 },
                { "Text", (SerializedPrimitive)"Test" } 
            } );
        }

        public interface IAnimal { string Speak(); }
        public class Dog : IAnimal { public string Name; public string Speak() => "Woof"; }
        public class Cat : IAnimal { public int Lives; public string Speak() => "Meow"; }

        [Test]
        public void Polymorphism_RoundTrip()
        {
            IAnimal dog = new Dog { Name = "Rex" };
            IAnimal cat = new Cat { Lives = 9 };

            SerializationTestUtils.AssertRoundTrip( dog, new SerializedObject 
            { 
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.TYPE, (SerializedPrimitive)(typeof( Dog ).AssemblyQualifiedName.ToString()) },
                { "Name", (SerializedPrimitive)"Rex" } }
            );
            SerializationTestUtils.AssertRoundTrip( cat, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.TYPE, (SerializedPrimitive)(typeof( Cat ).AssemblyQualifiedName.ToString()) },
                { "Lives", (SerializedPrimitive)9 }
            } );
        }

        [Test]
        public void Delegates_RoundTrip()
        {
            Action<int> delLambda = x => { };
            object target = delLambda.Target;
            Assert.That( target, Is.Not.Null );

            SerializationTestUtils.AssertRoundTrip( delLambda, new SerializedObject()
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.VALUE, new SerializedArray
                    {
                        new SerializedObject
                        {
                            { "method", new SerializedObject
                                {
                                    { "delegate_type", (SerializedPrimitive)typeof( Action<int> ).AssemblyQualifiedName },
                                    { "identifier", (SerializedPrimitive)delLambda.Method.Name },
                                    { "parameters", new SerializedArray { (SerializedPrimitive)typeof( int ).AssemblyQualifiedName } },
                                    { "declaring_type", (SerializedPrimitive)delLambda.Method.DeclaringType.AssemblyQualifiedName }
                                }
                            },
                            { "target", new SerializedObject() {
                                { KeyNames.REF, (SerializedPrimitive)"" },
                            } }
                        }
                    }
                }
            }, default, default, ( a, b ) =>
            {
                Assert.That( b, Is.Not.Null );
                Assert.That( b.GetInvocationList().Length, Is.EqualTo( a.GetInvocationList().Length ) );
            } );
        }

        [Test]
        public void Collections_RoundTrip()
        {
            var list = new List<int> { 1, 2, 3 };
            var dict = new Dictionary<string, int> { { "one", 1 }, { "two", 2 } };
            var array = new int[] { 1, 2, 3 };
            var hashSet = new HashSet<int> { 1, 2, 3 };
            var queue = new Queue<int>();
            queue.Enqueue( 1 );
            queue.Enqueue( 2 );
            queue.Enqueue( 3 );
            var stack = new Stack<int>();
            stack.Push( 1 );
            stack.Push( 2 );
            stack.Push( 3 );
            var linkedList = new LinkedList<int>( new[] { 1, 2, 3 } );

            var multiArray = new int[,] { { 1, 2 }, { 3, 4 } };
            var jaggedArray = new int[][] { new[] { 1, 2 }, new[] { 3, 4, 5 } };

            // Collections are wrapped by default (ObjectStructure.Wrapped) because they are classes (needsId = true).
            SerializationTestUtils.AssertRoundTrip( list, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.VALUE, new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2, (SerializedPrimitive)3 } }
            } );
            SerializationTestUtils.AssertRoundTrip( dict, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.VALUE, new SerializedArray {
                    new SerializedObject { { "key", (SerializedPrimitive)"one" }, { "value", (SerializedPrimitive)1 } },
                    new SerializedObject { { "key", (SerializedPrimitive)"two" }, { "value", (SerializedPrimitive)2 } }
                } } 
            } );
            SerializationTestUtils.AssertRoundTrip( array, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.VALUE, new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2, (SerializedPrimitive)3 } }
            } );
            SerializationTestUtils.AssertRoundTrip( hashSet, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.VALUE, new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2, (SerializedPrimitive)3 } }
            } );
            SerializationTestUtils.AssertRoundTrip( queue, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.VALUE, new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2, (SerializedPrimitive)3 } }
            } );
            SerializationTestUtils.AssertRoundTrip( stack, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.VALUE, new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2, (SerializedPrimitive)3 } }
            } );
            SerializationTestUtils.AssertRoundTrip( linkedList, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.VALUE, new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2, (SerializedPrimitive)3 } }
            } );

            SerializationTestUtils.AssertRoundTrip( multiArray, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.VALUE, new SerializedObject 
                {
                    { "lengths", new SerializedArray { (SerializedPrimitive)2, (SerializedPrimitive)2 } },
                    { "values", new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2, (SerializedPrimitive)3, (SerializedPrimitive)4 } }
                } } 
            } );
            SerializationTestUtils.AssertRoundTrip( jaggedArray, new SerializedObject
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { KeyNames.VALUE, new SerializedArray
                {
                    new SerializedObject
                    {
                        { KeyNames.ID, (SerializedPrimitive)"" },
                        { KeyNames.VALUE, new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2 } }
                    },
                    new SerializedObject
                    {
                        { KeyNames.ID, (SerializedPrimitive)"" },
                        { KeyNames.VALUE, new SerializedArray { (SerializedPrimitive)3, (SerializedPrimitive)4, (SerializedPrimitive)5 } }
                    }
                } }
            } );
        }

        [Test]
        public void UnityTypes_RoundTrip()
        {
            SerializationTestUtils.AssertRoundTrip( new Vector2( 1, 2 ), new SerializedArray { (SerializedPrimitive)1f, (SerializedPrimitive)2f } );
            SerializationTestUtils.AssertRoundTrip( new Vector3( 1, 2, 3 ), new SerializedArray { (SerializedPrimitive)1f, (SerializedPrimitive)2f, (SerializedPrimitive)3f } );
            SerializationTestUtils.AssertRoundTrip( new Vector4( 1, 2, 3, 4 ), new SerializedArray { (SerializedPrimitive)1f, (SerializedPrimitive)2f, (SerializedPrimitive)3f, (SerializedPrimitive)4f } );
            SerializationTestUtils.AssertRoundTrip( new Vector2Int( 1, 2 ), new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2 } );
            SerializationTestUtils.AssertRoundTrip( new Vector3Int( 1, 2, 3 ), new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2, (SerializedPrimitive)3 } );
            SerializationTestUtils.AssertRoundTrip( new Quaternion( 1, 2, 3, 4 ), new SerializedArray { (SerializedPrimitive)1f, (SerializedPrimitive)2f, (SerializedPrimitive)3f, (SerializedPrimitive)4f } );
            SerializationTestUtils.AssertRoundTrip( new Matrix4x4(
                new Vector4( 1, 2, 3, 4 ),
                new Vector4( 5, 6, 7, 8 ),
                new Vector4( 9, 10, 11, 12 ),
                new Vector4( 13, 14, 15, 16 )
            ), new SerializedArray {
                (SerializedPrimitive)1f, (SerializedPrimitive)5f, (SerializedPrimitive)9f, (SerializedPrimitive)13f,
                (SerializedPrimitive)2f, (SerializedPrimitive)6f, (SerializedPrimitive)10f, (SerializedPrimitive)14f,
                (SerializedPrimitive)3f, (SerializedPrimitive)7f, (SerializedPrimitive)11f, (SerializedPrimitive)15f,
                (SerializedPrimitive)4f, (SerializedPrimitive)8f, (SerializedPrimitive)12f, (SerializedPrimitive)16f
            } );
            SerializationTestUtils.AssertRoundTrip( new Color( 1, 0.5f, 0.2f, 1 ), new SerializedObject { { "r", (SerializedPrimitive)1f }, { "g", (SerializedPrimitive)0.5f }, { "b", (SerializedPrimitive)0.2f }, { "a", (SerializedPrimitive)1f } } );
            SerializationTestUtils.AssertRoundTrip( new Color32( 255, 128, 64, 255 ), new SerializedObject { { "r", (SerializedPrimitive)(byte)255 }, { "g", (SerializedPrimitive)(byte)128 }, { "b", (SerializedPrimitive)(byte)64 }, { "a", (SerializedPrimitive)(byte)255 } } );
            SerializationTestUtils.AssertRoundTrip( new Rect( 1, 2, 3, 4 ), new SerializedObject { { "x", (SerializedPrimitive)1f }, { "y", (SerializedPrimitive)2f }, { "width", (SerializedPrimitive)3f }, { "height", (SerializedPrimitive)4f } } );
            SerializationTestUtils.AssertRoundTrip( new RectInt( 1, 2, 3, 4 ), new SerializedObject { { "x", (SerializedPrimitive)1 }, { "y", (SerializedPrimitive)2 }, { "width", (SerializedPrimitive)3 }, { "height", (SerializedPrimitive)4 } } );
            SerializationTestUtils.AssertRoundTrip( new Bounds( new Vector3( 1, 2, 3 ), new Vector3( 4, 5, 6 ) ), new SerializedObject {
                { "center", new SerializedArray { (SerializedPrimitive)1f, (SerializedPrimitive)2f, (SerializedPrimitive)3f } },
                { "extents", new SerializedArray { (SerializedPrimitive)2f, (SerializedPrimitive)2.5f, (SerializedPrimitive)3f } }
            } );
            SerializationTestUtils.AssertRoundTrip( new BoundsInt( new Vector3Int( 1, 2, 3 ), new Vector3Int( 4, 5, 6 ) ), new SerializedObject {
                { "position", new SerializedArray { (SerializedPrimitive)1, (SerializedPrimitive)2, (SerializedPrimitive)3 } },
                { "size", new SerializedArray { (SerializedPrimitive)4, (SerializedPrimitive)5, (SerializedPrimitive)6 } }
            } );

            SerializationTestUtils.AssertRoundTrip( new LayerMask { value = 5 }, new SerializedObject { { "value", (SerializedPrimitive)5 } } );
            SerializationTestUtils.AssertRoundTrip( new Keyframe( 1f, 2f, 3f, 4f, 5f, 6f ), new SerializedObject {
                { "time", (SerializedPrimitive)1f },
                { "value", (SerializedPrimitive)2f },
                { "in_tangent", (SerializedPrimitive)3f },
                { "out_tangent", (SerializedPrimitive)4f },
                { "in_weight", (SerializedPrimitive)5f },
                { "out_weight", (SerializedPrimitive)6f },
                { "weighted_mode", (SerializedPrimitive)0 }
            } );

            var curve = new AnimationCurve( new Keyframe( 0, 0 ), new Keyframe( 1, 1 ) );
            SerializationTestUtils.AssertRoundTrip( curve, new SerializedObject()
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { "keys", new SerializedObject()
                {
                    { KeyNames.ID, (SerializedPrimitive)"" },
                    { KeyNames.VALUE, new SerializedArray()
                    {
                        new SerializedObject()
                        {
                            { "time", (SerializedPrimitive)0f },
                            { "value", (SerializedPrimitive)0f },
                            { "in_tangent", (SerializedPrimitive)0f },
                            { "out_tangent", (SerializedPrimitive)0f },
                            { "in_weight", (SerializedPrimitive)0f },
                            { "out_weight", (SerializedPrimitive)0f },
                            { "weighted_mode", (SerializedPrimitive)0 }
                        },
                        new SerializedObject()
                        {
                            { "time", (SerializedPrimitive)1f },
                            { "value", (SerializedPrimitive)1f },
                            { "in_tangent", (SerializedPrimitive)0f },
                            { "out_tangent", (SerializedPrimitive)0f },
                            { "in_weight", (SerializedPrimitive)0f },
                            { "out_weight", (SerializedPrimitive)0f },
                            { "weighted_mode", (SerializedPrimitive)0 }
                        }
                    } }
                } },
                { "pre_wrap_mode", (SerializedPrimitive)"ClampForever" },
                { "post_wrap_mode", (SerializedPrimitive)"ClampForever" }
            }, default, default, ( a, b ) =>
            {
                Assert.That( b.length, Is.EqualTo( a.length ) );
                Assert.That( b[0].time, Is.EqualTo( a[0].time ) );
                Assert.That( b[1].value, Is.EqualTo( a[1].value ) );
            } );

            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey( Color.red, 0f ), new GradientColorKey( Color.blue, 1f ) },
                new[] { new GradientAlphaKey( 1f, 0f ), new GradientAlphaKey( 0f, 1f ) }
            ); 
            SerializationTestUtils.AssertRoundTrip( gradient, new SerializedObject()
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { "color_keys", new SerializedObject() {
                    { KeyNames.ID, (SerializedPrimitive)"" },
                    { KeyNames.VALUE, new SerializedArray()
                    {
                        new SerializedObject {
                            { "color", new SerializedObject {
                                { "r", (SerializedPrimitive)1f },
                                { "g", (SerializedPrimitive)0f },
                                { "b", (SerializedPrimitive)0f },
                                { "a", (SerializedPrimitive)1f }
                            } },
                            { "time",  (SerializedPrimitive)0f }
                        },
                        new SerializedObject {
                            { "color", new SerializedObject {
                                { "r", (SerializedPrimitive)0f },
                                { "g", (SerializedPrimitive)0f },
                                { "b", (SerializedPrimitive)1f },
                                { "a", (SerializedPrimitive)1f }
                            } },
                            { "time",  (SerializedPrimitive)1f }
                        }
                    } }
                } },
                { "alpha_keys", new SerializedObject() {
                    { KeyNames.ID, (SerializedPrimitive)"" },
                    { KeyNames.VALUE, new SerializedArray()
                    {
                        new SerializedObject {
                            { "alpha", (SerializedPrimitive)1f },
                            { "time",  (SerializedPrimitive)0f }
                        },
                        new SerializedObject {
                            { "alpha", (SerializedPrimitive)0f },
                            { "time",  (SerializedPrimitive)1f }
                        }
                    } }
                } },
                // Most serializers also include gradient mode (even if default)
                { "mode", (SerializedPrimitive)"Blend" }   // or (SerializedPrimitive)0  depending on your enum handling
            }, default, default, ( a, b ) =>
            {
                Assert.That( b.colorKeys.Length, Is.EqualTo( a.colorKeys.Length ) );
                Assert.That( b.alphaKeys.Length, Is.EqualTo( a.alphaKeys.Length ) );
                Assert.That( b.colorKeys[0].color, Is.EqualTo( a.colorKeys[0].color ) );
                // you can add more if needed
            } );
        }

        public class MockAsset : ScriptableObject { public int Value; }

        [Test]
        public void Assets_RoundTrip()
        {
            var asset = ScriptableObject.CreateInstance<MockAsset>();
            asset.Value = 42;
            AssetRegistry.Register( "test::mock", asset );

            SerializationTestUtils.AssertRoundTrip( asset, new SerializedObject { { KeyNames.ASSETREF, (SerializedPrimitive)"test::mock" } }, ContextRegistry.GetID( typeof( Ctx.Asset ) ) );
        }

        public class Node
        {
            public string Name;
            public Node Next;


            [MapsInheritingFrom( typeof( Node ) )]
            private static IDescriptor Provide()
            {
                return new MemberwiseDescriptor<Node>()
                    .WithMember( "name", o => o.Name )
                    .WithMember( "next", o => o.Next );
            }
        }

        [Test]
        public void References_RoundTrip()
        {
            var node1 = new Node { Name = "Node1" };
            var node2 = new Node { Name = "Node2" };
            node1.Next = node2;
            node2.Next = node1; // Circular reference

            SerializationTestUtils.AssertRoundTrip( node1, new SerializedObject()
            {
                { KeyNames.ID, (SerializedPrimitive)"" },
                { "name", (SerializedPrimitive)"Node1" },
                { "next", new SerializedObject() {
                    { KeyNames.ID, (SerializedPrimitive)"" },
                    { "name", (SerializedPrimitive)"Node2" },
                    { "next", new SerializedObject() {
                        { KeyNames.REF, (SerializedPrimitive)"" },
                    } }
                } }
            }, default, new SerializationConfiguration() { CycleHandling = CycleHandling.AutoRef }, ( a, b ) =>
            {
                Assert.That( b.Name, Is.EqualTo( "Node1" ) );
                Assert.That( b.Next.Name, Is.EqualTo( "Node2" ) );
                Assert.That( b.Next.Next, Is.SameAs( b ) );
            } );
        }

        [Test]
        public void References_SerializeMany_RoundTrip()
        {
            var node1 = new Node { Name = "Node1" };
            var node2 = new Node { Name = "Node2" };
            node1.Next = node2;
            node2.Next = node1;
            var config = new SerializationConfiguration() { CycleHandling = CycleHandling.AutoRef };

            var data = SerializationUnit.SerializeMany( new[] { node1, node2 }, config ).ToArray();
            var result = SerializationUnit.DeserializeMany<Node>( data, config ).ToArray();

            var a = result[0];
            var b = result[1];

            Assert.That( a.Name, Is.EqualTo( "Node1" ) );
            Assert.That( a.Next.Name, Is.EqualTo( "Node2" ) );
            Assert.That( a.Next.Next, Is.SameAs( a ) );

            Assert.That( b.Name, Is.EqualTo( "Node2" ) );
            Assert.That( b.Next.Name, Is.EqualTo( "Node1" ) );
            Assert.That( b.Next.Next, Is.SameAs( b ) );

            Assert.That( b.Next, Is.Not.SameAs( a ) ); // 2 separate graphs, members 'want to be' objects and do not form a parent-chain-reference,
                                                       // thus should be different instances.
            Assert.That( a.Next, Is.Not.SameAs( b ) );
        }

        [Test]
        public void References_WithCustomReferenceMap_RoundTrip()
        {
            var node1 = new Node { Name = "Node1" };
            var node2 = new Node { Name = "Node2" };
            node1.Next = node2;
            var config = new SerializationConfiguration() { CycleHandling = CycleHandling.AutoRef };

            var refMap = new BidirectionalReferenceStore();
            var id1 = refMap.GetID( node1 );
            var id2 = refMap.GetID( node2 );

            var data = SerializationUnit.Serialize( node1, refMap, config );

            var newRefMap = new BidirectionalReferenceStore();
            var result = SerializationUnit.Deserialize<Node>( data, newRefMap, config );

            Assert.That( result.Name, Is.EqualTo( "Node1" ) );
            Assert.That( result.Next.Name, Is.EqualTo( "Node2" ) );
            Assert.That( newRefMap.GetID( result ), Is.EqualTo( id1 ) );
            Assert.That( newRefMap.GetID( result.Next ), Is.EqualTo( id2 ) );
        }
    }
}