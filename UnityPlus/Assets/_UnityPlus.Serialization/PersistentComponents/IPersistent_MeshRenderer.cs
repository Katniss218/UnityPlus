﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityPlus.Serialization
{
    public static class IPersistent_MeshRenderer
    {
        public static SerializedData GetData( this MeshRenderer mr, ISaver s )
        {
            SerializedArray matsJson = new SerializedArray();
            var mats = mr.sharedMaterials.Select( mat => s.WriteAssetReference( mat ) );
            foreach( var mat in mats )
            {
                matsJson.Add( mat );
            }

            return new SerializedObject()
            {
                { "shared_materials", matsJson },
                { "shadow_casting_mode", mr.shadowCastingMode.ToString() },
                { "receive_shadows", mr.receiveShadows }
            };
        }

        public static void SetData( this MeshRenderer mr, ILoader l, SerializedObject json )
        {
            if( json.TryGetValue( "shared_materials", out var jsonSharedMaterials ) )
            {
                List<Material> mats = new List<Material>();
                foreach( var sharedMatJson in (SerializedArray)jsonSharedMaterials )
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