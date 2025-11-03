using NUnit.Framework;
using UnityEngine;

namespace UnityEngineTests
{
    [TestFixture]
    public class Matrix3x3Tests
    {
        [Test]
        public void IdentityMatrix_SolvesExactly()
        {
            Matrix3x3 I = Matrix3x3.identity;
            Vector3 b = new Vector3( 1.234f, -2.0f, 3.5f );

            Vector3 x = I.Solve( b );
            Assert.AreEqual( b.x, x.x, 1e-6f );
            Assert.AreEqual( b.y, x.y, 1e-6f );
            Assert.AreEqual( b.z, x.z, 1e-6f );
        }

        [Test]
        public void DiagonalMatrix_ScalesCorrectly()
        {
            Matrix3x3 D = Matrix3x3.Scale( new Vector3( 2.0f, 3.0f, -4.0f ) ); // diag(2,3,-4)
            Vector3 b = new Vector3( 2f, -1f, 0.5f );

            Vector3 x = D.Solve( b ); // D * x = b  -> x = D^{-1} * b = (b / diag)
            Assert.AreEqual( b.x / 2.0f, x.x, 1e-6f );
            Assert.AreEqual( b.y / 3.0f, x.y, 1e-6f );
            Assert.AreEqual( b.z / -4.0f, x.z, 1e-6f );
        }

        [Test]
        public void KnownMatrix_SolvesToExpected_SingleRHS()
        {
            // A = [ [3,2,1], [1,0,2], [4,1,3] ], b = [1,2,3]
            // Solve by hand or via a reliable solver gives x = [0, 0, 1]
            Matrix3x3 A = new Matrix3x3(
                3f, 2f, 1f,
                1f, 0f, 2f,
                4f, 1f, 3f
            );
            Vector3 b = new Vector3( 1f, 2f, 3f );
            Assert.IsTrue( A.TrySolve( b, out Vector3 x ) );
            Assert.AreEqual( 0f, x.x, 1e-6f );
            Assert.AreEqual( 0f, x.y, 1e-6f );
            Assert.AreEqual( 1f, x.z, 1e-6f );
        }

        [Test]
        public void MultipleRHS_SolvesCorrectly()
        {
            // Use A = [[2,1,0],[0,1,0],[1,0,1]]
            // b1 = [1,2,3] -> x1 = [-0.5, 2.0, 3.5]
            // b2 = [3,2,1] -> x2 = [0.5, 2.0, 0.5]
            Matrix3x3 A = new Matrix3x3(
                2f, 1f, 0f,
                0f, 1f, 0f,
                1f, 0f, 1f
            );

            Vector3[] B = new[]
            {
            new Vector3(1f, 2f, 3f),
            new Vector3(3f, 2f, 1f)
        };
            // Solve into an output array
            Assert.IsTrue( A.TrySolve( B, out Vector3[] X ) );
            Assert.AreEqual( -0.5f, X[0].x, 1e-6f );  // x1.x
            Assert.AreEqual( 2.0f, X[0].y, 1e-6f );  // x1.y
            Assert.AreEqual( 3.5f, X[0].z, 1e-6f );  // x1.z

            Assert.AreEqual( 0.5f, X[1].x, 1e-6f );  // x2.x
            Assert.AreEqual( 2.0f, X[1].y, 1e-6f );  // x2.y
            Assert.AreEqual( 0.5f, X[1].z, 1e-6f );  // x2.z
        }

        [Test]
        public void SingularMatrix_TrySolveReturnsFalse()
        {
            // Two identical rows -> singular
            Matrix3x3 S = new Matrix3x3(
                1f, 2f, 3f,
                1f, 2f, 3f, // same as row 0
                0f, 1f, 1f
            );
            Vector3 b = new Vector3( 1f, 2f, 3f );
            Assert.IsFalse( S.TrySolve( b, out Vector3 x ) );
            Assert.AreEqual( Vector3.zero, x );
        }
    }
}