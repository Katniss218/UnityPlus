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
		[Test]
		public void GetClosest___Simple___ReturnsCorrectType()
		{
			// Arrange
			TypeMap<int> map = new TypeMap<int>();
			map.Set<int>( 2 );
			map.Set<object>( 1 );

			// Act
			int num = map.GetClosestOrDefault( typeof( int ) );

			// Assert
			Assert.That( num, Is.EqualTo( 2 ) );
		}

		class Base
		{

		}

		class Derived : Base
		{

		}

		[Test]
		public void GetClosest___SimpleDerived___ReturnsCorrectType()
		{
			// Arrange
			TypeMap<int> map = new TypeMap<int>();
			map.Set<object>( 1 );
			map.Set<Base>( 2 );

			// Act
			int num = map.GetClosestOrDefault( typeof( Derived ) );

			// Assert
			Assert.That( num, Is.EqualTo( 2 ) );
		}

		class Base<T>
		{

		}

		class Derived<T> : Base<T>
		{

		}

		[Test]
		public void GetClosest___GenericDerived___ReturnsBaseType()
		{
			// Arrange
			TypeMap<int> map = new TypeMap<int>();
			map.Set( typeof( Base<> ), 1 );
			map.Set<Base<int>>( 2 );

			// Act
			int num = map.GetClosestOrDefault( typeof( Derived<int> ) );

			// Assert
			Assert.That( num, Is.EqualTo( 2 ) );
		}

		[Test]
		public void GetClosest___GenericDerived___PrioritizesUnconstructedTypeOverBaseType()
		{
			// Derived<> is an 'unconstructed' type, whereas
			// Derived<int> is a 'constructed' type.

			// Arrange
			TypeMap<int> map = new TypeMap<int>();
			map.Set( typeof( Base<> ), 1 );
			map.Set( typeof( Base<int> ), 2 );
			map.Set( typeof( Derived<> ), 3 );

			// Act
			int num = map.GetClosestOrDefault( typeof( Derived<int> ) );

			// Assert
			Assert.That( num, Is.EqualTo( 3 ) );
		}
	}
}