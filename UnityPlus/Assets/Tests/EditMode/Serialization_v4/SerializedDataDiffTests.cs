using System.Collections.Generic;
using NUnit.Framework;
using UnityPlus.Serialization;

namespace Neoserialization.V4
{
    public class SerializedDataDiffTests
    {
        [Test]
        public void Diff_IdenticalPrimitives_ReturnsEmpty()
        {
            SerializedData a = 123;
            SerializedData b = 123;

            var diffs = SerializedDataDiff.Diff( a, b );

            Assert.That( diffs, Is.Empty );
        }

        [Test]
        public void Diff_DifferentPrimitives_ReturnsValueMismatch()
        {
            SerializedData a = 123;
            SerializedData b = 456;

            var diffs = SerializedDataDiff.Diff( a, b );

            Assert.That( diffs.Count, Is.EqualTo( 1 ) );
            Assert.That( diffs[0].Kind, Is.EqualTo( SerializedDataDifferenceKind.ValueMismatch ) );
            Assert.That( diffs[0].Path.ToString(), Is.EqualTo( "" ) );
        }

        [Test]
        public void Diff_ObjectsWithDifferentValues_ReturnsValueMismatchAtPath()
        {
            var a = new SerializedObject { ["health"] = 100 };
            var b = new SerializedObject { ["health"] = 80 };

            var diffs = SerializedDataDiff.Diff( a, b );

            Assert.That( diffs.Count, Is.EqualTo( 1 ) );
            Assert.That( diffs[0].Kind, Is.EqualTo( SerializedDataDifferenceKind.ValueMismatch ) );
            Assert.That( diffs[0].Path.ToString(), Is.EqualTo( "health" ) );
        }

        [Test]
        public void Diff_ObjectsWithMissingKeys_ReturnsMissing()
        {
            var a = new SerializedObject { ["health"] = 100, ["mana"] = 50 };
            var b = new SerializedObject { ["health"] = 100 };

            var diffs = SerializedDataDiff.Diff( a, b );

            Assert.That( diffs.Count, Is.EqualTo( 1 ) );
            Assert.That( diffs[0].Kind, Is.EqualTo( SerializedDataDifferenceKind.MissingInB ) );
            Assert.That( diffs[0].Path.ToString(), Is.EqualTo( "mana" ) );
        }

        [Test]
        public void Diff_ArraysWithDifferentElements_ReturnsValueMismatchAtIndex()
        {
            var a = new SerializedArray { 1, 2, 3 };
            var b = new SerializedArray { 1, 99, 3 };

            var diffs = SerializedDataDiff.Diff( a, b );

            Assert.That( diffs.Count, Is.EqualTo( 1 ) );
            Assert.That( diffs[0].Kind, Is.EqualTo( SerializedDataDifferenceKind.ValueMismatch ) );
            Assert.That( diffs[0].Path.ToString(), Is.EqualTo( "[1]" ) );
        }

        [Test]
        public void Diff_IgnoreKeys_SkipsSpecifiedKeys()
        {
            var a = new SerializedObject { ["health"] = 100, ["$type"] = "Player" };
            var b = new SerializedObject { ["health"] = 100, ["$type"] = "Enemy" };

            var options = new SerializedDataDiffConfig();
            options.IgnoreKeys.Add( "$type" );

            var diffs = SerializedDataDiff.Diff( a, b, options );

            Assert.That( diffs, Is.Empty );
        }

        [Test]
        public void Diff_IgnorePaths_SkipsSpecifiedPaths()
        {
            var a = new SerializedObject { ["pos"] = new SerializedObject { ["x"] = 1, ["y"] = 2 } };
            var b = new SerializedObject { ["pos"] = new SerializedObject { ["x"] = 1, ["y"] = 99 } };

            var options = new SerializedDataDiffConfig();
            options.IgnorePaths.Add( SerializedDataPath.Parse( "pos.y" ) );

            var diffs = SerializedDataDiff.Diff( a, b, options );

            Assert.That( diffs, Is.Empty );
        }

        [Test]
        public void Diff_CustomComparer_OverridesLogic()
        {
            var a = new SerializedObject { ["version"] = "1.0.0" };
            var b = new SerializedObject { ["version"] = "1.0.1" };

            var options = new SerializedDataDiffConfig();
            options.CustomComparer = ( path, valA, valB ) =>
            {
                if( path.ToString() == "version" )
                    return true; // Treat all versions as equal
                return null;
            };

            var diffs = SerializedDataDiff.Diff( a, b, options );

            Assert.That( diffs, Is.Empty );
        }

        [Test]
        public void Diff_CircularReferences_DoesNotStackOverflow()
        {
            var a = new SerializedObject();
            a["self"] = a;

            var b = new SerializedObject();
            b["self"] = b;

            var diffs = SerializedDataDiff.Diff( a, b );

            Assert.That( diffs, Is.Empty );
        }
    }
}
