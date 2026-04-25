using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.TestTools;
using UnityPlus.AssetManagement;

namespace AssetManagementTests
{
    [TestFixture]
    public class AssetRegistryTests
    {
        [SetUp]
        public void SetUp()
        {
            AssetRegistry.Clear();
        }

        [TearDown]
        public void TearDown()
        {
            AssetRegistry.Clear();
        }

        // --- MOCKS ---

        public class MockAsset
        {
            public string Value;
        }

        public class MockHandle : AssetDataHandle
        {
            public string Content;
            public override AssetFormat Format => new AssetFormat( "MOCK" );

            public override Task<byte[]> PeekBytesAsync( int count, CancellationToken ct )
            {
                return Task.FromResult( new byte[0] );
            }

            public override Task<Stream> OpenMainStreamAsync( CancellationToken ct )
            {
                return Task.FromResult<Stream>( new MemoryStream( System.Text.Encoding.UTF8.GetBytes( Content ) ) );
            }

            public override Task<bool> TryOpenSidecarAsync( string sidecarExtension, out Stream stream, CancellationToken ct )
            {
                stream = null;
                return Task.FromResult( false );
            }

            public override void Dispose() { }
        }

        public class MockResolver : IAssetResolver
        {
            public string ID => "Test.MockResolver";
            public string[] Before => null;
            public string[] After => null;
            public string[] Blacklist => null;

            public int CallCount = 0;
            public string ResolveTarget = "mock::foo";

            public bool CanResolve( AssetUri uri, Type targetType )
            {
                return uri.BaseID == ResolveTarget;
            }

            public Task<IEnumerable<AssetDataHandle>> ResolveAsync( AssetUri uri, CancellationToken ct )
            {
                CallCount++;
                var handle = new MockHandle { Content = "ResolvedContent" };
                return Task.FromResult<IEnumerable<AssetDataHandle>>( new[] { handle } );
            }
        }

        public class MockLoader : IAssetLoader
        {
            public Type OutputType => typeof( MockAsset );
            public int CallCount = 0;
            public TaskCompletionSource<bool> PauseSignal = null;

            public bool CanLoad( AssetDataHandle handle, Type targetType )
            {
                return handle.Format == new AssetFormat( "MOCK" );
            }

            public async Task<object> LoadAsync( AssetDataHandle handle, Type targetType, CancellationToken ct )
            {
                CallCount++;

                if( PauseSignal != null )
                {
                    await PauseSignal.Task;
                }

                if( handle is MockHandle mh )
                {
                    return new MockAsset { Value = mh.Content };
                }
                return null;
            }
        }

        // --- TESTS ---

        [Test]
        public void Register_And_Get_Sync_Works()
        {
            var asset = new MockAsset { Value = "Direct" };
            AssetRegistry.Register( "mock::direct", asset );

            Assert.That( AssetRegistry.IsLoaded( "mock::direct" ), Is.True );
            Assert.That( AssetRegistry.Get<MockAsset>( "mock::direct" ), Is.EqualTo( asset ) );
        }

        [Test]
        public void Unregister_RemovesAsset()
        {
            var asset = new MockAsset();
            AssetRegistry.Register( "mock::temp", asset );

            Assert.That( AssetRegistry.IsLoaded( "mock::temp" ), Is.True );

            AssetRegistry.Unregister( "mock::temp" );

            Assert.That( AssetRegistry.IsLoaded( "mock::temp" ), Is.False );
            Assert.That( AssetRegistry.Get<MockAsset>( "mock::temp" ), Is.Null );
        }

        [Test]
        public void Unregister_ByReference_RemovesAsset()
        {
            var asset = new MockAsset();
            AssetRegistry.Register( "mock::temp_ref", asset );

            AssetRegistry.Unregister( asset );

            Assert.That( AssetRegistry.IsLoaded( "mock::temp_ref" ), Is.False );
        }

        [Test]
        public void PruneDestroyedAssets_RemovesNullUnityObjects()
        {
            // Unity Objects require special handling
            GameObject go = new GameObject( "TestAsset" );
            AssetRegistry.Register( "unity::go", go );

            Assert.That( AssetRegistry.Get<GameObject>( "unity::go" ), Is.Not.Null );

            // Destroy
            UnityEngine.Object.DestroyImmediate( go );

            // Prune
            AssetRegistry.PruneDestroyedAssets();

            Assert.That( AssetRegistry.IsLoaded( "unity::go" ), Is.False );
        }

