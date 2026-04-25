using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.LowLevel;
using UnityPlus.PlayerLoop;
using UnityPlus.PlayerLoop.Phases;

namespace UnityPlus.Tests
{
    public class PlayerLoopCompilerTests
    {
        // Dummy native phases
        [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.Update ), Before = new[] { typeof( NativePhase2 ) } )]
        private struct NativePhase1 { }
        [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.Update ), Before = new[] { typeof( NativePhase3 ) } )]
        private struct NativePhase2 { }
        [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.Update ) )]
        private struct NativePhase3 { }

        // Custom systems
        [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.Initialization ), Before = new[] { typeof( SysB ) } )]
        private struct SysA { }

        [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.Initialization ) )]
        private struct SysB { }

        [PlayerLoopNative( typeof( UnityEngine.PlayerLoop.Initialization ), After = new[] { typeof( SysB ) } )]
        private struct SysC { }

        [PlayerLoopSystem( typeof( Update ) )]
        private struct SysD { }

        [PlayerLoopSystem( typeof( Update ), After = new[] { typeof( NativePhase1 ) }, Before = new[] { typeof( NativePhase2 ) } )]
        private struct SysE { }


        // Custom group
        [PlayerLoopSystem( typeof( Update ), Before = new[] { typeof( NativePhase2 ) }, After = new[] { typeof( NativePhase1 ) } )]
        private struct CustomGroup { }

        [PlayerLoopSystem( typeof( CustomGroup ) )]
        private struct NestedCustomGroup { }

        [PlayerLoopSystem( typeof( CustomGroup ) )]
        private struct SysF { }

        [PlayerLoopSystem( typeof( NestedCustomGroup ) )]
        private struct SysG { }

        [PlayerLoopSystem( typeof( UnityEngine.PlayerLoop.Update ) )]
        private struct SysInvalid { }

        private PlayerLoopSystem CreateMockNativeLoop()
        {
            var updatePhase = new PlayerLoopSystem
            {
                type = typeof( Update ),
                subSystemList = new[]
                {
                    new PlayerLoopSystem { type = typeof( NativePhase1 ) },
                    new PlayerLoopSystem { type = typeof( NativePhase2 ) },
                    new PlayerLoopSystem { type = typeof( NativePhase3 ) }
                }
            };

            var lareUpdatePhase = new PlayerLoopSystem
            {
                type = typeof( LateUpdate )
            };

            var root = new PlayerLoopSystem
            {
                type = typeof( UnityEngine.PlayerLoop.Initialization ), // acting as root type
                subSystemList = new[] { lareUpdatePhase, updatePhase }
            };

            return root;
        }

        [Test]
        public void Compiler_InjectsNodeIntoTargetBucket()
        {
            var compiler = new PlayerLoopCompiler();
            var nodes = new List<Type> { typeof( SysD ) };

            var root = compiler.Compile( CreateMockNativeLoop(), nodes, BucketHandling.IncludeThrow );

            var updatePhase = root.subSystemList.First( s => s.type == typeof( Update ) );
            Assert.That( updatePhase.subSystemList.Any( s => s.type == typeof( SysD ) ), Is.True, "SysD should be injected into Update bucket." );
        }

        [Test]
        public void Compiler_SortsNodesTopologically()
        {
            var compiler = new PlayerLoopCompiler();

            var nodes = new List<Type> { typeof( SysC ), typeof( SysA ), typeof( SysB ) };

            var root = compiler.Compile( CreateMockNativeLoop(), nodes, BucketHandling.IncludeThrow );

            var list = root.subSystemList.Select( s => s.type ).ToList();

            int idxA = list.IndexOf( typeof( SysA ) );
            int idxB = list.IndexOf( typeof( SysB ) );
            int idxC = list.IndexOf( typeof( SysC ) );

            Assert.That( idxA >= 0 && idxB >= 0 && idxC >= 0, Is.True, "All systems should be present." );
            Assert.That( idxA, Is.LessThan( idxB ), "SysA should be before SysB" );
            Assert.That( idxB, Is.LessThan( idxC ), "SysB should be before SysC" );
        }

        [Test]
        public void Compiler_PreservesNativeImplicitOrder()
        {
            var compiler = new PlayerLoopCompiler();
            var nodes = new List<Type>();

            var root = compiler.Compile( CreateMockNativeLoop(), nodes, BucketHandling.IncludeThrow );
            var updatePhase = root.subSystemList.First( s => s.type == typeof( UnityEngine.PlayerLoop.Update ) );
            var list = updatePhase.subSystemList.Select( s => s.type ).ToList();

            // Native order was 1, 2, 3
            Assert.That( list.IndexOf( typeof( NativePhase1 ) ), Is.LessThan( list.IndexOf( typeof( NativePhase2 ) ) ) );
            Assert.That( list.IndexOf( typeof( NativePhase2 ) ), Is.LessThan( list.IndexOf( typeof( NativePhase3 ) ) ) );
        }

        [Test]
        public void Compiler_InjectsBetweenNativeNodes()
        {
            var compiler = new PlayerLoopCompiler();
            var nodes = new List<Type> { typeof( SysE ) };

            var root = compiler.Compile( CreateMockNativeLoop(), nodes, BucketHandling.IncludeThrow );
            var updatePhase = root.subSystemList.First( s => s.type == typeof( UnityEngine.PlayerLoop.Update ) );
            var list = updatePhase.subSystemList.Select( s => s.type ).ToList();

            int idx1 = list.IndexOf( typeof( NativePhase1 ) );
            int idxA = list.IndexOf( typeof( SysE ) );
            int idx2 = list.IndexOf( typeof( NativePhase2 ) );

            Assert.That( idx1, Is.LessThan( idxA ), "SysE should be after NativePhase1" );
            Assert.That( idxA, Is.LessThan( idx2 ), "SysE should be before NativePhase2" );
        }

        [Test]
        public void Compiler_ScaffoldsMissingGroup()
        {
            var compiler = new PlayerLoopCompiler();
            var nodes = new List<Type> { typeof( SysF ) };

            var root = compiler.Compile( CreateMockNativeLoop(), nodes, BucketHandling.IncludeThrow );

            var updatePhase = root.subSystemList.First( s => s.type == typeof( UnityEngine.PlayerLoop.Update ) );
            var list = updatePhase.subSystemList.Select( s => s.type ).ToList();

            int idxGroup = list.IndexOf( typeof( CustomGroup ) );
            Assert.That( idxGroup, Is.GreaterThanOrEqualTo( 0 ), "CustomGroup should be scaffolded inside Update." );

            Assert.That( list.IndexOf( typeof( NativePhase1 ) ), Is.LessThan( idxGroup ) );
            Assert.That( idxGroup, Is.LessThan( list.IndexOf( typeof( NativePhase2 ) ) ) );

            var groupSystem = updatePhase.subSystemList.First( s => s.type == typeof( CustomGroup ) );
            Assert.That( groupSystem.subSystemList.Any( s => s.type == typeof( SysF ) ), Is.True, "SysF should be mapped inside CustomGroup." );
        }

        [Test]
        public void Compiler_ScaffoldsDeeplyNestedMissingGroup()
        {
            var compiler = new PlayerLoopCompiler();
            var nodes = new List<Type> { typeof( SysG ) };

            var root = compiler.Compile( CreateMockNativeLoop(), nodes, BucketHandling.IncludeThrow );

            var updatePhase = root.subSystemList.First( s => s.type == typeof( UnityEngine.PlayerLoop.Update ) );
            var groupSystem = updatePhase.subSystemList.First( s => s.type == typeof( CustomGroup ) );
            var nestedGroup = groupSystem.subSystemList.First( s => s.type == typeof( NestedCustomGroup ) );

            Assert.That( nestedGroup.subSystemList.Any( s => s.type == typeof( SysG ) ), Is.True, "SysG should be deeply mapped." );
        }

        [Test]
        public void Compiler_ThrowsOnRawUnityType()
        {
            var compiler = new PlayerLoopCompiler();
            var nodes = new List<Type> { typeof( SysInvalid ) };

            Assert.Throws<InvalidOperationException>( () =>
            {
                compiler.Compile( CreateMockNativeLoop(), nodes, BucketHandling.IncludeThrow );
            } );
        }
    }
}