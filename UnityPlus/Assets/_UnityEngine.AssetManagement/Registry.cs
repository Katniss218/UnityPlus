using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.AssetManagement
{
    public static class Registry<T>
    {
        // Registry is a class used to manage shared (singleton) references.

        // These can be:
        // - assets (prefabs, sprites, materials, etc).
        // - data   (loot tables, propellants, units, spells, dialogues, etc).

        // The distinguishing part is that there exists a "provider" that can find and return an item (asset/data) using an ID.
        //              It doesn't really have to be an ID per say, could also be a path, or other sequence of characters uniquely identifying an item.

        // Another key part is that the game will not reference things that are not a gameobject or component of which in the scene, or an item of the registry.

        // Allowed references:
        // - Registry Item
        // - GameObject
        // - Component of GameObject
        // - Event of Component of gameObject

        // registry items don't have to be loaded at startup, if a provider exists that can read e.g. an .fbx file, a mesh can be imported at runtime.


#warning TODO - should the asset ID be equivalent to the asset path? probably not.
    }
}