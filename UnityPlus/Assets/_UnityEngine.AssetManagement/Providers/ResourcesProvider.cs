using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.AssetManagement.Providers
{
    /// <summary>
    /// Provides an arbitrary object from the `Resources` directory.
    /// </summary>
    /// <remarks>
    /// The assetID is used directly as the path to the Resources file, using the `resources::` string prefix in front of the path.
    /// </remarks>
    public class ResourcesProvider : IAssetProvider
    {
        public const string PREFIX = "resources::";

        public Type ProvidedType { get; } = typeof( UnityEngine.Object ); // Resources.Load can only load Unity assets. I.e. UnityEngine.Object.

        public object TryLoad( string assetID )
        {
            if( assetID.StartsWith( PREFIX ) )
            {
                return Resources.Load( assetID[PREFIX.Length..] );
            }

            return null;
        }
    }
}