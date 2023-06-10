using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.AssetManagement
{
    /// <summary>
    /// Asset providers are used to return assets and they also map the asset ID to asset path.
    /// </summary>
    /// <remarks>
    /// Please include an empty default constructor if you inherit from this class.
    /// </remarks>
    public interface IAssetProvider
    {
        /// <summary>
        /// Specifies which calls to <see cref="Registry.Get"/> this provider will respond to.
        /// </summary>
        /// <remarks>
        /// Examples: <br />
        /// - <see cref="typeof(GameObject)"/> for prefabs <br />
        /// - <see cref="typeof(Texture2D)"/> for textures <br />
        /// - <see cref="typeof(object)"/> for anything and everything <br />
        /// - etc.
        /// </remarks>
        Type ProvidedType { get; }

        /// <summary>
        /// Tries to load an asset with the specified asset ID.
        /// </summary>
        /// <remarks>
        /// NOTE TO IMPLEMENTERS: <br />
        /// - You're supposed to convert the asset ID into an asset path (if applicable). No asset path will be provided by the caller.
        /// </remarks>
        object TryLoad( string assetID );
    }
}