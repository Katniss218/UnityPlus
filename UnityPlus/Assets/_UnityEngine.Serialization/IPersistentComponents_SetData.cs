using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AssetManagement;
using UnityEngine.Rendering;

namespace UnityEngine.Serialization
{
    public static class IPersistentComponents_SetData
    {
        // Methods here are equivalents of IPersistent.SetData, but for builtin components that don't implement this interface.

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this Transform t, Loader l, JToken json )
        {
            t.localPosition = l.ReadVector3( json["local_position"] );
            t.localRotation = l.ReadQuaternion( json["local_rotation"] );
            t.localScale = l.ReadVector3( json["local_scale"] );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this MeshFilter mf, Loader l, JToken json )
        {
            mf.sharedMesh = l.ReadAssetReference<Mesh>( json["shared_mesh"] );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this MeshRenderer mr, Loader l, JToken json )
        {
            List<Material> mats = new List<Material>();
            foreach( var sharedMatJson in json["shared_materials"] )
            {
                Material mat = l.ReadAssetReference<Material>( sharedMatJson );
                mats.Add( mat );
            }

            mr.sharedMaterials = mats.ToArray();
            mr.shadowCastingMode = Enum.Parse<ShadowCastingMode>( (string)json["shadow_casting_mode"] );
            mr.receiveShadows = (bool)json["receive_shadows"];
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this Component c, Loader l, JToken json )
        {
            // component "data" means that the component (which is a referencable object) has already been added by an object action, and we're now reading its data.

            switch( c )
            {
                case IPersistent comp:
                    comp.SetData( l, json ); break;
                case Transform comp:
                    comp.SetData( l, json ); break;
                case MeshFilter comp:
                    comp.SetData( l, json ); break;
                case MeshRenderer comp:
                    comp.SetData( l, json ); break;
            }
        }
    }
}