
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace UnityPlus.Serialization
{
    public static class Persistent_Type
    {
        private static readonly Dictionary<Type, string> _typeToString = new();
        private static readonly Dictionary<string, Type> _stringToType = new();

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static SerializedPrimitive SerializeType( this Type type )
        {
            if( type == null ) return null;

            if( _typeToString.TryGetValue( type, out string assemblyQualifiedName ) )
            {
                return (SerializedPrimitive)assemblyQualifiedName;
            }

            // 'AssemblyQualifiedName' is guaranteed to always uniquely identify a type.
            assemblyQualifiedName = type.AssemblyQualifiedName;
            _typeToString[type] = assemblyQualifiedName;
            
            // Pre-cache reverse lookup
            if (!_stringToType.ContainsKey(assemblyQualifiedName))
                _stringToType[assemblyQualifiedName] = type;

            return (SerializedPrimitive)assemblyQualifiedName;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Type DeserializeType( this SerializedData data )
        {
            return ResolveType( (string)data );
        }

        public static Type ResolveType( string assemblyQualifiedName )
        {
            if( string.IsNullOrEmpty( assemblyQualifiedName ) ) return null;

            if( _stringToType.TryGetValue( assemblyQualifiedName, out Type type ) )
            {
                return type;
            }

            // 1. Try Direct Lookup
            type = Type.GetType( assemblyQualifiedName );

            // 2. Try Assembly Scanning (Fallback for dynamic types)
            if( type == null )
            {
                foreach( var asm in AppDomain.CurrentDomain.GetAssemblies() )
                {
                    type = asm.GetType( assemblyQualifiedName );
                    if( type != null ) break;
                }
            }

            if( type != null )
            {
                _stringToType[assemblyQualifiedName] = type;
                if( !_typeToString.ContainsKey( type ) )
                    _typeToString[type] = assemblyQualifiedName;
            }

            return type;
        }
    }
}