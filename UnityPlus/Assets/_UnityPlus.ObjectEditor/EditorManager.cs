using UnityEngine;

namespace UnityPlus.ObjectEditor
{
    public class EditorManager : SingletonMonoBehaviour<EditorManager>
    {
        // editor edits OBJECTS (dotnet instances), not serializeddata.
        // - it should use serialization v4's mappings to define which members are drawn
        // - so the mapping itself should just be a generic 'accessor' to an object.
        // - for serialization, the serialization unit itself manages the serialization process. Move the serialization logic from the mappings to the centralized thing.
        // writing involves serializeddata

        // uses standard UILib elements to access/manage convertion to/from string, etc.


        // every access happens through a mapping.



        // undo/redo
        // - also through a mapping.






        // a mapping defines what 'actions' are possible to perform on a type, and allows to perform them.



    }
}