using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AssetManagement;
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


        private static string jsonO;
        private static string jsonD;

        private static IEnumerable<GameObject> GetRootGameObjects()
        {
            return SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        public static JToken WriteFactoryObject( Saver s, GameObject go )
        {
            CreatedByFactory cbf = go.GetComponent<CreatedByFactory>();
            if( cbf == null )
            {
                return null;
            }

            Guid objectGuid = s.GetID( go );
            string factoryID = cbf.FactoryAssetID;

            JObject goJson = new JObject()
            {
                { Saver_Ex_References.ID, s.WriteGuid(objectGuid) },
                { "$factory", factoryID }
            };

            return goJson;
        }

        public static GameObject ReadFactoryObject( Loader l, JToken goJson )
        {
            Guid objectGuid = l.ReadGuid( goJson[Saver_Ex_References.ID] );

            string factoryID = (string)goJson["$factory"];

            IFactory<GameObject> fac = Registry.Get<IFactory<GameObject>>( factoryID );

            if( fac == null )
            {
                Debug.LogWarning( $"Couldn't find a factory with asset ID `{factoryID}`." );
            }

            GameObject go = fac.Create();

            l.SetID( go, objectGuid );

            return go;
        }

        public void SaveSceneObjects_Object( Saver s )
        {
            // saves the information about what exists and what factory can be used to create that thing.

            // this should save to a file. to a specified dir.

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            JArray objectsJson = new JArray();

            foreach( var go in rootObjects )
            {
#warning TODO - if root doesn't have factory component, look through children.
                // maybe some sort of customizable tag/layer masking

                JToken goJson = WriteFactoryObject( s, go );
                if( goJson == null )
                    continue;
                objectsJson.Add( goJson );
            }

            jsonO = JsonConvert.SerializeObject( objectsJson );
            Debug.Log( jsonO );
        }

        public void SaveSceneObjects_Data( Saver s )
        {
            // saves the persistent information about the existing objects.

            // persistent information is one that is expected to change and be preserved (i.e. health, inventory, etc).

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            JArray objectsJson = new JArray();

#warning TODO - loop through children.
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

            jsonD = JsonConvert.SerializeObject( objectsJson );
            Debug.Log( jsonD );
        }

        public void LoadSceneObjects_Object( Loader l )
        {
            // Assumes that factories are already registered.

            // create dummy GOs with factories.

            JArray objectsJson = JsonConvert.DeserializeObject<JArray>( jsonO );

            foreach( var goJson in objectsJson )
            {
                ReadFactoryObject( l, goJson );
            }
        }

        public void LoadSceneObjects_Data( Loader l )
        {
            // loop through object data, get the corresponding objects using ID from registry, and apply.

            JArray objectsJson = JsonConvert.DeserializeObject<JArray>( jsonD );

            throw new Exception( "TODO - finish this" );
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