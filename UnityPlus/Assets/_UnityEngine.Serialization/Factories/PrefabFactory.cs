using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AssetManagement;

namespace UnityEngine.Serialization.Factories
{
    public class PrefabFactory : Factory<GameObject>
    {
        public string AssetID { get; set; }

        public override GameObject Create()
        {
            GameObject prefab = Registry<GameObject>.Get( this.AssetID );

            GameObject go = UnityEngine.Object.Instantiate( prefab );
            return go;
        }

        public override void SetData( JToken json )
        {
            this.AssetID = (string)json["AssetID"];
        }

        public override JToken GetData()
        {
            return new JObject()
            {
                { "AssetID", this.AssetID }
            };
        }
    }
}
