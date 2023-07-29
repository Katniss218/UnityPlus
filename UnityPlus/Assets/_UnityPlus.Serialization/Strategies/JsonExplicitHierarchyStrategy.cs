using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPlus.AssetManagement;
using UnityPlus.Serialization.ComponentData;

namespace UnityPlus.Serialization.Strategies
{
    /// <summary>
    /// Can be used to save the scene using the factory-gameobjectdata scheme.
    /// </summary>
    [Obsolete( "Incomplete" )]
    public sealed class JsonExplicitHierarchyStrategy
    {
        // Object actions are suffixed by _Object
        // Data actions are suffixed by _Data

        private static string jsonO;
        private static string jsonD;

        public int IncludedObjectsMask { get; set; } = int.MaxValue;

        private static IEnumerable<GameObject> GetRootGameObjects()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        private void WriteGameObjectHierarchy( GameObject go, ISaver s, ref SerializedArray arr )
        {
            if( !go.IsInLayerMask( IncludedObjectsMask ) )
            {
                return;
            }

            Guid objectGuid = s.GetID( go );

            // recursive.
            SerializedObject obj = new SerializedObject()
            {
                { "$id", s.WriteGuid(objectGuid) }
            };

            SerializedArray children = new SerializedArray();

            foreach( Transform child in go.transform )
            {
                WriteGameObjectHierarchy( child.gameObject, s, ref children );
            }

            SerializedArray components = new SerializedArray();

            foreach( var comp in go.GetComponents<Component>() )
            {
                Guid id = s.GetID( comp );
                SerializedObject compObj = new SerializedObject()
                {
                    { "$id", s.WriteGuid(id) },
                    { "$type", s.WriteType(comp.GetType()) }
                };

                components.Add( compObj );
            }

            obj.Add( "children", children );
            obj.Add( "components", components );

            arr.Add( obj );
        }

        public IEnumerator SaveSceneObjects_Object( ISaver s )
        {

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            SerializedArray objectsJson = new SerializedArray();

            foreach( var go in rootObjects )
            {
                // maybe some sort of customizable tag/layer masking

                WriteGameObjectHierarchy( go, s, ref objectsJson );

                yield return null;
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objectsJson, sb ).Write();
            jsonO = sb.ToString();

            Debug.Log( jsonO );
        }
        private static void SaveObjectDataRecursive( ISaver s, GameObject go, ref SerializedArray objects )
        {
            Guid id = s.GetID( go );

            SerializedArray components = new SerializedArray();

            // components' properties.
            Component[] comps = go.GetComponents();
            int i = 0;
            foreach( var comp in comps )
            {
                var dataJson = comp.GetData( s );

                if( dataJson != null )
                {
                    Guid cid = s.GetID( comp );
                    SerializedObject compData = new SerializedObject()
                    {
                        { "$ref", s.WriteGuid(cid) },
                        { "data", dataJson }
                    };
                    components.Add( compData );
                }
                i++;
            }

            // gameobject properties.
            if( components.Any() )
            {
                objects.Add( new SerializedObject()
                {
                    { "$ref", id.ToString( "D" ) },
                    { "name", go.name },
                    { "layer", go.layer },
                    { "is_active", go.activeSelf },
                    { "is_static", go.isStatic },
                    { "tag", go.tag },
                    { "components", components }
                } );
            }

            foreach( Transform ct in go.transform )
            {
                SaveObjectDataRecursive( s, ct.gameObject, ref objects );
            }
        }

        //public void SaveSceneObjects_Data( ISaver s )
        public IEnumerator SaveSceneObjects_Data( ISaver s )
        {
            // saves the persistent information about the existing objects.

            // persistent information is one that is expected to change and be preserved (i.e. health, inventory, etc).

            IEnumerable<GameObject> rootObjects = GetRootGameObjects();

            SerializedArray objData = new SerializedArray();

            foreach( var go in rootObjects )
            {
                ClonedGameObject cbf = go.GetComponent<ClonedGameObject>();
                if( cbf == null )
                {
                    continue;
                }
                yield return null;

                SaveObjectDataRecursive( s, go, ref objData );
            }

            var sb = new StringBuilder();
            new Serialization.Json.JsonStringWriter( objData, sb ).Write();
            jsonD = sb.ToString();

            //TMPro.TMP_InputField inp = UnityEngine.Object.FindObjectOfType<TMPro.TMP_InputField>();
            //inp.text = jsonD;
            Debug.Log( jsonD );
        }

        public IEnumerator LoadSceneObjects_Object( ILoader l )
        {
            throw new Exception();
        }

        public IEnumerator LoadSceneObjects_Data( ILoader l )
        {
            // same as prefab+data strat
            throw new Exception();
        }
    }
}