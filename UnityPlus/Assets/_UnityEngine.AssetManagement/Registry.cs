using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityEngine.AssetManagement
{
    /// <summary>
    /// A static registry for assets.
    /// </summary>
    /// <remarks>
    /// Register assets at startup, or use a provider to automatically register assets that match certain asset paths.
    /// </remarks>
    public static class Registry
    {
        // Registry is a class used to manage shared (singleton) references.

        // These can be:
        // - assets (prefabs, sprites, materials, etc).
        // - data   (loot tables, propellants, units, spells, dialogues, etc).

        // The distinguishing part is that there exists a "provider" that can find and return an item (asset/data) using an ID.
        //              It doesn't really have to be an ID per say, could also be a path, or other sequence of characters uniquely identifying an item.

        // Another key part is that the game will not reference things that are not a gameobject or component of which in the scene, or an item of the registry.

        // Allowed references:
        // - Registry Item
        // - GameObject
        // - Component of GameObject
        // - Event of Component of gameObject

        // registry items don't have to be loaded at startup, if a function is provided to the lazy loader, that can read e.g. an .fbx file, a mesh can be imported at runtime.
        // if a provider is used, the registry can also try to load assets under asset IDs that didn't have an asset registered yet.

        static Dictionary<string, object> _cache = new Dictionary<string, object>();
        static Dictionary<string, Func<object>> _lazyCache = new Dictionary<string, Func<object>>();

        static Dictionary<object, string> _inverseCache = new Dictionary<object, string>();

        static IAssetProvider[] _providers;

        static Registry()
        {
            ReloadProviders();
        }

        public static void ReloadProviders()
        {
            Type providerType = typeof( IAssetProvider );

            List<Type> prov = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() )
                .Where( t => t != providerType )// not the generic provider interface itself
                .Where( t => providerType.IsAssignableFrom( t ) )
                .ToList();

            _providers = new IAssetProvider[prov.Count];
            for( int i = 0; i < _providers.Length; i++ )
            {
                _providers[i] = (IAssetProvider)Activator.CreateInstance( prov[i] );
            }
        }

        /// <summary>
        /// Retrieves a registered asset, performs type conversion on the returned asset.
        /// </summary>
        /// <remarks>
        /// If nothing is registered under the specified <paramref name="assetID"/>, the registry will try to use an <see cref="IAssetProvider"/> to load an asset with a given <paramref name="assetID"/>.
        /// </remarks>
        /// <typeparam name="T">The type to convert to.</typeparam>
        /// <param name="assetID">The asset ID under which the asset is registered.</param>
        /// <returns>The registered asset, converted to the specified type <typeparamref name="T"/>. <br />
        /// <see cref="null"/> if no asset can be found. <br />
        /// <see cref="null"/> if the conversion fails.</returns>
        public static T Get<T>( string assetID ) where T : class
        {
            // Try to get an already loaded asset.
            if( _cache.TryGetValue( assetID, out object val ) && val != null )
            {
                return val as T;
            }

            // Try to load a lazy asset.
            if( _lazyCache.TryGetValue( assetID, out Func<object> loader ) )
            {
                object asset = loader();
                Register( assetID, asset );
                return asset as T;
            }

            Type type = typeof( T );
            // Try to use a provider to load an asset.
            // - Providers can provide a specific asset if it doesn't already exist.
            // - They automatically convert asset IDs into asset paths to load.
            foreach( var provider in _providers )
            {
                if( provider.ProvidedType.IsAssignableFrom( type ) )
                {
                    object obj = provider.TryLoad( assetID );

                    if( obj == null ) 
                        continue;

                    Register( assetID, obj );
                    return obj as T;
                }
            }

            return default;
        }

        /// <summary>
        /// Registers an object as an asset.
        /// </summary>
        /// <param name="assetID">The Asset ID to register the object under.</param>
        /// <param name="asset">The asset object to register.</param>
        public static void Register( string assetID, object asset )
        {
            _cache[assetID] = asset;
            _inverseCache[asset] = assetID;
        }

        /// <summary>
        /// Registers a lazy-loaded asset. <br />
        /// </summary>
        /// <remarks>
        /// A lazy-loaded asset will only be loaded if/when it's requested.
        /// </remarks>
        /// <param name="assetID">The Asset ID to register the object under.</param>
        /// <param name="loader">The function that will load the asset when requested.</param>
        public static void RegisterLazy( string assetID, Func<object> loader )
        {
            // () => PNGLoad(imagePathVariable); // for example
            _lazyCache[assetID] = loader;
        }

        /// <summary>
        /// Returns the asset ID of a registered asset.
        /// </summary>
        /// <param name="assetRef">A reference to an asset retrieved from this registry.</param>
        public static string GetAssetID( object assetRef )
        {
            // We don't have to bother checking providers, since we expect the parameter object to already come from the registry.

            if( _inverseCache.TryGetValue( assetRef, out string assetID ) )
            {
                return assetID;
            }

            return null;
        }
    }
}