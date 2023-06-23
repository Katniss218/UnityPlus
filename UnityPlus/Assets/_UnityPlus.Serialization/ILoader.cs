using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityPlus.Serialization
{
    /// <summary>
    /// Managed the task of deserialization.
    /// </summary>
    public interface ILoader
    {
        // Core ideas:
        /*
        
        Loading is split into 2 main steps:
        
        1. Creation of referencable objects.
            These objects will have default parameters, can can be created by a factory, or a number of other methods.
            This step includes deserializing other save-specific items, such as dialogues (if applicable).

        2. Applying data to the created objects. 
            After every referencable object has been created, we can load the things that reference them. In practice, this means we apply *all* data after everything has been created.

        Benefit: When a reference is deserialized, the object that it refers to is already created. 
        Benefit: We don't have to store the information about how to load the referenced object in the reference.

        Loading a scene when a previous scene is already loaded is not a concern of this class. The user should unload it first.

        */

        /// <summary>
        /// Registers the specified object with the specified ID.
        /// </summary>
        /// <remarks>
        /// Call this method when loading an object that might be referenced.
        /// </remarks>
        void SetID( object obj, Guid id );

        /// <summary>
        /// Returns the previously registered object.
        /// </summary>
        /// <remarks>
        /// Call this method to deserialize a previously loaded object reference.
        /// </remarks>
        public object Get( Guid id );
    }
}