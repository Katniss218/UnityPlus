## Mapping-first architecture for serialization and inspection

### Goals
- **Single source of truth**: One mapping describes how to access members of an object graph and the actions supported for each member/type.
- **Logic separation**: Mappings expose access and intent; serializers/inspectors implement the behavior.
- **Context-aware**: Behavior can vary by context (runtime vs editor, versioning, security, etc.).

### Core concepts
- **ObjectMapping**: Describes a type: how to construct it, enumerate members, and which actions are supported.
- **MemberMapping**: Describes a single member (field/property/indexer): getters/setters, metadata, supported actions, converters.
- **CollectionMapping**: Describes sequence/dictionary-like access (add/remove/insert/clear, key/value accessors).
- **PolymorphicMapping**: Describes base→derived relationships and discriminator policy.
- **Actions**: Declarative capabilities exposed by mappings (e.g., can read, can write, can add element, can reorder).
- **Strategies**: Plug-in logic that consumes mappings for a purpose (serialization, inspection, validation, diffing).
- **Context**: Ambient data that influences strategies (version, culture, formatting, privileges, drawer/theme, etc.).

### Key interfaces (conceptual)
```csharp
public interface IObjectMapping
{
    Type TargetType { get; }
    IObjectFactory Factory { get; }
    IReadOnlyList<IMemberMapping> Members { get; }
    IPolymorphicPolicy Polymorphism { get; }
    IActionSet Actions { get; } // e.g., Construct, Copy, Compare
}

public interface IMemberMapping
{
    string Name { get; }
    Type MemberType { get; }
    IValueAccessor Accessor { get; } // get/set raw value on instance
    IConverter? Converter { get; }    // optional type/format converter
    IMemberActions Actions { get; }   // e.g., Readable, Writable, Resettable
    IReadOnlyDictionary<string, object> Metadata { get; }
}

public interface ICollectionMapping : IMemberMapping
{
    bool IsDictionary { get; }
    ICollectionActions CollectionActions { get; } // Add/Remove/Clear/Reorder
    IElementAccessor Elements { get; }
}

public interface IPolymorphicPolicy
{
    bool TryGetDiscriminator(Type runtimeType, out string discriminator);
    bool TryResolve(string discriminator, out Type concreteType);
}

public interface IActionSet { bool Supports(string actionName); }

public interface IValueAccessor
{
    object? Get(object instance);
    void Set(object instance, object? value);
}
```

### How serialization consumes mappings
- Walk the `IObjectMapping` graph, not reflection directly.
- For each `IMemberMapping`:
  - Check `Actions.Writable/Readable` and strategy policy to decide inclusion.
  - Use `Accessor.Get/Set` for raw values; if `Converter` present, apply to/from wire representation.
  - Use `CollectionMapping` to iterate/add elements deterministically; include ordering metadata if provided.
- For polymorphism:
  - Use `IPolymorphicPolicy` to emit/read discriminator.
- Versioning/conditional members:
  - Strategies consult `Metadata` (e.g., `[Since(3)]`, `[Deprecated]`) and `Context.Version` to include/ignore.

Serialization pseudo-flow:
```csharp
var mapping = MappingRegistry.Get(typeof(T));
var node = writer.BeginObject(mapping.TargetType);
foreach (var m in mapping.Members)
{
    if (!m.Actions.Readable) continue;
    var value = m.Accessor.Get(instance);
    var serialized = m.Converter?.ToSerialized(value, ctx) ?? value;
    writer.WriteMember(m.Name, serialized, ctx);
}
writer.EndObject();
```

### How inspection consumes mappings
- Build UI using `IMemberMapping` metadata rather than reflection.
- Drawer selection:
  - Use a `DrawerProviderRegistry` that matches a member/type + `Metadata` to a drawer.
  - Drawer reads `Actions` to enable/disable controls (e.g., if not writable → readonly field).
- Editing flow:
  - UI control → convert via `Converter` (string⇄type, culture-aware) → `Accessor.Set`.
  - Undo/redo uses command objects that call `Accessor.Set` and store old/new values; commands can be grouped by mapping paths.
- Collections and polymorphism:
  - Use `CollectionActions` to show add/remove/reorder UI.
  - Use `IPolymorphicPolicy` to present type pickers for derived types.

Inspection pseudo-flow:
```csharp
var mapping = MappingRegistry.Get(obj.GetType());
foreach (var m in mapping.Members)
{
    var drawer = DrawerProvider.For(m, ctx);
    drawer.Draw(
        read: () => m.Accessor.Get(obj),
        write: v => m.Accessor.Set(obj, m.Converter?.FromUI(v, ctx) ?? v),
        actions: m.Actions,
        metadata: m.Metadata,
        ctx: ctx);
}
```

### Mapping construction
- Attribute-based: reflect once, read custom attributes into `Metadata`, create accessors/delegates; cache in `MappingRegistry`.
- Code-first: manually authored mappings for special cases or performance.
- Hybrid: attributes with overrides supplied by registration code.

### Registries
- `MappingRegistry`: Type → `IObjectMapping`. Immutable after warm-up; supports versioned variants.
- `ConverterRegistry`: Type/format → `IConverter` (e.g., `DateTime <-> string` with culture).
- `DrawerProviderRegistry`: Member/Type/metadata → drawer factory.

### Contexts
- `SerializationContext`: version, format, references, cycle handling, include/exclude policies.
- `InspectionContext`: theme, culture, validation mode, privilege level, change tracking.

### Actions (examples)
- Object-level: `Construct`, `Clone`, `Compare`, `Diff`.
- Member-level: `Readable`, `Writable`, `Resettable`, `Browsable`, `Required`.
- Collection-level: `Add`, `Remove`, `Insert`, `Clear`, `Reorder`.

### Paths and identity
- Stable logical paths (e.g., `Player.Inventory[3].Name`) derived from mappings to:
  - Drive undo/redo, diff/patch, and targeted serialization (partial updates).
  - Provide reliable keys for saved editor state and validation messages.

### Error handling and validation
- Mappings can attach validators in `Metadata` or `Converter` (range, regex, custom rules).
- Serialization strategy surfaces validation failures with path + message.
- Inspection strategy displays validation states and prevents invalid writes when configured.

### Versioning and compatibility
- Encode version rules in `Metadata` (since/obsolete/rename-to) and let strategies interpret them.
- Serializer can emit both current and legacy names (alias metadata) or migration hints.

### Migration and diff/patch
- Because mappings define paths and access, a diff strategy can compare two instances by walking the same mappings.
- A patch strategy can apply changes using `Accessor.Set` and collection actions.

### Benefits
- Eliminates duplication between serializer and inspector; both use the same authoritative access layer.
- Improves performance: reflection only during mapping build; runtime uses cached delegates.
- Enables consistent policies (visibility, writability, converters) across features.

### Minimal integration plan
1. Extract access and metadata from current `SerializationMapping` into `IObjectMapping`/`IMemberMapping`.
2. Move serialization logic into a `V4Serializer` that consumes mappings.
3. Add `DrawerProviderRegistry` and update `UIObjectInspectorWindow` to draw via mappings.
4. Introduce `ConverterRegistry` and migrate existing converters.
5. Add `IPolymorphicPolicy` and collection actions to mappings; update both strategies to use them.
6. Replace direct reflection in any remaining code paths with mapping accessors.


