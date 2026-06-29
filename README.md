# UnityPlus

**Hello there!**

Tis a collection of independent utility modules for the Unity Engine. Pick what you need, ignore the rest. Dependencies apply!

Unity was always quite annoying for me to work with, eventually instead of recreating the same utilities in every project, I consolidated them into what became UnityPlus. Most modules are production-ready and actively used in my other game https://github.com/Katniss218/HumanSpaceProgram. They're designed to work well together when needed, but you don't have to use all of them.

## Table of Contents:

1. What's Inside
2. Installation
3. Usage Examples
4. Contributing
5. License

## What's Inside:

**_UnityEngine** - Utility extensions and helpers for the base UnityEngine namespace. Includes high-precision math types (Vector3Dbl, QuaternionDbl, matrix types), curves, singleton patterns, coroutine utilities, and uncountable convenience methods.

**_UnityPlus.Animation [WIP]** - A serializable animation system with keyframe curves. Designed to be used at runtime rather than just as an editor tool.

**_UnityPlus.AssetManagement** - An extensible, modular asset resolution, loading, and caching framework.

**_UnityPlus.Input** - A hierarchical input system based on named channels and modular key bindings. Actions are priority-ordered - the first one to return true consumes the input. Channels can be enabled/disabled at runtime and bindings can be swapped out.

**_UnityPlus.CSharp [WIP]** - Compile and execute C# code at runtime using Roslyn. Useful for development tools, in-game script editors, or mod systems.

**_UnityPlus.Logging** - File and UI-based logging outputs. Automatically captures system information (CPU, GPU, memory, OS) on startup.

**_UnityPlus.OverridableEvents** - An event system where listeners can prevent other listeners from executing. Useful for modding systems and complex event flow where priority matters. Dependency sorting happens automatically based on listener declarations.

**_UnityPlus.OverridableValueProviders** - Similar concept to OverridableEvents, but for returning values through potentially complex pipelines (e.g. "get me gas properties at `vector`" might walk a hierarchy of different systems that all provide the same value, but with different priorities or conditions).

**_UnityPlus.Rendering** - Camera stacking and composition tools, barely working contact shadows.

**_UnityPlus.Serialization** - Flexible, format-agnostic object serialization with async support. Uses a powerful `Context` system allowing for contextual serialization. E.g. the same type of object might be an inline object when serialized as part of a game manager, but a reference when serialized as a member of the enemy AI component. Serialization can be paused and resumed later (via a time budget) allowing for 'background' serialization of large unity (main thread) objects without blocking the game.

**_UnityPlus.Serialization.Input** - Serialization descriptors for the input system.

**_UnityPlus.UILib** - Modular UI library wrapper around UGUI, including custom layout engine, and handy extensible fluent API.

## Installation:

**Prerequisites:**
- Unity 2022.1 or newer
- .NET Standard 2.1 or compatible

**Setup:**

1. Clone the repository or download as ZIP.

2. Copy the scripts into your `Assets` folder.

4. Start using the modules in your scripts:
   ```csharp
   using UnityPlus;
   using UnityPlus.Serialization;
   using UnityPlus.AssetManagement;
   ```

Each module has its own Assembly Definition, so you only compile what you need.

## Usage Examples - What's Outside:

### Serialization:

Saving:
```csharp
var dataHandler = new FileSerializedDataHandler( Path.Combine( celestialBodiesPath, $"{i}", "gameobjects.json" ), JsonFormat.Instance );

try
{
    SerializedData data = SerializationUnit.Serialize( celestialBody.gameObject, TimelineManager.RefStore );
    dataHandler.Write( data );
}
catch( UPSSerializationException ex )
{
    Debug.LogError( $"Failed to serialize celestial body '{celestialBody.name}': {ex.Message}" );
    Debug.LogException( ex );
}
```

Loading:
```csharp
var dataHandler = new FileSerializedDataHandler( Path.Combine( dir, "gameobjects.json" ), JsonFormat.Instance );
SerializedData data = dataHandler.Read();

try
{
    GameObject go = SerializationUnit.Deserialize<GameObject>( data, TimelineManager.RefStore );
}
catch( UPSSerializationException ex )
{
    Debug.LogError( $"Failed to deserialize celestial body from '{dir}'." );
    Debug.LogException( ex );
}
```

### Asset Management:

```csharp
// Register a custom resolver and loader for your project structure.
AssetRegistry.RegisterResolver( new MyGameDataFileResolver() );
AssetRegistry.RegisterLoader( new MyJsonLoader() );

// Retrieve assets
var texture = await AssetRegistry.GetAsync<Texture2D>("player_avatar");

// Find the ID for an asset you already have
string id = AssetRegistry.GetID(myTexture);
```

### Overridable Events:

```csharp
var evt = new OverridableEventRegistry<float>()
// Listeners can specify their own ordering, and block other listeners from being invoked.
var listener = new OverridableEventListener<float>( "my_listener", arrBlacklist, arrBefore, arrAfter, callback );

evt.TryCreate( "my_event" );
evt.TryAddListener( "my_event", listener );

evt.TryInvoke( "my_event", 3.1415 );
```

## Contributing:

Contributions are welcome. If you find issues or have ideas for improvements, open an issue or pull request on GitHub.

## License:

UnityPlus is licensed under [MIT](LICENSE).
