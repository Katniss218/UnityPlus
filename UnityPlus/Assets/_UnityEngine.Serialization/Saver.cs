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

        /// <summary>
        /// The set of actions to perform before saving.
        /// </summary>
        public event Action<Saver> OnWarmup;

        /// <summary>
        /// The set of actions to perform to save the scene.
        /// </summary>
        public event Action<Saver> OnSave;

        /// <summary>
        /// The set of actions to perform after saving.
        /// </summary>
        public event Action<Saver> OnCleanup;


        // Object registry (stores identifiers of objects to preserve references).

        Dictionary<object, Guid> objectToGuid = new Dictionary<object, Guid>();
        Dictionary<Guid, object> guidToObject = new Dictionary<Guid, object>();

        // ---

        private void ClearObjectRegistry()
        {
            objectToGuid.Clear();
            guidToObject.Clear();
        }

        public void Save()
        {
            ClearObjectRegistry();

            OnWarmup?.Invoke( this );

            OnSave?.Invoke( this ); // scene saving/loading via custom serializer set.

            OnCleanup?.Invoke( this );

            ClearObjectRegistry();
        }
    }
}