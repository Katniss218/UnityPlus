using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization.Scene
{
    /// <summary>
    /// Can be used to save the scene using the factory-objectdata scheme.
    /// </summary>
    public static class FactoryObjectDataSceneSaver
    {
        public static void SaveObjects( Saver s )
        {
            // saves the information about what exists and what factory can be used to create that thing.
        }

        public static void SaveObjectData( Saver s )
        {
            // saves the persistent information about the existing objects.

            // persistent information is one that is expected to change and be preserved (i.e. health, inventory, etc).
        }
    }
}