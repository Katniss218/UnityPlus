using Newtonsoft.Json;
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
    /// Can be used to save the scene using the factory-gameobjectdata scheme.
    /// </summary>
    public class SceneFactoryGameObjectDataStrategy
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
                CreatedByFactory cbf = go.GetComponent<CreatedByFactory>();
                if( cbf == null )
                {
                    continue;
                }
                Guid id = s.GetID( go );
                string factoryID = cbf.FactoryAssetID;

                JObject goJson = new JObject()
                {
                    { ID_STRING, id.ToString("D") },
                    { "$factory", factoryID }
                };
                objectsJson.Add( goJson );
            }

            Debug.Log( JsonConvert.SerializeObject( objectsJson ) );
        }

        public void SaveSceneObjects_Data( Saver s )
        {
            // saves the persistent information about the existing objects.

            // persistent information is one that is expected to change and be preserved (i.e. health, inventory, etc).

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            JArray objectsJson = new JArray();

            foreach( var go in rootObjects )
            {
                CreatedByFactory cbf = go.GetComponent<CreatedByFactory>();
                if( cbf == null )
                {
                    continue;
                }
                Guid id = s.GetID( go );

                JArray componentsJson = new JArray();

                Component[] comps = go.GetComponents<Component>();
                int i = 0;
                foreach( var comp in comps )
                {
                    var dataJson = comp.GetData( s );

                    if( dataJson != null )
                    {
                        JObject compJson = new JObject()
                        {
#warning TODO - ugly magic string value of `predicate_type`.
                            { "predicate_type", "index" },
                            { "predicate", new JObject()
                            {
                                { "index", i }
                            } },
                            { "data", dataJson }
                        };
                        componentsJson.Add( compJson );
                    }
                    i++;
                }

                if( componentsJson.Any() )
                {
                    objectsJson.Add( new JObject()
                    {
                        { "$ref", id.ToString( "D" ) },
                        { "components", componentsJson }
                    } );
                }
            }

            Debug.Log( JsonConvert.SerializeObject( objectsJson ) );
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
        static void ApplyTo( GameObjectData data, GameObject target )
        {
            Component[] components = target.GetComponents<Component>();

            foreach( var predDataPair in data.Data )
            {
                GameObjectData.PredicateRegistry[predDataPair.p.name]( components, predDataPair.p.data );
            }
        }

        static GameObjectData CreateFrom( GameObject target )
        {
            throw new Exception();
        }
    }
}