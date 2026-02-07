using System;

namespace UnityPlus.Serialization
{
    namespace Contexts
    {
        /// <summary>
        /// Base interface for all context marker types.
        /// </summary>
        public interface IContext { }

        // --- Core Markers ---
        public interface Default : IContext { }
        public interface Asset : IContext { }
        public interface Reference : IContext { }

        // --- Generics ---

        /// <summary>
        /// Creates a context for a Dictionary where Keys use TKeyContext and Values use TValueContext.
        /// Implement this interface on a class to create a shorthand alias.
        /// </summary>
        public interface CtxDict<TKeyContext, TValueContext> : IContext where TKeyContext : IContext where TValueContext : IContext { }

        /// <summary>
        /// Creates a context for a Collection (List/Array) where elements use TElementContext.
        /// Implement this interface on a class to create a shorthand alias.
        /// </summary>
        public interface CtxCollection<TElementContext> : IContext where TElementContext : IContext { }
    }

    /// <summary>
    /// General contexts applicable to any object type.
    /// Includes backward compatibility constants and Type-to-Int mapping initialization.
    /// </summary>
    public static class ObjectContext
    {
        public const int Default = 0;
        public const int Value = Default;
        public const int Ref = 536806356;
        public const int Asset = 271721118;

        static ObjectContext()
        {
            // Map the Types to the Legacy Ints
            ContextRegistry.RegisterMapping( typeof( Contexts.Default ), Default );
            ContextRegistry.RegisterMapping( typeof( Contexts.Asset ), Asset );
            ContextRegistry.RegisterMapping( typeof( Contexts.Reference ), Ref );
        }
    }

    public static class ArrayContext
    {
        public const int Default = ObjectContext.Default;
        public const int Values = Default;
        public const int Refs = 429303064;
        public const int Assets = -261997342;

        static ArrayContext()
        {
            // Map legacy array contexts to the new Unified Collection Context type
            ContextRegistry.RegisterMapping( typeof( Contexts.CtxCollection<Contexts.Reference> ), Refs );
            ContextRegistry.RegisterMapping( typeof( Contexts.CtxCollection<Contexts.Asset> ), Assets );
        }
    }

    public static class KeyValueContext
    {
        // Legacy constants kept for compatibility, but internally mapped via CtxDict<,> logic if used.
        public const int Default = ObjectContext.Default;
        public const int ValueToValue = Default;

        public const int ValueToRef = 846497468;
        public const int RefToValue = 132031121;
        public const int RefToRef = 240733231;
        public const int ValueToAsset = 172983851;
        public const int RefToAsset = -526574830;

        static KeyValueContext()
        {
            // We map the legacy combination IDs to the equivalent Generic Type combination.

            ContextRegistry.RegisterMapping( typeof( Contexts.CtxDict<Contexts.Default, Contexts.Reference> ), ValueToRef );
            ContextRegistry.RegisterMapping( typeof( Contexts.CtxDict<Contexts.Reference, Contexts.Default> ), RefToValue );
            ContextRegistry.RegisterMapping( typeof( Contexts.CtxDict<Contexts.Reference, Contexts.Reference> ), RefToRef );
            ContextRegistry.RegisterMapping( typeof( Contexts.CtxDict<Contexts.Default, Contexts.Asset> ), ValueToAsset );
            ContextRegistry.RegisterMapping( typeof( Contexts.CtxDict<Contexts.Reference, Contexts.Asset> ), RefToAsset );
        }
    }

    /// <summary>
    /// Helper to generate stable Context IDs for Dictionary Key/Value pairs.
    /// </summary>
    public struct DictionaryContext
    {
        public int KeyContext;
        public int ValueContext;

        public DictionaryContext( int keyContext, int valueContext )
        {
            KeyContext = keyContext;
            ValueContext = valueContext;
        }

        public DictionaryContext( Type keyContext, Type valueContext )
        {
            KeyContext = ContextRegistry.GetId( keyContext );
            ValueContext = ContextRegistry.GetId( valueContext );
        }

        public int GetId()
        {
            return ContextRegistry.GetOrRegisterDictionaryContext( KeyContext, ValueContext );
        }
    }
}
