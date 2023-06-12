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
    public static class IPersistent_MeshFilter
    {
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken GetData( this MeshFilter mf, Saver s )
        {
            return new JObject()
            {
                { "shared_mesh", s.WriteAssetReference( mf.sharedMesh ) }
            };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this MeshFilter mf, Loader l, JObject json )
        {
            if( json.TryGetValue( "shared_mesh", out var jsonSharedMesh ) )
                mf.sharedMesh = l.ReadAssetReference<Mesh>( jsonSharedMesh );
        }
    }
}