        [UnityTest]
        public IEnumerator Pipeline_ResolveAndLoad_Works()
        {
            var resolver = new MockResolver();
            var loader = new MockLoader();

            AssetRegistry.RegisterResolver( resolver );
            AssetRegistry.RegisterLoader( loader );

            Task<MockAsset> task = AssetRegistry.GetAsync<MockAsset>( "mock::foo" );

            while( !task.IsCompleted ) yield return null;

            MockAsset result = task.Result;

            Assert.That( result, Is.Not.Null );
            Assert.That( result.Value, Is.EqualTo( "ResolvedContent" ) );
            Assert.That( resolver.CallCount, Is.EqualTo( 1 ) );
            Assert.That( loader.CallCount, Is.EqualTo( 1 ) );
            Assert.That( AssetRegistry.IsLoaded( "mock::foo" ), Is.True ); // Should be cached
        }

        [UnityTest]
        public IEnumerator Pipeline_Deduplication_Works()
        {
            var resolver = new MockResolver();
            var loader = new MockLoader();
            loader.PauseSignal = new TaskCompletionSource<bool>(); // Pause loading

            AssetRegistry.RegisterResolver( resolver );
            AssetRegistry.RegisterLoader( loader );

            // Start two requests
            Task<MockAsset> task1 = AssetRegistry.GetAsync<MockAsset>( "mock::foo" );
            Task<MockAsset> task2 = AssetRegistry.GetAsync<MockAsset>( "mock::foo" );

            // Wait a frame to ensure logic has run up to the pause
            yield return null;

            // Release
            loader.PauseSignal.SetResult( true );

            while( !task1.IsCompleted || !task2.IsCompleted ) yield return null;

            Assert.That( task2.Result, Is.EqualTo( task1.Result ) ); // Same instance
            Assert.That( resolver.CallCount, Is.EqualTo( 1 ), "Resolver should only be called once" );
            Assert.That( loader.CallCount, Is.EqualTo( 1 ), "Loader should only be called once" );
        }

        [UnityTest]
        public IEnumerator Pipeline_TypeMismatch_ReturnsNull()
        {
            var resolver = new MockResolver(); // Returns MockHandle
            var loader = new MockLoader();     // Loads MockAsset

            AssetRegistry.RegisterResolver( resolver );
            AssetRegistry.RegisterLoader( loader );

            // Request Texture2D instead of MockAsset
            // Loader.OutputType (MockAsset) is not assignable to Texture2D
            Task<Texture2D> task = AssetRegistry.GetAsync<Texture2D>( "mock::foo" );

            while( !task.IsCompleted ) yield return null;

            Assert.That( task.Result, Is.Null );
            // Resolver runs, but loader should be skipped or fail CanLoad check implicitly by type
            // (The implementation checks Type before calling CanLoad)
            Assert.That( resolver.CallCount, Is.EqualTo( 1 ) );
            Assert.That( loader.CallCount, Is.EqualTo( 0 ) );
        }

        // Circular Dependency Mock
        public class CircularResolver : IAssetResolver
        {
            public string ID => "Test.Circular";
            public string[] Before => null;
            public string[] After => null;
            public string[] Blacklist => null;

            public bool CanResolve( AssetUri uri, Type targetType ) => true;

            public Task<IEnumerable<AssetDataHandle>> ResolveAsync( AssetUri uri, CancellationToken ct )
            {
                // Trigger recursive load
                // A -> B -> A
                if( uri.BaseID == "A" )
                {
                    // Synchronously blocking on async inside resolve is bad practice but possible here to test recursion check.
                    // However, GetAsync spins off tasks. 
                    // To properly test IsLoadingRecursive, we need to call GetAsync inside the flow.
                    // Since Resolve is async, we can await.
                    var t = AssetRegistry.GetAsync<MockAsset>( "B" );
                }
                else if( uri.BaseID == "B" )
                {
                    var t = AssetRegistry.GetAsync<MockAsset>( "A" );
                }

                return Task.FromResult<IEnumerable<AssetDataHandle>>( null );
            }
        }

        [UnityTest]
        public IEnumerator Pipeline_CircularDependency_IsHandled()
        {
            // Note: The registry's recursion check relies on AsyncLocal.
            // When we await inside ResolveAsync, we are in the same async context flow.

            var resolver = new CircularResolver();
            AssetRegistry.RegisterResolver( resolver );

            Task<MockAsset> task = AssetRegistry.GetAsync<MockAsset>( "A" );

            // We expect this to complete (likely with null) without hanging or stack overflow.
            // The deadlock protection or cycle check should return null for the re-entrant call.

            // Allow some time
            float timeout = Time.realtimeSinceStartup + 1.0f;
            while( !task.IsCompleted && Time.realtimeSinceStartup < timeout ) yield return null;

            Assert.That( task.IsCompleted, Is.True, "Circular dependency caused hang." );
            Assert.That( task.Result, Is.Null );
        }
    }
}