using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.Serialization
{
    public class Saver
    {
        /// <summary>
        /// Specifies where to save the data.
        /// </summary>
        public string FilePath { get; set; }


        // Save actions.

        List<Action<Saver>> _onPreSave = new List<Action<Saver>>();

        List<Action<Saver>> _onSave = new List<Action<Saver>>();

        List<Action<Saver>> _onPostSave = new List<Action<Saver>>();


        // Object registry (stores identifiers of objects to preserve references).

        Dictionary<object, Guid> objectToGuid = new Dictionary<object, Guid>();
        Dictionary<Guid, object> guidToObject = new Dictionary<Guid, object>();

        // ---

        public Saver( params Action<Saver>[] OnSave )
        {
            foreach( var action in OnSave )
            {
                this._onSave.Add( action );
            }
        }

        private void ClearObjectRegistry()
        {
            objectToGuid.Clear();
            guidToObject.Clear();
        }

        /// <summary>
        /// Performs a save to the current path, and with the current save actions.
        /// </summary>
        public void Save()
        {
            ClearObjectRegistry();

            foreach( var preSave in _onPreSave )
            {
                preSave?.Invoke( this );
            }

            foreach( var save in _onSave )
            {
                // scene saving/loading via custom serializer set.
                // Could also be used to save something else than the scene, e.g. current dialogues, by specifying a different set of save actions.
                save?.Invoke( this );
            }
            
            foreach( var postSave in _onPostSave )
            {
                postSave?.Invoke( this );
            }

            ClearObjectRegistry();
        }
    }
}