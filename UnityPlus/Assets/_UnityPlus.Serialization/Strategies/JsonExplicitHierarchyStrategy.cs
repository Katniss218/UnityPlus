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

        private static IEnumerable<GameObject> GetRootGameObjects()
        {
            return UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        }

        public IEnumerator SaveSceneObjects_Object( ISaver s )
        {

            throw new Exception();
        }

        public IEnumerator SaveSceneObjects_Data( ISaver s )
        {

            throw new Exception();
        }

        public IEnumerator LoadSceneObjects_Object( ILoader l )
        {

            throw new Exception();
        }

        public IEnumerator LoadSceneObjects_Data( ILoader l )
        {

            throw new Exception();
        }
    }
}