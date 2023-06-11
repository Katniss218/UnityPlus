using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization
{
    public static class Saver_Ex_Primitives
    {
        static Dictionary<Type, string> _typeToString = new Dictionary<Type, string>();

        // Primitives in this context are types that are always saved in-place.

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken WriteVector2( this Saver _, Vector2 v )
        {
            return new JArray() { v.x, v.y };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken WriteVector2Int( this Saver _, Vector2Int v )
        {
            return new JArray() { v.x, v.y };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken WriteVector3( this Saver _, Vector3 v )
        {
            return new JArray() { v.x, v.y, v.z };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken WriteVector3Int( this Saver _, Vector3Int v )
        {
            return new JArray() { v.x, v.y, v.z };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken WriteVector4( this Saver _, Vector4 v )
        {
            return new JArray() { v.x, v.y, v.z, v.w };
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken WriteQuaternion( this Saver _, Quaternion q )
        {
            return new JArray() { q.x, q.y, q.z, q.w };
        }

        /// <summary>
        /// Writes a Globally-Unique Identifier (GUID/UUID)
        /// </summary>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken WriteGuid( this Saver _, Guid value )
        {
            // GUIDs should be saved in the '00000000-0000-0000-0000-000000000000' format.
            return new JValue( value.ToString( "D" ) );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static JToken WriteType( this Saver _, Type value )
        {
            // 'AssemblyQualifiedName' is guaranteed to always uniquely identify a type.
            if( _typeToString.TryGetValue( value, out string assemblyQualifiedName ) )
            {
                return new JValue( assemblyQualifiedName );
            }

            // Cache the type because accessing the Type.AssemblyQualifiedName and Type.GetType(string) is very slow.
            assemblyQualifiedName = value.AssemblyQualifiedName;
            _typeToString.Add( value, assemblyQualifiedName );

            return new JValue( assemblyQualifiedName );
        }
    }
}