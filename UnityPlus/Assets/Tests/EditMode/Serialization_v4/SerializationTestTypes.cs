using System.Collections.Generic;
using UnityEngine;
using UnityPlus.Serialization;

namespace Neoserialization.V4
{
    public struct DeepStruct
    {
        public int Value;
    }

    public struct MiddleStruct
    {
        public DeepStruct Deep;
        public string Tag;
    }

    public class RootClass
    {
        public MiddleStruct Mid;
        public string Name;
    }

    public class SimpleRef
    {
        public SimpleRef Next;

        [MapsInheritingFrom( typeof( SimpleRef ) )]
        public static IDescriptor Mapping()
        {
            return new MemberwiseDescriptor<SimpleRef>()
                .WithMember( "next", typeof( Ctx.Ref ), o => o.Next );
        }
    }

    public class Container
    {
        public Dictionary<string, MockAsset> Assets = new Dictionary<string, MockAsset>();
    }

    public class MockAsset : ScriptableObject
    {
        public int Value;
    }

    class Hello
    {
        public string a;
    }

    public struct SimpleStruct
    {
        public int Value;
        public string Text;
    }

    public class SimpleClass
    {
        public int Value;
        public string Text;
    }

    public enum TestEnumStr { First = 0, Second = 1, Third = 2 }
    public enum TestEnumInt { First = 0, Second = 1, Third = 2 }

    static class TestTypeDescriptors
    {
        [MapsInheritingFrom( typeof( TestEnumStr ) )]
        private static IDescriptor ProvideTestEnumStr() => new EnumDescriptor<TestEnumStr>( EnumSerializationMode.String );
        [MapsInheritingFrom( typeof( TestEnumInt ) )]
        private static IDescriptor ProvideTestEnumInt() => new EnumDescriptor<TestEnumInt>( EnumSerializationMode.Integer );
    }

    public interface IAnimal { string Speak(); }
    public class Dog : IAnimal { public string Name; public string Speak() => "Woof"; }
    public class Cat : IAnimal { public int Lives; public string Speak() => "Meow"; }

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
}