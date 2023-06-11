using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization
{
    public static partial class IPersistentComponents_GetData
    {
        // Methods here are equivalents of IPersistent.GetData, but for builtin components that don't implement this interface.

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken GetData( this Transform t, Saver s )
        {
            return new JObject()
            {
                { "local_position", s.WriteVector3( t.localPosition ) },
                { "local_rotation", s.WriteQuaternion( t.localRotation ) },
                { "local_scale", s.WriteVector3( t.localScale ) }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken GetData( this MeshFilter mf, Saver s )
        {
            return new JObject()
            {
                { "shared_mesh", s.WriteAssetReference( mf.sharedMesh ) }
            };
        }

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
        public static JToken GetData( this Component c, Saver s )
        {
            switch( c )
            {
                case IPersistent comp:
                    return comp.GetData( s );
                case Transform comp:
                    return comp.GetData( s );
                case MeshFilter comp:
                    return comp.GetData( s );
                case MeshRenderer comp:
                    return comp.GetData( s );
            }
            return null;
        }
    }
}