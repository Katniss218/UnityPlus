using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Serialization.Factories
{
    /// <summary>
    /// Holds the references to all factories, so that their type is not required to fully identify them.
    /// </summary>
    internal static class FactoryRegistry
    {
        internal static Dictionary<string, object> _factories = new Dictionary<string, object>();
    }

    /// <summary>
    /// Inherit from this class to create a factory type.
    /// </summary>
    /// <typeparam name="T">The type of objects that this factory creates.</typeparam>
    public abstract class Factory<T> : IPersistent
    {
        /// <summary>
        /// Registers a new factory with the ID specified by the factory instance.
        /// </summary>
        /// <param name="factory">The factory instance to register.</param>
        public static void Register( Factory<T> factory )
        {
            FactoryRegistry._factories.Add( factory.ID, factory );
        }

        /// <summary>
        /// Retrieves a registered factory type by the provided factory ID.
        /// </summary>
        /// <returns>The factory instance in the registry (reference).</returns>
        public static Factory<T> Get( string factoryID )
        {
            if( FactoryRegistry._factories.TryGetValue( factoryID, out object factory ) )
            {
                return factory as Factory<T>;
            }
            return null;
        }

        /// <summary>
        /// ID of the factory. <br />
        /// Can be used to register the factory using the <see cref="Register"/> method. <br />
        /// Can be used to look the factory up using the <see cref="Get"/> method.
        /// </summary>
        public string ID { get; protected set; }

        /// <summary>
        /// Creates the object using the factory parameters.
        /// </summary>
        public abstract T Create();

        public abstract void SetData( JToken json );

        public abstract JToken GetData();
    }
}