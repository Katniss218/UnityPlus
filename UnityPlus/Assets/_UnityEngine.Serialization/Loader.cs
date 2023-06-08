using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization
{
    /// <summary>
    /// Managed the task of deserialization.
    /// </summary>
    public class Loader
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
        /// Specifies where to save the data.
        /// </summary>
        public string SaveDirectory { get; set; }

        List<Action<Loader>> _loadActions = new List<Action<Loader>>();

        Dictionary<Guid, object> _guidToObject = new Dictionary<Guid, object>();

        // ---

        public Loader( string saveDirectory )
        {
            this.SaveDirectory = saveDirectory;
        }

        public Loader( string saveDirectory, params Action<Loader>[] OnLoad )
        {
            this.SaveDirectory = saveDirectory;

            foreach( var action in OnLoad )
            {
                this._loadActions.Add( action );
            }
        }

        // ---

        private void ClearReferenceRegistry()
        {
            _guidToObject.Clear();
        }

        /// <summary>
        /// Registers the specified object with the specified ID.
        /// </summary>
        /// <remarks>
        /// Call this method when loading an object that might be referenced.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Register( object obj, Guid id )
        {
            _guidToObject.Add( id, obj );
        }

        /// <summary>
        /// Returns the previously registered object.
        /// </summary>
        /// <remarks>
        /// Call this method to deserialize a previously loaded object reference.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public object Get( Guid id )
        {
            if( _guidToObject.TryGetValue( id, out object obj ) )
            {
                return obj;
            }
#if DEBUG
            Debug.Log( $"Tried to get a reference to object `{id:X}` before it was loaded." );
#endif
            return null;
        }

        // ---

        /// <summary>
        /// Performs a save to the current path, and with the current save actions.
        /// </summary>
        public void Load()
        {
            ClearReferenceRegistry();

            foreach( var action in _loadActions )
            {
                // scene saving/loading via custom serializer set.
                // Could also be used to save something else than the scene, e.g. current dialogues, by specifying a different set of save actions.
                action?.Invoke( this );
            }

            ClearReferenceRegistry();
        }
    }
}