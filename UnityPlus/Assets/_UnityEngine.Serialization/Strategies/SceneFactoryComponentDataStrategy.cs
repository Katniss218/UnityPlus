using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Serialization.ComponentData;
using UnityEngine.Serialization.Factories;

namespace UnityEngine.Serialization.Strategies
{
    /// <summary>
    /// Can be used to save the scene using the factory-objectdata scheme.
    /// </summary>
    public class SceneFactoryComponentDataStrategy
    {
        // Object actions are suffixed by _Object
        // Data actions are suffixed by _Data

        public string ID_STRING = "$id";

        private static IEnumerable<GameObject> GetRootGameObjects()
        {
            return SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        public void SaveSceneObjects_Object( Saver s )
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

        public void SaveSceneObjects_Data( Saver s )
        {
            // saves the persistent information about the existing objects.

            // persistent information is one that is expected to change and be preserved (i.e. health, inventory, etc).

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            // loop through children.

            // loop through components.

            // save.
        }

        public void LoadSceneObjects_Object( Loader l )
        {
            // Assumes that factories are already registered.

            // create dummy GOs with factories.
        }

        public void LoadSceneObjects_Data( Loader l )
        {
            // loop through object data, get the corresponding objects using ID from registry, and apply.
        }




        // part of serialization.

        /// <summary>
        /// Applies this data to a specified gameobject.
        /// </summary>
        public static void ApplyTo( GameObjectData data, GameObject target )
        {
            Component[] components = target.GetComponents<Component>();

            foreach( var predDataPair in data.Data )
            {
                GameObjectData.Predicates[predDataPair.p.name]( components, predDataPair.p.data );
            }
        }

        public static GameObjectData CreateFrom( GameObject target )
        {
            throw new Exception();
        }
    }
}