
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps context marker types to unique IDs for the duration of the execution of the application.
    /// Also manages the structural rules of contexts (e.g. A List in Context X contains Elements in Context Y).
    /// </summary>
    public static class ContextRegistry
    {
        private static readonly Dictionary<Type, int> _typeToId = new Dictionary<Type, int>();
        private static readonly Dictionary<int, Type> _idToType = new Dictionary<int, Type>(); // Reverse lookup for debugging
        private static readonly Dictionary<int, string> _idToName = new Dictionary<int, string>(); // Name lookup

        // Start dynamic IDs high to avoid collision with legacy v3 constants
        private static int _nextDynamicId = 2000000;

        // Generic cache: "GenericTypeDefName + ArgIDs" -> ContextID
        // Key is string to avoid allocation of complex struct keys or tuples of varying length
        private static readonly Dictionary<string, int> _genericCombinations = new Dictionary<string, int>();

        // Structural Rules: Map a Context ID to its Generic Argument Context IDs.
        // e.g. Ctx.Dict<Ref, Asset> (ID 100) -> [ RefID, AssetID ]
        private static readonly Dictionary<int, int[]> _contextArguments = new Dictionary<int, int[]>();

        // Cache for context hierarchies
        private static readonly Dictionary<int, int[]> _hierarchyCache = new Dictionary<int, int[]>();

        /// <summary>
        /// Registers a fixed mapping between a Type and an ID. 
        /// Used for v3 backward compatibility.
        /// </summary>
        public static void Register( Type type, int id )
        {
            if( type == null ) return;
            _typeToId[type] = id;
            _idToType[id] = type;
        }

        public static void RegisterName( int id, string name )
        {
            _idToName[id] = name;
        }

        public static string GetContextName( ContextKey key )
        {
            if( _idToName.TryGetValue( key.ID, out string name ) ) return name;
            if( _idToType.TryGetValue( key.ID, out Type t ) ) return t.Name;

            // Try to resolve dynamic names
            if( _contextArguments.TryGetValue( key.ID, out int[] args ) && args.Length > 0 )
            {
                var argNames = args.Select( a => GetContextName( new ContextKey( a ) ) );
                return $"Ctx<{string.Join( ", ", argNames )}>";
            }

            return key.ID == 0 ? "Default" : $"Context_{key.ID}";
        }

        public static Type GetContextType( ContextKey key )
        {
            return _idToType.TryGetValue( key.ID, out Type t ) ? t : null;
        }

        /// <summary>
        /// Registers a rule that states: When in <paramref name="context"/>, the generic arguments are <paramref name="args"/>.
        /// </summary>
        public static void RegisterContextArguments( ContextKey context, params ContextKey[] args )
        {
            _contextArguments[context.ID] = args.Select( c => c.ID ).ToArray();
        }

        /// <summary>
        /// Gets the generic arguments associated with the context.
        /// Returns empty array if no rules exist (e.g. ObjectContext.Default).
        /// </summary>
        public static ContextKey[] GetContextArguments( ContextKey context )
        {
            if( _contextArguments.TryGetValue( context.ID, out int[] args ) )
            {
                var result = new ContextKey[args.Length];
                for( int i = 0; i < args.Length; i++ ) result[i] = new ContextKey( args[i] );
                return result;
            }
            return Array.Empty<ContextKey>();
        }

        /// <summary>
        /// Gets the unique ID for a context type.
        /// Automatically handles Generic Contexts (types implementing IContext) by registering new context rules on the fly.
        /// Enforces that Contexts must be Interfaces implementing IContext.
        /// </summary>
        public static ContextKey GetID( Type contextType )
        {
            if( contextType == null ) return ContextIDs.Default;

            // 1. Direct Cache / Legacy Fixed Mappings
            if( _typeToId.TryGetValue( contextType, out int id ) )
                return new ContextKey( id );

            // 2. Core Marker Aliasing (Inheritance)
            // Cache the result to avoid repeated IsAssignableFrom calls
            if( typeof( Ctx.Asset ).IsAssignableFrom( contextType ) )
            {
                Register( contextType, ContextIDs.Asset );
                return ObjectContext.Asset;
            }
            if( typeof( Ctx.Ref ).IsAssignableFrom( contextType ) )
            {
                Register( contextType, ContextIDs.Ref );
                return ObjectContext.Ref;
            }
            if( typeof( Ctx.Value ).IsAssignableFrom( contextType ) )
            {
                Register( contextType, ContextIDs.Default );
                return ObjectContext.Default;
            }

            // 3. Generic Interface Scanning (If the type is the interface definition itself)
            if( contextType.IsGenericType && typeof( Ctx.IContext ).IsAssignableFrom( contextType ) )
            {
                if( TryProcessGenericType( contextType, contextType, out id ) )
                {
                    Register( contextType, id );
                    return new ContextKey( id );
                }
            }

            var interfaces = contextType.GetInterfaces();

            // Pass 1: Prioritize User Defined Interfaces (Direct Implementation)
            foreach( var i in interfaces )
            {
                if( !typeof( Ctx.IContext ).IsAssignableFrom( i ) ) continue;
                if( i == typeof( Ctx.IContext ) ) continue; // Skip root

                if( i.IsGenericType )
                {
                    if( TryProcessGenericType( i, contextType, out id ) )
                        return new ContextKey( id );
                }
                else
                {
                    // Aliasing to non-generic interface (e.g. class MyContext : IMyContext)
                    ContextKey aliasId = GetID( i );
                    if( aliasId.ID != ContextIDs.Default )
                    {
                        Register( contextType, aliasId.ID );
                        return aliasId;
                    }
                }
            }

            // 4. Definition Check: Must be Interface and implement IContext
            if( contextType.IsInterface && typeof( Ctx.IContext ).IsAssignableFrom( contextType ) && contextType != typeof( Ctx.IContext ) )
            {
                id = _nextDynamicId++;
                Register( contextType, id );
                return new ContextKey( id );
            }

            // Invalid Type: Does not implement IContext, or is a Class not implementing IContext.
            return ContextIDs.Default;
        }

        private static bool TryProcessGenericType( Type genericInterface, Type originalType, out int id )
        {
            // Must be an IContext
            if( !typeof( Ctx.IContext ).IsAssignableFrom( genericInterface ) )
            {
                id = 0;
                return false;
            }

            Type genericDef = genericInterface.GetGenericTypeDefinition();
            Type[] typeArgs = genericInterface.GetGenericArguments();

            // If arguments are generic parameters (Open Generic definition), we can't recurse GetId on them.
            // e.g. typeof(Ctx.List<>)
            foreach( var arg in typeArgs )
            {
                if( arg.IsGenericParameter )
                {
                    id = 0;
                    return false; // Treat as Open Generic Definition -> Register as unique ID (Fallthrough to step 4)
                }
            }

            // Extract Context Args
            List<ContextKey> contextArgs = new List<ContextKey>();
            bool hasValidArgs = false;

            foreach( var arg in typeArgs )
            {
                // Only consider arguments that are Contexts
                if( typeof( Ctx.IContext ).IsAssignableFrom( arg ) )
                {
                    contextArgs.Add( GetID( arg ) );
                    hasValidArgs = true;
                }
            }

            if( hasValidArgs )
            {
                id = GetOrRegisterGenericContext( genericDef, contextArgs.ToArray(), originalType ).ID;
                return true;
            }

            // If we are inspecting the interface itself (e.g. IMyContext<T>), and it's closed (IMyContext<int>),
            // but T isn't a context, we just register it normally via Step 4.

            if( originalType == genericInterface )
            {
                id = 0;
                return false;
            }

            // If a class implements IContext directly without generic context args, alias it to the interface ID
            id = GetID( genericInterface ).ID;
            return true;
        }

        /// <summary>
        /// Gets an ID for a generic context combination. If it doesn't exist, mints one and registers the rules.
        /// </summary>
        public static ContextKey GetOrRegisterGenericContext( Type genericDefinition, ContextKey[] args, Type sourceContextType = null )
        {
            // Create a stable key for the combination: "TypeName[Arg1,Arg2,...]"
            StringBuilder keyBuilder = new StringBuilder();
            keyBuilder.Append( genericDefinition.AssemblyQualifiedName );
            keyBuilder.Append( '[' );
            for( int i = 0; i < args.Length; i++ )
            {
                if( i > 0 )
                    keyBuilder.Append( ',' );
                keyBuilder.Append( args[i].ID );
            }
            keyBuilder.Append( ']' );

            // Can be optimized by using integer IDs in an array?
            string key = keyBuilder.ToString();

            if( !_genericCombinations.TryGetValue( key, out int id ) )
            {
                id = _nextDynamicId++;
                _genericCombinations[key] = id;

                // Register the rule immediately
                RegisterContextArguments( new ContextKey( id ), args );
            }

            // Map the specific source type to this ID so subsequent lookups are fast O(1) map hits
            if( sourceContextType != null )
            {
                Register( sourceContextType, id );
            }

            return new ContextKey( id );
        }
    }
}