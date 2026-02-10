using System;
using System.Collections.Generic;

namespace UnityPlus.Serialization
{
    public class DefaultTypeResolver : ITypeResolver
    {
        private readonly Dictionary<string, Type> _cache = new Dictionary<string, Type>();

        public Type ResolveType( string typeName )
        {
            if( _cache.TryGetValue( typeName, out var type ) )
                return type;

            // 1. Try standard lookup
            type = Type.GetType( typeName );

            // 2. Try iterating assemblies if standard fails (handles dynamically loaded or editor-only types)
            if( type == null )
            {
                foreach( var asm in AppDomain.CurrentDomain.GetAssemblies() )
                {
                    type = asm.GetType( typeName );
                    if( type != null ) break;
                }
            }

            if( type != null )
                _cache[typeName] = type;

            return type;
        }
    }
}