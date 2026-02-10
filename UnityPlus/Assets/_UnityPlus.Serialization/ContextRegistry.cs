
using System;
using System.Collections.Generic;
using UnityPlus.Serialization.Contexts;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Maps context marker types to unique integer IDs for the duration of the execution of the application.
    /// Also manages the structural rules of contexts (e.g. A List in Context X contains Elements in Context Y).
    /// </summary>
    public static class ContextRegistry
    {
        private static readonly Dictionary<Type, int> _typeToId = new Dictionary<Type, int>();
        private static readonly Dictionary<int, Type> _idToType = new Dictionary<int, Type>(); // Reverse lookup for debugging

        // Start dynamic IDs high to avoid collision with legacy v3 constants
        private static int _nextDynamicId = 2000000;

        // Generic cache for Dictionary Combinations (KeyID, ValueID) -> ContextID
        private static readonly Dictionary<(int, int), int> _dictionaryCombinations = new Dictionary<(int, int), int>();
        private static readonly Dictionary<int, int> _collectionCombinations = new Dictionary<int, int>();

        // Structural Rules: Map a Container Context ID to its Element Context IDs
        private static readonly Dictionary<int, int> _collectionContextRules = new Dictionary<int, int>();
        private static readonly Dictionary<int, (int keyCtx, int valCtx)> _dictionaryContextRules = new Dictionary<int, (int, int)>();

        /// <summary>
        /// Registers a fixed mapping between a Type and an ID. 
        /// Used for v3 backward compatibility.
        /// </summary>
        public static void RegisterMapping( Type type, int id )
        {
            if( type == null ) return;
            _typeToId[type] = id;
            _idToType[id] = type;
        }

        /// <summary>
        /// Registers a rule that states: When a Collection is in <paramref name="containerContext"/>, 
        /// its elements should be serialized in <paramref name="elementContext"/>.
        /// </summary>
        public static void RegisterCollectionRule( int containerContext, int elementContext )
        {
            _collectionContextRules[containerContext] = elementContext;
        }

        /// <summary>
        /// Registers a rule that states: When a Dictionary is in <paramref name="containerContext"/>, 
        /// its keys/values should be serialized in <paramref name="keyContext"/>/<paramref name="valueContext"/>.
        /// </summary>
        public static void RegisterDictionaryRule( int containerContext, int keyContext, int valueContext )
        {
            _dictionaryContextRules[containerContext] = (keyContext, valueContext);
        }

        public static int GetCollectionElementContext( int containerContext )
        {
            return _collectionContextRules.TryGetValue( containerContext, out int ctx ) ? ctx : containerContext;
        }

        public static (int keyCtx, int valCtx) GetDictionaryElementContexts( int containerContext )
        {
            return _dictionaryContextRules.TryGetValue( containerContext, out var ctx ) ? ctx : (ObjectContext.Default, ObjectContext.Default);
        }

        /// <summary>
        /// Gets the unique ID for a context type.
        /// Automatically handles `CtxDict<K,V>` and `CtxCollection<T>` generic types (and types implementing them) by registering new context rules on the fly.
        /// </summary>
        public static int GetId( Type contextType )
        {
            if( contextType == null ) return 0; // Default

            // 1. Direct Cache / Legacy Fixed Mappings
            if( _typeToId.TryGetValue( contextType, out int id ) )
                return id;

            // 2. Core Marker Aliasing (Inheritance)
            // If a custom type implements Asset/Reference/Default, reuse the core ID.
            if( typeof( Asset ).IsAssignableFrom( contextType ) ) return ObjectContext.Asset;
            if( typeof( Reference ).IsAssignableFrom( contextType ) ) return ObjectContext.Ref;
            if( typeof( Default ).IsAssignableFrom( contextType ) ) return ObjectContext.Default;

            // 3. Generic Interface Scanning (CtxDict / CtxCollection)
            // This handles direct generics (CtxCollection<Asset>) AND user types implementing them (MyList : CtxCollection<Asset>)

            // Check the type itself (if it's the generic interface directly)
            if( contextType.IsGenericType )
            {
                if( TryProcessGenericType( contextType, contextType, out id ) )
                    return id;
            }

            // Check implemented interfaces
            foreach( var i in contextType.GetInterfaces() )
            {
                if( i.IsGenericType )
                {
                    if( TryProcessGenericType( i, contextType, out id ) )
                        return id;
                }
            }

            // 4. Unknown type: Assign new unique ID
            id = _nextDynamicId++;
            _typeToId[contextType] = id;
            _idToType[id] = contextType;

            return id;
        }

        private static bool TryProcessGenericType( Type genericInterface, Type originalType, out int id )
        {
            Type genericDef = genericInterface.GetGenericTypeDefinition();
            Type[] args = genericInterface.GetGenericArguments();

            // Handle CtxDict<KeyCtx, ValCtx>
            if( genericDef == typeof( CtxDict<,> ) )
            {
                int keyCtxId = GetId( args[0] );
                int valCtxId = GetId( args[1] );
                id = GetOrRegisterDictionaryContext( keyCtxId, valCtxId, originalType );
                return true;
            }

            // Handle CtxCollection<ElemCtx>
            if( genericDef == typeof( CtxCollection<> ) )
            {
                int elementCtxId = GetId( args[0] );
                id = GetOrRegisterCollectionContext( elementCtxId, originalType );
                return true;
            }

            id = 0;
            return false;
        }

        /// <summary>
        /// Gets an ID for a dictionary combination. If it doesn't exist, mints one and registers the rule.
        /// </summary>
        public static int GetOrRegisterDictionaryContext( int keyContextId, int valueContextId, Type sourceContextType = null )
        {
            // This only needs to be stable across a single execution of the app, not across executions, but it *needs* to be stable.

            if( keyContextId == 0 && valueContextId == 0 ) return 0;

            var key = (keyContextId, valueContextId);
            if( !_dictionaryCombinations.TryGetValue( key, out int id ) )
            {
                id = _nextDynamicId++;
                _dictionaryCombinations[key] = id;

                // Register the rule immediately
                RegisterDictionaryRule( id, keyContextId, valueContextId );
            }

            // Map the specific source type to this ID so subsequent lookups are fast O(1) map hits
            if( sourceContextType != null )
            {
                RegisterMapping( sourceContextType, id );
            }

            return id;
        }

        /// <summary>
        /// Gets an ID for a collection (List/Array) combination. If it doesn't exist, mints one and registers the rule.
        /// </summary>
        public static int GetOrRegisterCollectionContext( int elementContextId, Type sourceContextType = null )
        {
            if( elementContextId == 0 ) return 0;

            if( !_collectionCombinations.TryGetValue( elementContextId, out int id ) )
            {
                id = _nextDynamicId++;
                _collectionCombinations[elementContextId] = id;

                // Register the rule immediately
                RegisterCollectionRule( id, elementContextId );
            }

            if( sourceContextType != null )
            {
                RegisterMapping( sourceContextType, id );
            }

            return id;
        }
    }
}