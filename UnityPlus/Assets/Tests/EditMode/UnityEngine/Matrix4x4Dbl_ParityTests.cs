using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngineTests
{
    [TestFixture]
    public class Matrix4x4Dbl_ParityTests
    {
        // Tolerance for comparing float vs double results (absolute)
        const double Tolerance = 1e-6;

        // --- Helpers ----------------------------------------------------------------

        Matrix4x4Dbl ToDbl( Matrix4x4 f )
        {
            return new Matrix4x4Dbl(
                f.m00, f.m01, f.m02, f.m03,
                f.m10, f.m11, f.m12, f.m13,
                f.m20, f.m21, f.m22, f.m23,
                f.m30, f.m31, f.m32, f.m33
            );
        }

        Matrix4x4 ToFloat( Matrix4x4Dbl d )
        {
            Matrix4x4 f = new Matrix4x4();
            f.m00 = (float)d.m00; f.m01 = (float)d.m01; f.m02 = (float)d.m02; f.m03 = (float)d.m03;
            f.m10 = (float)d.m10; f.m11 = (float)d.m11; f.m12 = (float)d.m12; f.m13 = (float)d.m13;
            f.m20 = (float)d.m20; f.m21 = (float)d.m21; f.m22 = (float)d.m22; f.m23 = (float)d.m23;
            f.m30 = (float)d.m30; f.m31 = (float)d.m31; f.m32 = (float)d.m32; f.m33 = (float)d.m33;
            return f;
        }

        // Assuming these small constructors exist in your project; if not, replace with your types' constructors.
        Vector3Dbl ToVector3Dbl( Vector3 v ) => new Vector3Dbl( v.x, v.y, v.z );
        QuaternionDbl ToQuaternionDbl( Quaternion q ) => new QuaternionDbl( q.x, q.y, q.z, q.w );

        void AssertMatricesApproximatelyEqual( Matrix4x4 expectedFloat, Matrix4x4Dbl actualDbl, double tol = Tolerance )
        {
            // Compare element-wise
            Assert.That( Math.Abs( actualDbl.m00 - expectedFloat.m00 ), Is.LessThanOrEqualTo( tol ), "m00" );
            Assert.That( Math.Abs( actualDbl.m01 - expectedFloat.m01 ), Is.LessThanOrEqualTo( tol ), "m01" );
            Assert.That( Math.Abs( actualDbl.m02 - expectedFloat.m02 ), Is.LessThanOrEqualTo( tol ), "m02" );
            Assert.That( Math.Abs( actualDbl.m03 - expectedFloat.m03 ), Is.LessThanOrEqualTo( tol ), "m03" );

            Assert.That( Math.Abs( actualDbl.m10 - expectedFloat.m10 ), Is.LessThanOrEqualTo( tol ), "m10" );
            Assert.That( Math.Abs( actualDbl.m11 - expectedFloat.m11 ), Is.LessThanOrEqualTo( tol ), "m11" );
            Assert.That( Math.Abs( actualDbl.m12 - expectedFloat.m12 ), Is.LessThanOrEqualTo( tol ), "m12" );
            Assert.That( Math.Abs( actualDbl.m13 - expectedFloat.m13 ), Is.LessThanOrEqualTo( tol ), "m13" );

            Assert.That( Math.Abs( actualDbl.m20 - expectedFloat.m20 ), Is.LessThanOrEqualTo( tol ), "m20" );
            Assert.That( Math.Abs( actualDbl.m21 - expectedFloat.m21 ), Is.LessThanOrEqualTo( tol ), "m21" );
            Assert.That( Math.Abs( actualDbl.m22 - expectedFloat.m22 ), Is.LessThanOrEqualTo( tol ), "m22" );
            Assert.That( Math.Abs( actualDbl.m23 - expectedFloat.m23 ), Is.LessThanOrEqualTo( tol ), "m23" );

            Assert.That( Math.Abs( actualDbl.m30 - expectedFloat.m30 ), Is.LessThanOrEqualTo( tol ), "m30" );
            Assert.That( Math.Abs( actualDbl.m31 - expectedFloat.m31 ), Is.LessThanOrEqualTo( tol ), "m31" );
            Assert.That( Math.Abs( actualDbl.m32 - expectedFloat.m32 ), Is.LessThanOrEqualTo( tol ), "m32" );
            Assert.That( Math.Abs( actualDbl.m33 - expectedFloat.m33 ), Is.LessThanOrEqualTo( tol ), "m33" );
        }

        void AssertVector3ApproximatelyEqual( Vector3 expectedF, Vector3Dbl actualD, double tol = Tolerance )
        {
            Assert.That( Math.Abs( actualD.x - expectedF.x ), Is.LessThanOrEqualTo( tol ) );
            Assert.That( Math.Abs( actualD.y - expectedF.y ), Is.LessThanOrEqualTo( tol ) );
            Assert.That( Math.Abs( actualD.z - expectedF.z ), Is.LessThanOrEqualTo( tol ) );
        }

        void AssertQuaternionApproximatelyEqual( Quaternion expectedF, QuaternionDbl actualD, double tol = 1e-5 )
        {
            // allow a slightly larger tol for quaternion sign ambiguity and float->double differences
            Assert.That( Math.Abs( actualD.x - expectedF.x ), Is.LessThanOrEqualTo( tol ) );
            Assert.That( Math.Abs( actualD.y - expectedF.y ), Is.LessThanOrEqualTo( tol ) );
            Assert.That( Math.Abs( actualD.z - expectedF.z ), Is.LessThanOrEqualTo( tol ) );
            Assert.That( Math.Abs( actualD.w - expectedF.w ), Is.LessThanOrEqualTo( tol ) );
        }

        // Provide some representative matrices to test
        IEnumerable<Matrix4x4> SampleMatrices()
        {
            // 1) Identity
            yield return Matrix4x4.identity;

            // 2) Typical TRS
            yield return Matrix4x4.TRS(
                new Vector3( 1.2345f, -2.3456f, 3.4567f ),
                Quaternion.Euler( 23.5f, -45.3f, 89.0f ),
                new Vector3( 2.0f, 0.5f, -1.5f )
            );

            // 3) Small perspective / non-affine last row
            {
                var m = Matrix4x4.TRS( Vector3.zero, Quaternion.identity, Vector3.one );
                m.m30 = 0.002f; m.m31 = -0.001f; m.m32 = 0.0005f; m.m33 = 1.0f;
                yield return m;
            }

            // 4) Seeded random matrices (couple of samples)
            System.Random rnd = new System.Random( 123456 );
            for( int k = 0; k < 3; ++k )
            {
                Matrix4x4 m = new Matrix4x4();
                m.m00 = (float)(rnd.NextDouble() * 10 - 5); m.m01 = (float)(rnd.NextDouble() * 10 - 5); m.m02 = (float)(rnd.NextDouble() * 10 - 5); m.m03 = (float)(rnd.NextDouble() * 10 - 5);
                m.m10 = (float)(rnd.NextDouble() * 10 - 5); m.m11 = (float)(rnd.NextDouble() * 10 - 5); m.m12 = (float)(rnd.NextDouble() * 10 - 5); m.m13 = (float)(rnd.NextDouble() * 10 - 5);
                m.m20 = (float)(rnd.NextDouble() * 10 - 5); m.m21 = (float)(rnd.NextDouble() * 10 - 5); m.m22 = (float)(rnd.NextDouble() * 10 - 5); m.m23 = (float)(rnd.NextDouble() * 10 - 5);
                m.m30 = (float)(rnd.NextDouble() * 10 - 5); m.m31 = (float)(rnd.NextDouble() * 10 - 5); m.m32 = (float)(rnd.NextDouble() * 10 - 5); m.m33 = (float)(rnd.NextDouble() * 10 - 5);
                yield return m;
            }
        }

        // --- Tests ------------------------------------------------------------------

        [Test]
        public void Transpose__MatchesUnity()
        {
            foreach( var f in SampleMatrices() )
            {
                var d = ToDbl( f );

                Matrix4x4 expected = f.transpose;
                Matrix4x4Dbl actual = Matrix4x4Dbl.Transpose( d );

                AssertMatricesApproximatelyEqual( expected, actual );
            }
        }

        [Test]
        public void Determinant__MatchesUnity()
        {
            foreach( var f in SampleMatrices() )
            {
                var d = ToDbl( f );

                float expected = f.determinant;
                double actual = Matrix4x4Dbl.Determinant( d );

                Assert.That( Math.Abs( actual - expected ), Is.LessThanOrEqualTo( Tolerance ) );
            }
        }

        [Test]
        public void Inverse__MatchesUnity_ForInvertibleMatrices()
        {
            foreach( var f in SampleMatrices() )
            {
                // only test matrices that are reasonably invertible (abs(det) > small threshold)
                float detF = f.determinant;
                if( Math.Abs( detF ) < 1e-6f ) continue; // skip near-singular cases

                var d = ToDbl( f );

                Matrix4x4 expected = f.inverse; // Unity's inverse (float)
                Matrix4x4Dbl actualD = Matrix4x4Dbl.Inverse( d );

                AssertMatricesApproximatelyEqual( expected, actualD, 1e-5 );
            }
        }

        [Test]
        public void Multiply_MatrixMultiplication__MatchesUnity()
        {
            List<Matrix4x4> mats = new List<Matrix4x4>( SampleMatrices() );
            for( int i = 0; i < mats.Count; ++i )
                for( int j = 0; j < mats.Count; ++j )
                {
                    Matrix4x4 aF = mats[i];
                    Matrix4x4 bF = mats[j];
                    Matrix4x4 prodF = aF * bF;

                    Matrix4x4Dbl aD = ToDbl( aF );
                    Matrix4x4Dbl bD = ToDbl( bF );
                    Matrix4x4Dbl prodD = aD * bD;

                    AssertMatricesApproximatelyEqual( prodF, prodD, 1e-5 );
                }
        }

        [Test]
        public void MultiplyPoint3x4__MatchesUnity()
        {
            var testVectors = new[] {
                new Vector3(0f,0f,0f),
                new Vector3(1.234f, -2.345f, 3.456f),
                new Vector3(-10f, 5.5f, 0.25f)
            };

            foreach( var f in SampleMatrices() )
            {
                var d = ToDbl( f );

                foreach( var v in testVectors )
                {
                    Vector3 expected = f.MultiplyPoint3x4( v );

                    // Convert input vector to Vector3Dbl and call the double implementation
                    Vector3Dbl vD = ToVector3Dbl( v );
                    Vector3Dbl outD = d.MultiplyPoint3x4( vD );

                    AssertVector3ApproximatelyEqual( expected, outD, 1e-4 );
                }
            }
        }

        [Test]
        public void MultiplyPoint__MatchesUnity_IncludingHomogeneousDivide()
        {
            var testVectors = new[] {
                new Vector3(0.5f, -0.2f, 2.0f),
                new Vector3(3.3f, 1.1f, -4.4f)
            };

            foreach( var f in SampleMatrices() )
            {
                var d = ToDbl( f );

                foreach( var v in testVectors )
                {
                    Vector3 expected = f.MultiplyPoint( v );

                    Vector3Dbl outD = d.MultiplyPoint( ToVector3Dbl( v ) );

                    AssertVector3ApproximatelyEqual( expected, outD, 1e-4 );
                }
            }
        }

        [Test]
        public void MultiplyVector__MatchesUnity()
        {
            var testVectors = new[] {
                new Vector3(1f,0f,0f),
                new Vector3(0f,1f,0f),
                new Vector3(0.3f,-0.6f,2.2f)
            };

            foreach( var f in SampleMatrices() )
            {
                var d = ToDbl( f );

                foreach( var v in testVectors )
                {
                    Vector3 expected = f.MultiplyVector( v );
                    Vector3Dbl outD = d.MultiplyVector( ToVector3Dbl( v ) );
                    AssertVector3ApproximatelyEqual( expected, outD, 1e-6 );
                }
            }
        }

        [Test]
        public void TRS__MatchesUnity()
        {
            var translations = new[] { new Vector3( 1.2f, -2.3f, 4.5f ), Vector3.zero };
            var rotations = new[] { Quaternion.Euler( 12f, 34f, -56f ), Quaternion.identity };
            var scales = new[] { new Vector3( 2f, 0.5f, -1.0f ), Vector3.one };

            foreach( var t in translations )
                foreach( var r in rotations )
                    foreach( var s in scales )
                    {
                        Matrix4x4 f = Matrix4x4.TRS( t, r, s );
                        Matrix4x4Dbl d = Matrix4x4Dbl.TRS( ToVector3Dbl( t ), ToQuaternionDbl( r ), ToVector3Dbl( s ) );

                        AssertMatricesApproximatelyEqual( f, d, 1e-6 );
                    }
        }

        [Test]
        public void Rotate_Scale_Translate_Individual_Constructors__MatchUnity()
        {
            // Rotate
            Quaternion q = Quaternion.Euler( 7.7f, -33.3f, 123.0f );
            Matrix4x4 expectedRotate = Matrix4x4.Rotate( q );
            Matrix4x4Dbl actualRotate = Matrix4x4Dbl.Rotate( ToQuaternionDbl( q ) );
            AssertMatricesApproximatelyEqual( expectedRotate, actualRotate );

            // Scale
            Vector3 s = new Vector3( 2.0f, 3.0f, -1.5f );
            Matrix4x4 expectedScale = Matrix4x4.Scale( s );
            Matrix4x4Dbl actualScale = Matrix4x4Dbl.Scale( ToVector3Dbl( s ) );
            AssertMatricesApproximatelyEqual( expectedScale, actualScale );

            // Translate
            Vector3 t = new Vector3( -5.5f, 2.2f, 0.0f );
            Matrix4x4 expectedTranslate = Matrix4x4.Translate( t );
            Matrix4x4Dbl actualTranslate = Matrix4x4Dbl.Translate( ToVector3Dbl( t ) );
            AssertMatricesApproximatelyEqual( expectedTranslate, actualTranslate );
        }

        [Test]
        public void LossyScale__MatchesUnity()
        {
            int count = 0;
            foreach( var f in SampleMatrices() )
            {
                if( !f.ValidTRS() )
                    continue;
                count++;
                var d = ToDbl( f );
                Vector3 expected = f.lossyScale;
                Vector3Dbl actual = d.lossyScale;
                AssertVector3ApproximatelyEqual( expected, actual );
            }
            Assert.That( count, Is.GreaterThan( 0 ) );
        }

        [Test]
        public void RotationExtraction__MatchesUnityQuaternion()
        {
            // test with matrices that represent pure rotation (to avoid scale ambiguities)
            var rotations = new[] { Quaternion.Euler( 10, 20, 30 ), Quaternion.Euler( -45, 120, 5 ), Quaternion.identity };
            foreach( var q in rotations )
            {
                Matrix4x4 f = Matrix4x4.Rotate( q );
                Matrix4x4Dbl d = ToDbl( f );

                Quaternion expected = f.rotation;
                QuaternionDbl actual = d.rotation;

                // Unity quaternion components are floats; compare to doubles with a little looser tolerance
                AssertQuaternionApproximatelyEqual( expected, actual, 1e-5 );
            }
        }

        [Test]
        public void GetSetColumn__Roundtrip()
        {
            foreach( var f in SampleMatrices() )
            {
                var d = ToDbl( f );

                // Columns
                for( int i = 0; i < 4; ++i )
                {
                    var colF = f.GetColumn( i );
                    var colD = d.GetColumn( i );

                    // Column in Matrix4x4Dbl is Vector4Dbl; compare element-wise
                    Assert.That( Math.Abs( colD.x - colF.x ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( colD.y - colF.y ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( colD.z - colF.z ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( colD.w - colF.w ), Is.LessThanOrEqualTo( Tolerance ) );

                    // Now set column to something new and get it back
                    var newCol = new Vector4( colF.x + 0.1f, colF.y - 0.2f, colF.z + 0.3f, colF.w - 0.4f );
                    d.SetColumn( i, new Vector4Dbl( newCol.x, newCol.y, newCol.z, newCol.w ) );
                    var fetched = d.GetColumn( i );
                    Assert.That( Math.Abs( fetched.x - newCol.x ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( fetched.y - newCol.y ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( fetched.z - newCol.z ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( fetched.w - newCol.w ), Is.LessThanOrEqualTo( Tolerance ) );
                }
            }
        }
        [Test]
        public void GetSetRow__Roundtrip()
        {
            foreach( var f in SampleMatrices() )
            {
                var d = ToDbl( f );

                // Rows
                for( int i = 0; i < 4; ++i )
                {
                    var rowF = f.GetRow( i );
                    var rowD = d.GetRow( i );

                    Assert.That( Math.Abs( rowD.x - rowF.x ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( rowD.y - rowF.y ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( rowD.z - rowF.z ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( rowD.w - rowF.w ), Is.LessThanOrEqualTo( Tolerance ) );

                    var newRow = new Vector4( rowF.x * 1.1f, rowF.y * 0.9f, rowF.z * -1.0f, rowF.w + 0.7f );
                    d.SetRow( i, new Vector4Dbl( newRow.x, newRow.y, newRow.z, newRow.w ) );
                    var fetched = d.GetRow( i );
                    Assert.That( Math.Abs( fetched.x - newRow.x ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( fetched.y - newRow.y ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( fetched.z - newRow.z ), Is.LessThanOrEqualTo( Tolerance ) );
                    Assert.That( Math.Abs( fetched.w - newRow.w ), Is.LessThanOrEqualTo( Tolerance ) );
                }
            }
        }

        [Test]
        public void EqualsAndHashCode__RoundtripAndEquality()
        {
            foreach( var f in SampleMatrices() )
            {
                var d = ToDbl( f );
                // Converting to float and back should preserve values within tolerance
                Matrix4x4 backToFloat = ToFloat( d );

                // elementwise compare
                Assert.That( Math.Abs( backToFloat.m00 - f.m00 ), Is.LessThanOrEqualTo( Tolerance ) );
                Assert.That( Math.Abs( backToFloat.m11 - f.m11 ), Is.LessThanOrEqualTo( Tolerance ) );
                Assert.That( Math.Abs( backToFloat.m22 - f.m22 ), Is.LessThanOrEqualTo( Tolerance ) );

                // equality operator
                Matrix4x4Dbl copy = new Matrix4x4Dbl(
                    d.m00, d.m01, d.m02, d.m03,
                    d.m10, d.m11, d.m12, d.m13,
                    d.m20, d.m21, d.m22, d.m23,
                    d.m30, d.m31, d.m32, d.m33
                );
                Assert.IsTrue( d == copy );
                Assert.IsFalse( d != copy );
                Assert.AreEqual( d.GetHashCode(), copy.GetHashCode() );
            }
        }
    }
}
