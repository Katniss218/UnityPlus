using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AssetManagement;

namespace UnityEngine.Serialization.Factories
{
    [CreateAssetMenu( fileName = "prefab factory", menuName = "PrefabFactory", order = 1 )]
    public class PrefabAssetFactory : ScriptableObject, IFactory<GameObject>
    {
        /// <summary>
        /// The asset ID of the prefab that this factory will instantiate.
        /// </summary>
        [field: SerializeField]
        public string PrefabAssetID { get; set; }

        public GameObject Create()
        {
            GameObject prefab = Registry.Get<GameObject>( this.PrefabAssetID );
            if( prefab == null )
            {
                throw new InvalidOperationException( $"A `{nameof( GameObject )}` with the assetID `{PrefabAssetID}` couldn't be loaded from the asset registry." );
            }
            string factoryAssetID = Registry.GetAssetID( this );
            if( factoryAssetID == null )
            {
                throw new InvalidOperationException( $"The factory instance was not referenced from the asset registry." );
            }

            GameObject go = Instantiate( prefab );

            CreatedByFactory fcomp = go.GetComponent<CreatedByFactory>();
            if( fcomp == null )
                fcomp = go.AddComponent<CreatedByFactory>();
            fcomp.FactoryAssetID = factoryAssetID;

            return go;
        }

        public JToken GetData( Saver s )
        {
#warning TODO - if possible to return asset ID from a resources asset, it would be possible to do a reverse lookup and serialize an inspector-assigned reference too.
            return new JObject()
            {
                { "AssetID", this.PrefabAssetID }
            };
        }

        public void SetData( Loader l, JToken json )
        {
            this.PrefabAssetID = (string)json["AssetID"];
        }
    }
}