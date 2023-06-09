using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Serialization.Factories;

namespace UnityEngine.Serialization.Strategies
{
    /// <summary>
    /// Can be used to save the scene using the factory-objectdata scheme.
    /// </summary>
    public static class FactoryObjectData
    {
        public static string ID_STRING = "$id";

        public static Saver GetDefaultSaver( string directory )
        {
            return new Saver( directory, new Action<Saver>[] { SaveSceneObjects }, new Action<Saver>[] { SaveSceneObjectData } );
        }

        private static IEnumerable<GameObject> GetRootGameObjects()
        {
            return SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        public static void SaveSceneObjects( Saver s )
        {
            // saves the information about what exists and what factory can be used to create that thing.

            // this should save to a file. to a specified dir.

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            JArray objectsJson = new JArray();

            foreach( var go in rootObjects )
            {
                Guid id = s.GetID( go );
                CreatedByFactory cbf = go.GetComponent<CreatedByFactory>();
                if( cbf == null)
                {
                    continue;
                }
                string factoryID = cbf.GetFactoryID();

                JObject goJson = new JObject()
                {
                    { ID_STRING, id.ToString("X") },
                    { "$factory", factoryID }
                };
                objectsJson.Add( goJson );
            }
        }

        public static void SaveSceneObjectData( Saver s )
        {
            // saves the persistent information about the existing objects.

            // persistent information is one that is expected to change and be preserved (i.e. health, inventory, etc).

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            // loop through children.

            // loop through components.

            // save.
        }

        public static void LoadSceneObjects( Loader l )
        {
            // create dummy GOs with factories.
        }

        public static void LoadSceneObjectData( Loader l )
        {
            // loop through 
        }
    }
}