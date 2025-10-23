using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityPlus.Serialization;
using UnityPlus.Serialization.Patching;

namespace Serialization
{
    public class SerializedDataPathTests
    {
        [Test]
        public void Parsing()
        {
            var path = SerializedDataPath.Parse( "this" );
            Assert.That( path.Segments.Count, Is.EqualTo( 1 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.This() ) );

            path = SerializedDataPath.Parse( "any" );
            Assert.That( path.Segments.Count, Is.EqualTo( 1 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Any() ) );

            path = SerializedDataPath.Parse( "this.hello" );
            Assert.That( path.Segments.Count, Is.EqualTo( 2 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.This() ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.Named( "hello" ) ) );

            path = SerializedDataPath.Parse( "any.world.foo" );
            Assert.That( path.Segments.Count, Is.EqualTo( 3 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Any() ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.Named( "world" ) ) );
            Assert.That( path.Segments[2], Is.EqualTo( SerializedDataPathSegment.Named( "foo" ) ) );

            path = SerializedDataPath.Parse( "any[42]" );
            Assert.That( path.Segments.Count, Is.EqualTo( 2 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Any() ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.Indexed( 42 ) ) );

            path = SerializedDataPath.Parse( "this[*].foo.any[10].bar" );
            Assert.That( path.Segments.Count, Is.EqualTo( 6 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.This() ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.IndexedAll() ) );
            Assert.That( path.Segments[2], Is.EqualTo( SerializedDataPathSegment.Named( "foo" ) ) );
            Assert.That( path.Segments[3], Is.EqualTo( SerializedDataPathSegment.Named( "any" ) ) );
            Assert.That( path.Segments[4], Is.EqualTo( SerializedDataPathSegment.Indexed( 10 ) ) );
            Assert.That( path.Segments[5], Is.EqualTo( SerializedDataPathSegment.Named( "bar" ) ) );

            // Quoted name + all-indexer
            path = SerializedDataPath.Parse( "\"hello world\"[*]" );
            Assert.That( path.Segments.Count, Is.EqualTo( 2 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Named( "hello world" ) ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.IndexedAll() ) );

            path = SerializedDataPath.Parse( "hi[*][*][2]" );
            Assert.That( path.Segments.Count, Is.EqualTo( 4 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Named( "hi" ) ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.IndexedAll() ) );
            Assert.That( path.Segments[2], Is.EqualTo( SerializedDataPathSegment.IndexedAll() ) );
            Assert.That( path.Segments[3], Is.EqualTo( SerializedDataPathSegment.Indexed( 2 ) ) );

            // Quoted string with escaped quote inside, then normal child
            path = SerializedDataPath.Parse( "\"he\\\"llo\".x" ); // path text: "he\"llo".x
            Assert.That( path.Segments.Count, Is.EqualTo( 2 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Named( "he\"llo" ) ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.Named( "x" ) ) );

            // Quoted unicode escape \uXXXX -> should be interpreted by parser as the unicode char 'A'
            path = SerializedDataPath.Parse( "\"\\u0041\"" ); // parses to "A"
            Assert.That( path.Segments.Count, Is.EqualTo( 1 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Named( "A" ) ) );

            // Ranges: [2..5:2]
            path = SerializedDataPath.Parse( "hello[2..5]" );
            Assert.That( path.Segments.Count, Is.EqualTo( 2 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Named( "hello" ) ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.IndexedRange( 2, 5 ) ) );

            // Ranges: [2..5:2]
            path = SerializedDataPath.Parse( "any[2..5:2]" );
            Assert.That( path.Segments.Count, Is.EqualTo( 2 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Any() ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.IndexedRange( 2, 5, 2 ) ) );

            // Open upper bound [2..] and open lower bound [..5]
            path = SerializedDataPath.Parse( "this[2..]" );
            Assert.That( path.Segments.Count, Is.EqualTo( 2 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.This() ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.IndexedRange( 2, int.MaxValue, 1 ) ) );

            path = SerializedDataPath.Parse( "this[..5]" );
            Assert.That( path.Segments.Count, Is.EqualTo( 2 ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.IndexedRange( 0, 5, 1 ) ) );

            // Bracket with spaces around '*' should be accepted (inner string is trimmed)
            path = SerializedDataPath.Parse( "any[ * ]" );
            Assert.That( path.Segments.Count, Is.EqualTo( 2 ) );
            Assert.That( path.Segments[1], Is.EqualTo( SerializedDataPathSegment.IndexedAll() ) );

            // Empty string -> empty path
            path = SerializedDataPath.Parse( string.Empty );
            Assert.That( path.Segments.Count, Is.EqualTo( 0 ) );

            // Prefix collision: "anyone" should be parsed as a single named child "anyone" (not Any + "one")
            path = SerializedDataPath.Parse( "anyone" );
            Assert.That( path.Segments.Count, Is.EqualTo( 1 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Named( "anyone" ) ) );

            path = SerializedDataPath.Parse( "thischild" );
            Assert.That( path.Segments.Count, Is.EqualTo( 1 ) );
            Assert.That( path.Segments[0], Is.EqualTo( SerializedDataPathSegment.Named( "thischild" ) ) );

        }

