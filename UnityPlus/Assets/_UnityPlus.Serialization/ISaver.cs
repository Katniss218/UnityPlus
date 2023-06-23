using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Manages the task of serialization.
    /// </summary>
    public interface ISaver
    {
        // Core ideas:
        /*
        
        Saving is split into 2 main steps:
        
        1. Save object data.
            Loop through every object and save its persistent data.
            When serializing a reference, ask what the ID of that object is by passing the object to the `RegisterOrGet` method.

        2. Save object instances (references).
            Loop through these objects again, and save them, along with their IDs (if referenced by anything).
            Use the object registry to get the IDs of objects that have been assigned to them in step 1.

        Benefit: We know what is referenced before we save the objects (all of which could potentially be referenced).

        */

        bool TryGetID( object obj, out Guid id );

        /// <summary>
        /// Registers the specified object in the registry (if not registered already) and returns its reference ID.
        /// </summary>
        /// <remarks>
        /// Call this to map an object to an ID when saving an object reference.
        /// </remarks>
        Guid GetID( object obj );
    }
}