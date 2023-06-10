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
    public class PrefabFactory : ScriptableObject, IFactory<GameObject>
    {
        [field: SerializeField]
        public string ID { get; private set; }

        [field: SerializeField]
        public string AssetID { get; set; }

        public GameObject Create()
        {
            GameObject prefab = Registry.Get<GameObject>( this.AssetID );
            if( prefab == null )
            {
                Debug.LogWarning( $"Prefab with an assetID `{AssetID}` was not in the {nameof( Registry )}." );
            }

            GameObject go = Instantiate( prefab );
            return go;
        }

        public void SetData( JToken json )
        {
            this.AssetID = (string)json["AssetID"];
        }

        public JToken GetData()
        {
            return new JObject()
            {
                { "AssetID", this.AssetID }
            };
        }
    }
}