        [Test]
        public void Parsing_BadCases_ShouldThrow()
        {
            // Unterminated bracket.
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "any[5" ) );

            // Unterminated quoted string
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "\"hello" ) );

            // Space before an unquoted child name is invalid
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "this. hello" ) );

            // missing path segments.
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "this." ) );
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "this. " ) );

            // Quote in the middle of unquoted identifier.
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "hel\"hi" ) );

            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( " " ) );

            // Empty bracket content.
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "any[]" ) );

            // Empty range specifier ("[..]" without bounds) - ambiguous/invalid. Use [*]
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "this[..]" ) );

            // Non-numeric index
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "a[b]" ) );

            // Non-numeric range bound
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "a[1..x]" ) );

            // Negative index
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "a[-1]" ) );

            // Invalid step (zero)
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "a[0..5:0]" ) );

            // Missing step after colon
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "a[2..5:]" ) );

            // Single index with an extra ":step"
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "a[5:2]" ) );
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "a[:2]" ) );

            // Too many ':' tokens (malformed)
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "a[1:2:3]" ) );

            // Malformed range with two '..' occurrences
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "a[1..2..3]" ) );

            // Nested brackets are not supported / malformed
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "a[[1]]" ) );

            // Trailing backslash inside quoted string (unterminated escape)
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "\"abc\\\"" ) ); // note: literal ends with backslash

            // Invalid unicode escape (too short)
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "\"\\u00\"" ) );

            // Invalid unicode hex digit
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "\"\\u00G1\"" ) );

            // Illegal characters in unquoted identifier (space in identifier)
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "foo.bar-baz" ) ); // '-' not allowed in identifier
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "foo.bar$" ) );    // '$' not allowed

            // Unrecognized stray character at top-level
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "@invalid" ) );

            // Unclosed quote inside a name followed by bracket
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "\"name[5]" ) );

            // Dot followed by end-of-input (only whitespace after dot) should be invalid per original bad-case expectations
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "this.   " ) );

            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "this." ) );
            Assert.Throws<FormatException>( () => SerializedDataPath.Parse( "." ) );
        }

        [Test]
        public void ThisOnObject()
        {
            // Arrange
            int i1o = 1;
            bool i3o = true;
            string i5o = "Hi!";

            // Act
            SerializedObject obj = new SerializedObject()
            {
                { "first", i1o },
                { "second", 2 },
                { "third", i3o },
                { "fourth", 4 },
                { "fifth", i5o }
            };
            TrackedSerializedData tObj = new TrackedSerializedData( obj );
            List<TrackedSerializedData> list = new SerializedDataPath( SerializedDataPathSegment.This() ).Evaluate( tObj ).ToList();

            // Assert
            Assert.That( list[0].value, Is.EqualTo( obj ) );
        }

        [Test]
        public void AnyOnObject()
        {
            // Arrange
            int i1o = 1;
            bool i3o = true;
            string i5o = "Hi!";

            // Act
            SerializedObject obj = new SerializedObject()
            {
                { "first", i1o },
                { "second", 2 },
                { "third", i3o },
                { "fourth", 4 },
                { "fifth", i5o }
            };
            TrackedSerializedData tObj = new TrackedSerializedData( obj );
            List<TrackedSerializedData> list = new SerializedDataPath( SerializedDataPathSegment.Any() ).Evaluate( tObj ).ToList();

            // Assert
            Assert.That( list[0].value, Is.EqualTo( (SerializedPrimitive)i1o ) );
            Assert.That( list[2].value, Is.EqualTo( (SerializedPrimitive)i3o ) );
            Assert.That( list[4].value, Is.EqualTo( (SerializedPrimitive)i5o ) );
        }

        [Test]
        public void AnyOnArray()
        {
            // Arrange
            int i1o = 1;
            bool i3o = true;
            string i5o = "Hi!";

            // Act
            SerializedArray obj = new SerializedArray()
            {
                i1o,
                2,
                i3o,
                4,
                i5o
            };
            TrackedSerializedData tObj = new TrackedSerializedData( obj );
            List<TrackedSerializedData> list = new SerializedDataPath( SerializedDataPathSegment.Any() ).Evaluate( tObj ).ToList();

            // Assert
            Assert.That( list[0].value, Is.EqualTo( (SerializedPrimitive)i1o ) );
            Assert.That( list[2].value, Is.EqualTo( (SerializedPrimitive)i3o ) );
            Assert.That( list[4].value, Is.EqualTo( (SerializedPrimitive)i5o ) );
        }

        [Test]
        public void IndexedRangeOnArray()
        {
            // Arrange
            int i1o = 1;
            bool i3o = true;
            string i5o = "Hi!";

            // Act
            SerializedArray obj = new SerializedArray()
            {
                0,
                i1o,
                2,
                i3o,
                4,
                i5o,
                6,
                7
            };
            TrackedSerializedData tObj = new TrackedSerializedData( obj );
            List<TrackedSerializedData> list = new SerializedDataPath( SerializedDataPathSegment.IndexedRange( 2, 6, 2 ) ).Evaluate( tObj ).ToList();

            // Assert
            Assert.That( list.Count, Is.EqualTo( 2 ) );
            Assert.That( list[0].value, Is.EqualTo( (SerializedPrimitive)2 ) );
            Assert.That( list[1].value, Is.EqualTo( (SerializedPrimitive)4 ) );
        }

        [Test]
        public void IndexedRangeOnArray_OpenBounds()
        {
            // Arrange
            int i1o = 1;
            bool i3o = true;
            string i5o = "Hi!";

            // Act
            SerializedArray obj = new SerializedArray()
            {
                0,
                i1o,
                2,
                i3o,
                4,
                i5o,
                6,
                7
            };
            TrackedSerializedData tObj = new TrackedSerializedData( obj );
            List<TrackedSerializedData> list = new SerializedDataPath( SerializedDataPathSegment.IndexedRange( 3, int.MaxValue ) ).Evaluate( tObj ).ToList();

            // Assert
            Assert.That( list.Count, Is.EqualTo( 5 ) );
            Assert.That( list[0].value, Is.EqualTo( (SerializedPrimitive)i3o ) );
            Assert.That( list[1].value, Is.EqualTo( (SerializedPrimitive)4 ) );
            Assert.That( list[2].value, Is.EqualTo( (SerializedPrimitive)i5o ) );
            Assert.That( list[3].value, Is.EqualTo( (SerializedPrimitive)6 ) );
            Assert.That( list[4].value, Is.EqualTo( (SerializedPrimitive)7 ) );
        }

        [Test]
        public void IndexedRangeOnArray_OpenLowerBound()
        {
            // Arrange
            int i1o = 1;
            bool i3o = true;
            string i5o = "Hi!";

            // Act
            SerializedArray obj = new SerializedArray()
            {
                0,
                i1o,
                2,
                i3o,
                4,
                i5o,
                6,
                7
            };
            TrackedSerializedData tObj = new TrackedSerializedData( obj );
            List<TrackedSerializedData> list = new SerializedDataPath( SerializedDataPathSegment.IndexedRange( 0, 5 ) ).Evaluate( tObj ).ToList();

            // Assert
            Assert.That( list.Count, Is.EqualTo( 5 ) );
            Assert.That( list[0].value, Is.EqualTo( (SerializedPrimitive)0 ) );
            Assert.That( list[1].value, Is.EqualTo( (SerializedPrimitive)i1o ) );
            Assert.That( list[2].value, Is.EqualTo( (SerializedPrimitive)2 ) );
            Assert.That( list[3].value, Is.EqualTo( (SerializedPrimitive)i3o ) );
            Assert.That( list[4].value, Is.EqualTo( (SerializedPrimitive)4 ) );
        }

        [Test]
        public void NamedOnObject()
        {
            // Arrange
            int i1o = 1;
            bool i3o = true;
            string i5o = "Hi!";

            // Act
            SerializedObject obj = new SerializedObject()
            {
                { "first", i1o },
                { "second", 2 },
                { "third", i3o },
                { "fourth", 4 },
                { "fifth", i5o }
            };
            TrackedSerializedData tObj = new TrackedSerializedData( obj );
            List<TrackedSerializedData> list = new SerializedDataPath( SerializedDataPathSegment.Named( "third" ) ).Evaluate( tObj ).ToList();

            // Assert
            Assert.That( list.Count, Is.EqualTo( 1 ) );
            Assert.That( list[0].value, Is.EqualTo( (SerializedPrimitive)i3o ) );
        }
    }
}