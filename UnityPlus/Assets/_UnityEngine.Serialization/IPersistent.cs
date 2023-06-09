using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization
{
    /// <summary>
    /// Inherit from this to specify that your class handles assigning persistent data for itself.
    /// </summary>
    public interface IPersistent
    {
        /// <summary>
        /// Sets the persistent data after creating the object with default parameters.
        /// </summary>
        void SetData( JToken json );

        /// <summary>
        /// Gets the persistent data from an object.
        /// </summary>
        JToken GetData();
    }
}