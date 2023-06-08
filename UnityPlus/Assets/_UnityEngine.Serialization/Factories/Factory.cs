using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization.Factories
{
    public abstract class Factory<T> where T : UnityEngine.Object
    {
        /// <summary>
        /// ID of the factory. Can be used to look up the factory in the factory registry.
        /// </summary>
        public string ID { get; protected set; }
    }
}