using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.Serialization
{
    public class GameObjectData<T>
    {
        // Serialization is split into 2 pieces:

        // - Object Factories - they create objects from some data (asset / json / whatever).

        // - ObjectData       - applies an arbitrary set of information to an existing object.

        public struct Predicate
        {
            public string name;
            public object data; // formally a tuple, the type depends on the name, should match the input assumed by the method registered with the given name.
        }

        public static Component GetComponentByIndex( Component[] components, object data )
        {
            int index = (int)data;
            if( index < 0 || index >= components.Length )
            {
                return null;
            }
            return components[index];
        }

        public static Component GetComponentByTypeAndIndex( Component[] components, object data )
        {
            (Type type, int index) = ((Type, int))data;

            int current = 0;
            foreach( var comp in components )
            {
                if( comp.GetType() == type )
                {
                    if( current == index )
                    {
                        return comp;
                    }
                    current++;
                }
            }
            return null;
        }

        /// <summary>
        /// Searches through the specified gameobject and returns the first gameObject that matches this predicate.
        /// </summary>
        public static Dictionary<string, Func<Component[], object, Component>> Predicates { get; } = new Dictionary<string, Func<Component[], object, Component>>()
        {
            { "index", GetComponentByIndex },
            { "type-and-index", GetComponentByTypeAndIndex }
        };

        /// <summary>
        /// The array of data to apply, and each data has a predicate associated with it that will match one component on the target GameObject.
        /// </summary>
        public List<(Predicate p, T d)> Data { get; set; }


        public void ApplyTo( GameObject target )
        {
            Component[] components = target.GetComponents<Component>();

            foreach( var predDataPair in Data )
            {
                Predicates[predDataPair.p.name]( components, predDataPair.p.data );
            }
        }
    }
}