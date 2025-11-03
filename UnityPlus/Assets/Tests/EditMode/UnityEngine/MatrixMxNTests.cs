using System;
using NUnit.Framework;
using UnityEngine;

namespace UnityEngineTests
{
    [TestFixture]
    public class MatrixMxNTests
    {
        const float EPS = 1e-5f;

        // helper to build matrix from 2D array
        private MatrixMxN FromArray( float[,] arr )
        {
            int rows = arr.GetLength( 0 );
            int cols = arr.GetLength( 1 );
            MatrixMxN m = new MatrixMxN( rows, cols );
            for( int i = 0; i < rows; i++ )
                for( int j = 0; j < cols; j++ )
                    m[i, j] = arr[i, j];
            return m;
        }

        private void AssertMatrixEquals( MatrixMxN expected, MatrixMxN actual, float tol = EPS )
        {
            Assert.AreEqual( expected.Rows, actual.Rows, "Rows differ" );
            Assert.AreEqual( expected.Cols, actual.Cols, "Cols differ" );
            for( int i = 0; i < expected.Rows; i++ )
                for( int j = 0; j < expected.Cols; j++ )
                    Assert.AreEqual( expected[i, j], actual[i, j], tol, $"Element [{i},{j}] differs" );
        }

        [Test]
        public void Constructor_And_Indexer_Basic()
        {
            var m = new MatrixMxN( 2, 3 );
            Assert.AreEqual( 2, m.Rows );
            Assert.AreEqual( 3, m.Cols );
            m[0, 0] = 1.5f;
            m[1, 2] = -2f;
            Assert.AreEqual( 1.5f, m[0, 0], EPS );
            Assert.AreEqual( -2f, m[1, 2], EPS );
        }

        [Test]
        public void Zero_Creates_ZeroMatrix()
        {
            var z = MatrixMxN.Zero( 2, 2 );
            Assert.AreEqual( 2, z.Rows );
            Assert.AreEqual( 2, z.Cols );
            for( int i = 0; i < 2; i++ )
                for( int j = 0; j < 2; j++ )
                    Assert.AreEqual( 0f, z[i, j], EPS );
        }

        [Test]
        public void Identity_Is_Diagonal_Ones()
        {
            var id = MatrixMxN.Identity( 3 );

            // Identity should have 1 on the diagonal and 0 elsewhere.
            for( int r = 0; r < 3; r++ )
                for( int c = 0; c < 3; c++ )
                    Assert.AreEqual( r == c ? 1f : 0f, id[r, c], EPS, $"id[{r},{c}]" );
        }

        [Test]
        public void Add_Subtract_ScalarMultiply()
        {
            var a = FromArray( new float[,] { { 1, 2 }, { 3, 4 } } );
            var b = FromArray( new float[,] { { -1, 1 }, { 0, 2 } } );

            var sum = MatrixMxN.Add( a, b );
            var expectedSum = FromArray( new float[,] { { 0, 3 }, { 3, 6 } } );
            AssertMatrixEquals( expectedSum, sum );

            var diff = MatrixMxN.Subtract( a, b );
            var expectedDiff = FromArray( new float[,] { { 2, 1 }, { 3, 2 } } );
            AssertMatrixEquals( expectedDiff, diff );

            var scaled = MatrixMxN.Multiply( a, 2f );
            var expectedScaled = FromArray( new float[,] { { 2, 4 }, { 6, 8 } } );
            AssertMatrixEquals( expectedScaled, scaled );

            var divided = MatrixMxN.Divide( a, 2f );
            var expectedDiv = FromArray( new float[,] { { 0.5f, 1f }, { 1.5f, 2f } } );
            AssertMatrixEquals( expectedDiv, divided );
        }

        [Test]
        public void MatrixMultiply_2x2()
        {
            var a = FromArray( new float[,] { { 1, 2 }, { 3, 4 } } );
            var b = FromArray( new float[,] { { 2, 0 }, { 1, 2 } } );

            var prod = MatrixMxN.Multiply( a, b );
            var expected = FromArray( new float[,] { { 4, 4 }, { 10, 8 } } );
            AssertMatrixEquals( expected, prod );
        }

        [Test]
        public void Determinant_2x2_and_3x3()
        {
            var a = FromArray( new float[,] { { 1, 2 }, { 3, 4 } } );
            float detA = MatrixMxN.Determinant( a );
            Assert.AreEqual( 1f * 4f - 2f * 3f, detA, 1e-4f );

            var b = FromArray( new float[,] { { 6, 1, 1 }, { 4, -2, 5 }, { 2, 8, 7 } } );
            // Known determinant: 6 * (-2*7 - 5*8) - 1 * (4*7 - 5*2) + 1 * (4*8 - (-2)*2)
            float expectedB = 6 * (-14 - 40) - 1 * (28 - 10) + 1 * (32 + 4); // compute explicitly
            float detB = MatrixMxN.Determinant( b );
            Assert.AreEqual( expectedB, detB, 1e-3f );
        }

        [Test]
        public void Solve_Simple_2x2()
        {
            // Solve [2 1; 1 3] * x = [1; 2]
            var A = FromArray( new float[,] { { 2, 1 }, { 1, 3 } } );
            var b = MatrixMxN.ColumnVector( 2 );
            b[0, 0] = 1f;
            b[1, 0] = 2f;

            var x = MatrixMxN.Solve( A, b );

            // Direct solve: inverse computed or algebra -> x = [ (1*3 -1*2)/(2*3-1*1) etc... ] but easier to check A*x == b
            var Ax = MatrixMxN.Multiply( A, x );
            AssertMatrixEquals( b, Ax, 1e-4f );
        }

        [Test]
        public void Inverse_2x2()
        {
            var A = FromArray( new float[,] { { 4, 7 }, { 2, 6 } } );
            var invA = MatrixMxN.Inverse( A );

            // Known inverse: (1/det) * [6 -7; -2 4], det=4*6 - 7*2 = 10
            var expected = FromArray( new float[,] { { 6f / 10f, -7f / 10f }, { -2f / 10f, 4f / 10f } } );
            AssertMatrixEquals( expected, invA, 1e-4f );

            // additionally check A * invA == Identity
            var prod = MatrixMxN.Multiply( A, invA );
            var id = MatrixMxN.Identity( 2 );
            AssertMatrixEquals( id, prod, 1e-4f );
        }

        [Test]
        public void Determinant_SingularMatrix_ReturnsZero()
        {
            var singular = FromArray( new float[,] { { 1, 2 }, { 2, 4 } } );
            float det = MatrixMxN.Determinant( singular );
            Assert.AreEqual( 0f, det, 1e-6f );
        }

        [Test]
        public void Solve_Singular_Throws()
        {
            var A = FromArray( new float[,] { { 1, 2 }, { 2, 4 } } ); // singular
            var b = MatrixMxN.ColumnVector( 2 );
            b[0, 0] = 1f; b[1, 0] = 2f;
            Assert.Throws<InvalidOperationException>( () => MatrixMxN.Solve( A, b ) );
        }

        [Test]
        public void Identity_Diagonal()
        {
            var id = MatrixMxN.Identity( 4 );

            for( int r = 0; r < 4; r++ )
                for( int c = 0; c < 4; c++ )
                    Assert.AreEqual( r == c ? 1f : 0f, id[r, c], 1e-5f, $"Identity entry [{r},{c}]" );
        }
    }
}