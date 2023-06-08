using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization
{
    /// <summary>
    /// Manages the task of serialization.
    /// </summary>
    public class Saver
    {
        // Core ideas:
        /*
        
        Saving is split into 2 main steps:
        
        1. Save object data.
            Loop through every object and save its persistent data.
            When serializing a reference, ask what the ID of that object is by passing the object to the `RegisterOrGet` method.

        2. Save referencable objects.
            Loop through these objects again, and save them, along with their IDs (if referenced by anything).
            Use the object registry to get the IDs of objects that have been assigned to them in step 1.

        Benefit: We know what is referenced before we save the objects (all of which could potentially be referenced).

        */

        /// <summary>
        /// Specifies where to save the data.
        /// </summary>
        public string SaveDirectory { get; set; }

        List<Action<Saver>> _saveActions = new List<Action<Saver>>();

        Dictionary<object, Guid> _objectToGuid = new Dictionary<object, Guid>();

        // ---

        public Saver( string saveDirectory )
        {
            this.SaveDirectory = saveDirectory;
        }

        public Saver( string saveDirectory, params Action<Saver>[] OnSave )
        {
            this.SaveDirectory = saveDirectory;

            foreach( var action in OnSave )
            {
                this._saveActions.Add( action );
            }
        }

        // ---

        private void ClearReferenceRegistry()
        {
            _objectToGuid.Clear();
        }

        /// <summary>
        /// Registers the specified object in the registry (if not registered already) and returns its reference ID.
        /// </summary>
        /// <remarks>
        /// Call this to map an object to an ID when saving an object reference.
        /// </remarks>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Guid RegisterOrGet( object obj )
        {
            if( _objectToGuid.TryGetValue( obj, out Guid id ) )
            {
                return id;
            }

            Guid newID = Guid.NewGuid();
            _objectToGuid.Add( obj, newID );
            return newID;
        }

        // ---

        /// <summary>
        /// Performs a save to the current path, and with the current save actions.
        /// </summary>
        public void Save()
        {
            ClearReferenceRegistry();

            foreach( var action in _saveActions )
            {
                // scene saving/loading via custom serializer set.
                // Could also be used to save something else than the scene, e.g. current dialogues, by specifying a different set of save actions.
                action?.Invoke( this );
            }

            ClearReferenceRegistry();
        }


        // save data (with things that can be referenced)
        // save referencable objects (gameobjects, additional eg dialogues).
        // - save ID if thing is in cache.


        // load referencable objects
        // load data (with references) once referencables are loaded.


        // dialogues are maps of references between dialogue options. load options first, then load dialogues. this separation *can* be done inside a single load action.
    }
}