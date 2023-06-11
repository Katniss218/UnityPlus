using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.Serialization.Factories
{
    public interface IFactory<T> : IPersistent
    {
        /// <summary>
        /// Creates the object using the factory's parameters.
        /// </summary>
        T Create();
    }
}