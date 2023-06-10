using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.AssetManagement
{
    public static class Registry<T>
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

        // registry items don't have to be loaded at startup, if a provider exists that can read e.g. an .fbx file, a mesh can be imported at runtime.

        static Dictionary<string, T> _cache = new Dictionary<string, T>();
        static Dictionary<string, Func<T>> _lazyCache = new Dictionary<string, Func<T>>();

        static Dictionary<T, string> _inverseCache = new Dictionary<T, string>();

        static IAssetProvider<T>[] _providers;

        public static T Get( string assetID )
        {
            if( _cache.TryGetValue( assetID, out T val ) && val != null )
            {
                return val;
            }

            if( _lazyCache.TryGetValue( assetID, out var loader ) )
            {
                T asset = loader();
                Register( assetID, asset );
                return asset;
            }

            throw new Exception();
            // providers can provide a specific asset if it doesn't already exist.
        }

        /// <summary>
        /// Registers an object as an asset.
        /// </summary>
        /// <param name="assetID">The Asset ID to register the object under.</param>
        /// <param name="asset">The asset object to register.</param>
        public static void Register( string assetID, T asset )
        {
            _cache[assetID] = asset;
            _inverseCache[asset] = assetID;
        }

        /// <summary>
        /// Registers a lazy-loaded asset.
        /// </summary>
        /// <param name="assetID">The Asset ID to register the object under.</param>
        /// <param name="loader">The function that will load the asset when requested.</param>
        public static void RegisterLazy( string assetID, Func<T> loader )
        {
            // () => PNGLoad(imagePathVariable); // for example
            _lazyCache[assetID] = loader;
        }

        public static string GetAssetID( T assetRef )
        {
            // assetref should be an instance cached in this registry.

            if( _inverseCache.TryGetValue( assetRef, out string assetID ) )
            {
                return assetID;
            }

            return null;
        }
    }
}