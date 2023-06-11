using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Serialization.ComponentData
{
    /// <summary>
    /// Stores persistent data about a gameobject.
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

        public static Dictionary<string, Func<Component[], object, Component>> PredicateRegistry { get; } = new Dictionary<string, Func<Component[], object, Component>>()
        {
            { "index", ComponentFinder.GetComponentByIndex },
            { "type-and-index", ComponentFinder.GetComponentByTypeAndIndex }
        };

        /// <summary>
        /// The array of data to apply.
        /// </summary>
        /// <remarks>
        /// Each entry is a predicate-data pair. The predicate will match one component on the target GameObject, and the data will be applied to that component.
        /// </remarks>
        public List<(Predicate p, JToken d)> Data { get; set; }
    }
}