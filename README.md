# UnityPlus
A set of framework/library-like things for the Unity Engine

## Components:

### UnityEngine
A set of extensions to the base UnityEngine namespace.

### UnityPlus.AssetManagement
A library for working with assets.

Assets are identified by their ID with which they were registered.

Supports 2-way lookup (by ID returning asset reference, and by asset reference returning ID).

### UnityPlus.Serialization
Contains a set of classes used for format-agnostic serialization of Unity (and non-unity) objects.

Uses GUIDs to serialize references to scene objects.

Uses UnityPlus.AssetManagement to serialize references to assets.

### UnityPlus.Animation
WIP custom serializable and runtime-friendly animation system.
