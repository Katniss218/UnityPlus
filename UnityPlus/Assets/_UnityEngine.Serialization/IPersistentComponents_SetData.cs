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
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static void SetData( this Component c, Loader l, JObject json )
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