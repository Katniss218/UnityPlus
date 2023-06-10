using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Serialization.ComponentData
{
    /// <summary>
    /// A way to apply persistent data to a gameobject.
    /// </summary>
    public class GameObjectData
    {
        /// <summary>
        /// A predicate is used to find a specific component.
        /// </summary>
        public struct Predicate
        {
            /// <summary>
            /// The name of the predicate. Has to match one of registered predicate methods.
            /// </summary>
            /// <remarks>
            /// 'index', 'type-and-index', etc.
            /// </remarks>
            public string name;

            /// <summary>
            /// The data to go with the predicate.
            /// </summary>
            /// <remarks>
            /// Formally a tuple, the type depends on the <see cref="name"/> - should match the input assumed by the method registered with the given name.
            /// </remarks>
            public object data;
        }

        /// <summary>
        /// Searches through the specified gameobject and returns the first gameObject that matches this predicate.
        /// </summary>
        public static Dictionary<string, Func<Component[], object, Component>> Predicates { get; } = new Dictionary<string, Func<Component[], object, Component>>()
        {
            { "index", ComponentFinder.GetComponentByIndex },
            { "type-and-index", ComponentFinder.GetComponentByTypeAndIndex }
        };

        /// <summary>
        /// The array of data to apply, and each data has a predicate associated with it that will match one component on the target GameObject.
        /// </summary>
        public List<(Predicate p, JToken d)> Data { get; set; }

    }
}