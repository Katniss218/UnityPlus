using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.AssetManagement
{
    /// <summary>
    /// A static registry for assets.
    /// Handles retrieval via cache or the async Resolver-Loader pipeline.
    /// Supports multiple assets sharing the same ID, differentiated by their System.Type.
    /// </summary>
    public static class AssetRegistry
    {
        // Cache: Maps ID -> List of loaded objects (to support collision/overloading by type)
        private static readonly Dictionary<string, List<object>> _loaded = new Dictionary<string, List<object>>();
        private static readonly Dictionary<object, string> _inverseLoaded = new Dictionary<object, string>();

        private static List<IAssetResolver> _resolvers = new List<IAssetResolver>();
        private static List<IAssetLoader> _loaders = new List<IAssetLoader>();

        // Async deduplication
        private static readonly Dictionary<(string id, Type reqType), Task<object>> _loadingTasks = new Dictionary<(string, Type), Task<object>>();

        private static readonly object _lock = new object();

        // Cycle Detection / Re-entrancy Context
        private class LoadNode
        {
            public string AssetID;
            public LoadNode Parent;
        }
        private static readonly AsyncLocal<LoadNode> _reentrancyStack = new AsyncLocal<LoadNode>();

        /// <summary>
        /// The maximum time (in milliseconds) a synchronous Get<T> call will wait before giving up.
        /// Default: 30 seconds.
        /// </summary>
        public static int SynchronousLoadTimeoutMs = 30000;

        /// <summary>
        /// The number of cached (loaded) assets in the registry.
        /// </summary>
        public static int LoadedCount
        {
            get { lock( _lock ) return _inverseLoaded.Count; }
        }

        public static void RegisterResolver( IAssetResolver resolver )
        {
            if( resolver == null )
                throw new ArgumentNullException( nameof( resolver ) );

            lock( _lock )
            {
                if( !_resolvers.Contains( resolver ) )
                {
                    _resolvers.Add( resolver );
                    _resolvers = _resolvers.SortDependencies<IAssetResolver, string>()
                        .ToList();
                }
            }
        }

        public static void UnregisterResolver( IAssetResolver resolver )
        {
            if( resolver == null )
                throw new ArgumentNullException( nameof( resolver ) );

            lock( _lock )
            {
                _resolvers.Remove( resolver );
                _resolvers = _resolvers.SortDependencies<IAssetResolver, string>()
                    .ToList();
            }
        }

        public static void RegisterLoader( IAssetLoader loader )
        {
            if( loader == null )
                throw new ArgumentNullException( nameof( loader ) );

            lock( _lock )
            {
                if( !_loaders.Contains( loader ) )
                    _loaders.Add( loader );
            }
        }

        public static void UnregisterLoader( IAssetLoader loader )
        {
            if( loader == null )
                throw new ArgumentNullException( nameof( loader ) );

            lock( _lock )
                _loaders.Remove( loader );
        }

        /// <summary>
        /// Registers the given (loaded) object as an asset.
        /// If an asset with the same ID and Type exists, it will NOT be overwritten unless it is the exact same reference.
        /// </summary>
        public static void Register( string assetID, object asset )
        {
            if( assetID == null )
                throw new ArgumentNullException( nameof( assetID ) );
            if( asset == null )
                throw new ArgumentNullException( nameof( asset ) );

            lock( _lock )
            {
                if( !_loaded.TryGetValue( assetID, out List<object> assets ) )
                {
                    assets = new List<object>();
                    _loaded[assetID] = assets;
                }

                // Check for duplicate reference
                if( !assets.Contains( asset ) )
                {
                    assets.Add( asset );
                    _inverseLoaded[asset] = assetID;
                }
            }
        }

        /// <summary>
        /// Unregisters and unloads an asset.
        /// </summary>
        public static bool Unregister( string assetID )
        {
            if( assetID == null )
                throw new ArgumentNullException( nameof( assetID ) );

            lock( _lock )
            {
                if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                {
                    foreach( var asset in assets )
                    {
                        _inverseLoaded.Remove( asset );
                    }
                    _loaded.Remove( assetID );
                    return true;
                }
            }
            return false;
        }

        public static bool Unregister( object assetRef )
        {
            if( assetRef == null )
                throw new ArgumentNullException( nameof( assetRef ) );

            lock( _lock )
            {
                if( _inverseLoaded.TryGetValue( assetRef, out string assetID ) )
                {
                    _inverseLoaded.Remove( assetRef );

                    if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                    {
                        assets.Remove( assetRef );
                        if( assets.Count == 0 )
                            _loaded.Remove( assetID );
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Scans the registry for UnityEngine.Objects that have been destroyed (== null) and unregisters them.
        /// Call this periodically or on Scene Unload to prevent memory leaks in the inverse dictionary.
        /// </summary>
        public static void PruneDestroyedAssets()
        {
            lock( _lock )
            {
                var toRemove = new List<object>();
                foreach( var kvp in _inverseLoaded )
                {
                    // Check if it is a Unity Object and if it is destroyed
                    if( kvp.Key is UnityEngine.Object uObj && uObj == null )
                    {
                        toRemove.Add( kvp.Key );
                    }
                }

                foreach( var obj in toRemove )
                {
                    // Perform internal unregister logic without re-locking
                    if( _inverseLoaded.TryGetValue( obj, out string assetID ) )
                    {
                        _inverseLoaded.Remove( obj );
                        if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                        {
                            assets.Remove( obj );
                            if( assets.Count == 0 )
                                _loaded.Remove( assetID );
                        }
                    }
                }

                if( toRemove.Count > 0 )
                {
                    Debug.Log( $"[AssetRegistry] Pruned {toRemove.Count} destroyed assets." );
                }
            }
        }

        /// <summary>
        /// Retrieves the ID of a registered asset object.
        /// </summary>
        public static string GetAssetID( object assetRef )
        {
            if( assetRef == null )
                return null;

            lock( _lock )
            {
                if( _inverseLoaded.TryGetValue( assetRef, out string id ) )
                    return id;
            }

            return null;
        }

        public static List<string> GetLoadingAssets()
        {
            lock( _lock )
            {
                return _loadingTasks.Keys.Select( k => $"{k.Item1} ({k.Item2.Name})" ).ToList();
            }
        }

        public static bool IsLoaded( string assetID )
        {
            lock( _lock )
                return _loaded.ContainsKey( assetID );
        }

        /// <summary>
        /// Helper to find an asset of type T in a list of mixed-type objects.
        /// </summary>
        private static T FindTypedAsset<T>( List<object> assets ) where T : class
        {
            for( int i = 0; i < assets.Count; i++ )
            {
                if( assets[i] is T typedAsset )
                    return typedAsset;
            }
            return null;
        }

        //
        //
        //

        /// <summary>
        /// Retrieves a registered asset synchronously.
        /// </summary>
        public static T Get<T>( string assetID ) where T : class
        {
            if( assetID == null )
                throw new ArgumentNullException( nameof( assetID ) );

            // 1. Check Cache
            lock( _lock )
            {
                if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                {
                    T existing = FindTypedAsset<T>( assets );
                    if( existing != null )
                        return existing;
                }
            }

            // 2. Trigger Async Pipeline Synchronously
            try
            {
                using( var cts = new CancellationTokenSource( SynchronousLoadTimeoutMs ) )
                using( cts.Token.Register( () => Debug.LogWarning( $"[AssetRegistry] Synchronous load of '{assetID}' timed out after {SynchronousLoadTimeoutMs}ms." ) ) )
                {
                    Task<T> task = GetAsync<T>( assetID, cts.Token );

                    while( !task.IsCompleted )
                    {
                        MainThreadDispatcher.Pump();
                        if( !task.IsCompleted )
                            Thread.Sleep( 0 );
                    }

                    return task.GetAwaiter().GetResult();
                }
            }
            catch( AggregateException ae )
            {
                Exception inner = ae.Flatten().InnerException;
                Debug.LogError( $"AssetRegistry.Get<{typeof( T ).Name}>('{assetID}') failed: {inner}" );
                return null;
            }
            catch( Exception ex )
            {
                Debug.LogError( $"AssetRegistry.Get<{typeof( T ).Name}>('{assetID}') failed: {ex}" );
                return null;
            }
        }

        /// <summary>
        /// Retrieves an asset asynchronously. 
        /// </summary>
        public static async Task<T> GetAsync<T>( string assetID, CancellationToken ct = default ) where T : class
        {
            if( string.IsNullOrEmpty( assetID ) )
                return null;

            // 1. Fast Cache Check
            lock( _lock )
            {
                if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                {
                    T existing = FindTypedAsset<T>( assets );
                    if( existing != null )
                        return existing;
                }
            }

            // 2. Cycle Detection
            bool isReentrant = IsLoadingRecursive( assetID );

            // 3. Deduplication
            Task<object> task;
            var loadKey = (assetID, typeof( T ));

            lock( _lock )
            {
                // We use a composite key (ID, Type) to prevent race conditions where loading 'Foo' as Texture 
                // would otherwise block loading 'Foo' as JSON.
                if( !isReentrant && _loadingTasks.TryGetValue( loadKey, out task ) )
                {
                    // Task exists, await it.
                }
                else
                {
                    task = GetAsyncInternal<T>( assetID, ct );
                    if( !isReentrant )
                        _loadingTasks[loadKey] = task;
                }
            }

            try
            {
                object result = await task.ConfigureAwait( false );

                if( result is T typedResult )
                    return typedResult;

                // Fallback: If deduplication logic failed us (unlikely with typed keys) or another thread finished 
                // just before we started, check cache one last time.
                lock( _lock )
                {
                    if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                    {
                        T existing = FindTypedAsset<T>( assets );
                        if( existing != null )
                            return existing;
                    }
                }

                return null;
            }
            finally
            {
                lock( _lock )
                {
                    // Only remove if it's the exact task we added/found
                    if( _loadingTasks.TryGetValue( loadKey, out var t ) && t == task )
                    {
                        _loadingTasks.Remove( loadKey );
                    }
                }
            }
        }

        private static bool IsLoadingRecursive( string assetID )
        {
            LoadNode current = _reentrancyStack.Value;

            while( current != null )
            {
                if( current.AssetID == assetID )
                    return true;
                current = current.Parent;
            }

            return false;
        }

        private static async Task<object> GetAsyncInternal<T>( string assetID, CancellationToken ct ) where T : class
        {
            LoadNode parentNode = _reentrancyStack.Value;
            _reentrancyStack.Value = new LoadNode()
            {
                AssetID = assetID,
                Parent = parentNode
            };

            try
            {
                // Double-check cache
                lock( _lock )
                {
                    if( _loaded.TryGetValue( assetID, out List<object> assets ) )
                    {
                        T existing = FindTypedAsset<T>( assets );
                        if( existing != null )
                            return existing;
                    }
                }

                if( !AssetUri.TryParse( assetID, out AssetUri uri ) )
                    return null;

                List<IAssetResolver> activeResolvers;
                lock( _lock )
                    activeResolvers = new List<IAssetResolver>( _resolvers );

                // 1. RESOLUTION PHASE
                List<AssetDataHandle> candidates = new List<AssetDataHandle>();

                foreach( var resolver in activeResolvers )
                {
                    if( resolver.CanResolve( uri , typeof( T ) ) )
                    {
                        try
                        {
                            IEnumerable<AssetDataHandle> handles = await resolver.ResolveAsync( uri, ct ).ConfigureAwait( false );
                            if( handles != null )
                            {
                                candidates.AddRange( handles );
                            }
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Resolver {((IOverridable<string>)resolver).ID} failed for {assetID}: {ex}" );
                        }
                    }
                }

                if( candidates.Count == 0 )
                    return null;

                // 2. LOADING PHASE
                try
                {
                    List<IAssetLoader> activeLoaders;
                    lock( _lock ) activeLoaders = new List<IAssetLoader>( _loaders );

                    foreach( var handle in candidates )
                    {
                        if( handle == null )
                            continue;

                        foreach( var loader in activeLoaders )
                        {
                            // 1. Type Check: Does this loader produce the type we want?
                            if( !typeof( T ).IsAssignableFrom( loader.OutputType ) )
                                continue;

                            // 2. Format Check: Can the loader handle this data?
                            if( loader.CanLoad( handle, typeof( T ) ) )
                            {
                                // MATCH FOUND
                                // Run loader on ThreadPool to avoid blocking main thread with heavy parsing logic.
                                // Loaders are responsible for dispatching unity-api calls to main thread.
                                object result = await Task.Run( async () =>
                                {
                                    return await loader.LoadAsync( handle, typeof( T ), ct ).ConfigureAwait( false );
                                }, ct ).ConfigureAwait( false );

                                if( result != null )
                                {
                                    Register( assetID, result );
                                    return result;
                                }
                            }
                        }
                    }
                }
                finally
                {
                    // Dispose all handles
                    foreach( var handle in candidates )
                    {
                        try
                        {
                            handle?.Dispose();
                        }
                        catch( Exception ex )
                        {
                            Debug.LogError( $"Error disposing handle for {assetID}: {ex}" );
                        }
                    }
                }

                return null;
            }
            finally
            {
                _reentrancyStack.Value = parentNode;
            }
        }
    }
}