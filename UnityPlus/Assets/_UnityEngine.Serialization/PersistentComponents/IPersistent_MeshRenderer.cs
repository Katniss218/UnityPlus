using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;

namespace UnityEngine.Serialization
{
    public static class IPersistent_MeshRenderer
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken GetData( this MeshRenderer mr, Saver s )
        {
            JArray matsJson = new JArray( mr.sharedMaterials.Select( mat => s.WriteAssetReference( mat ) ) );

            return new JObject()
            {
                { "shared_materials", matsJson },
                { "shadow_casting_mode", mr.shadowCastingMode.ToString() },
                { "receive_shadows", mr.receiveShadows }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this MeshRenderer mr, Loader l, JObject json )
        {
            if( json.TryGetValue( "shared_materials", out var jsonSharedMaterials ) )
            {
                List<Material> mats = new List<Material>();
                foreach( var sharedMatJson in jsonSharedMaterials )
                {
                    Material mat = l.ReadAssetReference<Material>( sharedMatJson );
                    mats.Add( mat );
                }

                mr.sharedMaterials = mats.ToArray();
            }

            if( json.TryGetValue( "shadow_casting_mode", out var jsonShadowCastingMode ) )
                mr.shadowCastingMode = Enum.Parse<ShadowCastingMode>( (string)jsonShadowCastingMode );

            if( json.TryGetValue( "receive_shadows", out var jsonReceiveShadows ) )
                mr.receiveShadows = (bool)jsonReceiveShadows;
        }
    }
}