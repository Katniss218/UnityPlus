using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Serialization.Factories
{
    public interface IFactory : IPersistent
    {
        /// <summary>
        /// ID of the factory. <br />
        /// Can be used to register the factory using the <see cref="Register"/> method. <br />
        /// Can be used to look the factory up using the <see cref="Get"/> method.
        /// </summary>
        string ID { get; }
    }

    public interface IFactory<T> : IFactory
    {
        /// <summary>
        /// Creates the object using the factory parameters.
        /// </summary>
        T Create();
    }
    /*
    public static class FactoryRegistry
    {
        static Dictionary<string, object> _factories = new Dictionary<string, object>();

        /// <summary>
        /// Registers a new factory with the ID specified by the factory instance.
        /// </summary>
        /// <param name="factory">The factory instance to register.</param>
        public static void Register<T>( IFactory<T> factory )
        {
            _factories.Add( factory.ID, factory );
        }

        /// <summary>
        /// Retrieves a registered factory type by the provided factory ID.
        /// </summary>
        /// <returns>The factory instance in the registry (reference).</returns>
        public static IFactory<T> Get<T>( string factoryID )
        {
            if( _factories.TryGetValue( factoryID, out object factory ) )
            {
                return factory as IFactory<T>;
            }
            return null;
        }
    }*/
}