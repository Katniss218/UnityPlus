# UnityPlus
A set of framework/library-like things for the Unity Engine

## Components:

### UnityEngine
A set of extensions to the base UnityEngine namespace.

### UnityPlus.Animation
WIP custom serializable and runtime-friendly animation system.

### UnityPlus.AssetManagement
A library for working with assets.

Assets are identified by their ID with which they were registered.

Supports 2-way lookup (by ID returning asset reference, and by asset reference returning ID).

### UnityPlus.Console
WIP console with commands and log stream display.

### UnityPlus.CSharp
Uses the Roslyn compiler to compile and run C# code at runtime.

### UnityPlus.OverridableEvents
Event manager with events that can disable (override) other events. Can be useful for modding.

### UnityPlus.Rendering
WIP compositing, camera stacking, shaders, and vfx.

### UnityPlus.Serialization
Contains a set of classes used for format-agnostic serialization of Unity (and non-unity) objects.

Uses GUIDs to serialize references to scene objects.

Uses UnityPlus.AssetManagement to serialize references to assets.
