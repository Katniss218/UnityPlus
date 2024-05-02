using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.Serialization;

namespace UnityEngine
{
    public class TypeMapTests
    {
        private class Base { }
        private class Derived : Base { }
        private class Base<T> { }
        private class Derived<T> : Base<T> { }

        [Test]
        public void GetOrDefault___NoMatch___ReturnsDefault()
        {
            // Arrange
            TypeMap<int> map = new TypeMap<int>();
            map.Set( typeof( string ), 2 );

            // Act
            int result = map.GetOrDefault( typeof( int ) );

            // Assert
            Assert.That( result, Is.EqualTo( default( int ) ) );
        }

        [Test]
        public void GetOrDefault___Match___ReturnsMatching()
        {
            // Arrange
            TypeMap<int> map = new TypeMap<int>( 1 );
            map.Set( typeof( string ), 2 );

            // Act
            int result = map.GetOrDefault( typeof( string ) );

            // Assert
            Assert.That( result, Is.EqualTo( 2 ) );
        }

        //
        //
        //

        [Test]
        public void TryGet___NoMatch___ReturnsFalse()
        {
            // Arrange
            TypeMap<int> map = new TypeMap<int>();
            map.Set( typeof( string ), 2 );

            // Act
            bool isMatch = map.TryGet( typeof( int ), out _ );

            // Assert
            Assert.That( isMatch, Is.False );
        }

        [Test]
        public void TryGet___Match___ReturnsMatching()
        {
            // Arrange
            TypeMap<int> map = new TypeMap<int>( 1 );
            map.Set( typeof( string ), 2 );

            // Act
            bool isMatch = map.TryGet( typeof( string ), out var result );

            // Assert
            Assert.That( isMatch, Is.True );
            Assert.That( result, Is.EqualTo( 2 ) );
        }

        //
        //
        //

        [Test]
        public void GetClosestOrDefault___NoMatch___ReturnsDefault()
        {
            // Arrange
            TypeMap<int> map = new TypeMap<int>();
            map.Set( typeof( string ), 2 );

            // Act
            int result = map.GetClosestOrDefault( typeof( int ) );

            // Assert
            Assert.That( result, Is.EqualTo( default( int ) ) );
        }

        [Test]
        public void GetClosestOrDefault___Match___ReturnsMatching()
        {
            // Arrange
            TypeMap<int> map = new TypeMap<int>( 1 );
            map.Set( typeof( string ), 2 );

            // Act
            int result = map.GetClosestOrDefault( typeof( string ) );

            // Assert
            Assert.That( result, Is.EqualTo( 2 ) );
        }

        [Test]
        public void GetClosestOrDefault___Derived___ReturnsBaseType()
        {
            // Arrange
            TypeMap<int> map = new TypeMap<int>( 1 );
            map.Set( typeof( Base ), 2 );
            map.Set( typeof( string ), 3 );

            // Act
            int result = map.GetClosestOrDefault( typeof( Derived ) );

            // Assert
            Assert.That( result, Is.EqualTo( 2 ) );
        }

        [Test]
        public void GetClosestOrDefault___GenericDerived___ReturnsBaseType()
        {
            // Arrange
            TypeMap<int> map = new TypeMap<int>();
            map.Set( typeof( Base<> ), 1 );
            map.Set( typeof( Base<int> ), 2 );

            // Act
            int result = map.GetClosestOrDefault( typeof( Derived<int> ) );

            // Assert
            Assert.That( result, Is.EqualTo( 2 ) );
        }

        [Test]
        public void GetClosestOrDefault___GenericDerived___PrioritizesUnconstructedTypeOverBaseType()
        {
            // Derived<> is an 'unconstructed' type, whereas
            // Derived<int> is a 'constructed' type.

            // Arrange
            TypeMap<int> map = new TypeMap<int>();
            map.Set( typeof( Base<int> ), 2 );
            map.Set( typeof( Derived<> ), 3 );

            // Act
            int result = map.GetClosestOrDefault( typeof( Derived<int> ) );

            // Assert
            Assert.That( result, Is.EqualTo( 3 ) );
        }

        //
        //
        //

        [Test]
        public void TryGetClosest___NoMatch___ReturnsFalse()
        {
            // Arrange
            TypeMap<int> map = new TypeMap<int>();
            map.Set( typeof( string ), 2 );

            // Act
            bool isMatch = map.TryGetClosest( typeof( int ), out _ );

            // Assert
            Assert.That( isMatch, Is.False );
        }

        [Test]
        public void TryGetClosest___GenericDerived___PrioritizesUnconstructedTypeOverBaseType()
        {
            // Derived<> is an 'unconstructed' type, whereas
            // Derived<int> is a 'constructed' type.

            // Arrange
            TypeMap<int> map = new TypeMap<int>();
            map.Set( typeof( Base<> ), 1 );
            map.Set( typeof( Base<int> ), 2 );
            map.Set( typeof( Derived<> ), 3 );

            // Act
            bool isMatch = map.TryGetClosest( typeof( Derived<int> ), out var result );

            // Assert
            Assert.That( isMatch, Is.True );
            Assert.That( result, Is.EqualTo( 3 ) );
        }

        static string PrintBase( Base obj )
        {
            return "BASE";
        }

        static string PrintString( string obj )
        {
            return "STRING";
        }

        [Test]
        public void Delegates()
        {
            // Arrange
            TypeMap<Delegate> map = new TypeMap<Delegate>();
            var del1 = Delegate.CreateDelegate( typeof( Func<Base, string> ), typeof( TypeMapTests ), "PrintBase" );
            var del2 = Delegate.CreateDelegate( typeof( Func<string, string> ), typeof( TypeMapTests ), "PrintString" );
            map.Set( typeof( Base ), del1 );
            map.Set( typeof( string ), del2 );

            // Act
            var result = map.GetClosestOrDefault( typeof( Derived ) );
            var str = result.DynamicInvoke( new Derived() );

            // Assert
            Assert.That( result, Is.EqualTo( del1 ) );
            Assert.That( str, Is.EqualTo( "BASE" ) );
        }

    }
}