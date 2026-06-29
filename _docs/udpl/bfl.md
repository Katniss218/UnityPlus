
# UDPL Standard Library (BFL)

The Built-in Function Library (BFL) provides essential tools for manipulating `SerializedData`.
All functions accept and return `SerializedData` (or its subclasses: `SerializedPrimitive`, `SerializedObject`, `SerializedArray`).

### Type Safety & Error Behavior
UDPL functions are designed to be predictable. Unless otherwise specified:
*   **Type Mismatch:** Passing an `Object` where an `Array` is expected (or vice versa) throws a `TypeMismatchException`.
*   **Null Handling:** Passing `null` to a function that expects a value usually returns `null` (soft failure) unless in `STRICT` mode or if the function is a predicate (e.g., `IsNull`).
*   **Out of Bounds:** Array index access out of bounds throws an `IndexOutOfRangeException`.
*   **Strict Mode:** When `STRICT;` is enabled, soft failures (like returning `null` for a missing key) are promoted to exceptions.

**Function Overloading:** Functions in the BFL can be overloaded by the number of parameters. For example, `RenameKey(@old, @new)` operates on the implicit `this` context, while `RenameKey(@obj, @old, @new)` operates on a specific object.

**Naming Conventions:**
*   **Predicates:** Start with `Is`, `Has`, `Can` (e.g., `IsObject`, `ContainsKey`).
*   **Transformations:** Use verbs (e.g., `Clone`, `Append`, `Merge`).
*   **Conversions:** Start with `To` (e.g., `ToString`, `ToInt`).
*   **Collection Queries:** Start with `Get`, `Find`, `Count` (e.g., `GetKeys`, `Count`).

---

## 0. Operators

UDPL supports standard logical and relational operators natively. These are not functions, but part of the language syntax.

| Operator | Description |
| :--- | :--- |
| `==`, `!=` | Equality and inequality. |
| `>`, `<`, `>=`, `<=` | Relational comparisons (numeric). |
| `AND`, `OR`, `NOT` | Logical AND, OR, and NOT. |
| `+`, `-`, `*`, `/`, `%` | Arithmetic operators. `+` also concatenates strings (throws if mixing string and number). |
| `??` | Null-coalescing operator. Returns the left side if not null, otherwise evaluates and returns the right side. |
| `??=` | Null-coalescing assignment. Assigns the right side to the left side only if the left side is currently `null`. |

---

## 1. Core & Defensive Utilities

Functions for handling nulls, existence, and identity generation.

| Function | Signature | Description |
| :--- | :--- | :--- |
| **NewGuid** | `NewGuid()` | Generates a new random GUID string. |
| **HashOf** | `HashOf(@val)` | Returns a deterministic integer hash of the input value or object ID. |
| **Clone** | `Clone(@node)` | Creates a deep copy of the specified node. |
| **Delete** | `Delete(@node)` | Removes the node from its parent. Throws if the node has no parent (e.g., is the root). |
| **Exists** | `Exists(@path)` | Returns `true` if the path resolves to at least one node. |
| **ResolvePath**| `ResolvePath(@root, @pathStr)`| Evaluates a string path dynamically against the given root and returns a SerializedArray with deep-copies of the matching nodes. |
| **Assert** | `Assert(@boolean, @message)` | Throws an error with `message` if `boolean` is `false`. Useful for runtime validation. |
| **Throw** | `Throw(@reason)` | Immediately terminates the script execution and discards the transaction buffer, throwing an error with the given reason. |
| **IsNull** | `IsNull(@node)` | Returns `true` if the node is `null`. |

---

## 2. Math

All numeric operations return `double` (Float64) or `long` (Int64).

| Function | Signature | Description |
| :--- | :--- | :--- |
| **Pow** | `Pow(@a, @b)` | Returns `a` raised to the power of `b`. |
| **Abs** | `Abs(@val)` | Returns the absolute value. |
| **Clamp** | `Clamp(@val, @min, @max)` | Clamps value between min and max. |
| **Min** | `Min(@a, @b)` | Returns the smaller of two numbers. |
| **Max** | `Max(@a, @b)` | Returns the larger of two numbers. |
| **Round** | `Round(@val)` | Rounds to the nearest integer. |
| **Floor** | `Floor(@val)` | Rounds down. |
| **Ceil** | `Ceil(@val)` | Rounds up. |
| **RandomInt** | `RandomInt(@min, @max)` | Returns a random integer between min (inclusive) and max (exclusive). |
| **RandomFloat** | `RandomFloat(@min, @max)` | Returns a random float between min (inclusive) and max (exclusive). |

---

## 3. String Manipulation

| Function | Signature | Description |
| :--- | :--- | :--- |
| **Format** | `Format(@fmt, ...@args)` | Formats a string using C#-style placeholders (`{0}`, `{1}`). |
| **StartsWith**| `StartsWith(@str, @prefix)`| Returns `true` if `str` starts with `prefix`. |
| **EndsWith** | `EndsWith(@str, @suffix)` | Returns `true` if `str` ends with `suffix`. |
| **ToLower** | `ToLower(@str)` | Converts string to lowercase. |
| **ToUpper** | `ToUpper(@str)` | Converts string to uppercase. |
| **Trim** | `Trim(@str)` | Removes leading and trailing whitespace. |

---

## 4. Collections (Arrays, Objects, Strings)

These functions operate polymorphically across different collection types. **Note:** Mutation functions (like `Append`, `Insert`, `RemoveAt`, `Clear`, `Resize`) modify the collection **in-place** and return the modified collection. If called on a detached literal (e.g., `Append(["a"], "b")`), the literal is mutated but the document remains unchanged.

| Function | Signature | Description |
| :--- | :--- | :--- |
| **Count** | `Count(@collection)` | Returns the number of elements in an array, keys in an object, or characters in a string. |
| **Contains** | `Contains(@collection, @val)` | Returns `true` if an array contains the value, an object has a matching value, or a string contains the substring. |
| **Resize** | `Resize(@list, @size)` | Resizes the array. New elements are initialized to `null`. |
| **Append** | `Append(@list, @val)` | Appends a value to the end of the array. |
| **Insert** | `Insert(@list, @index, @val)` | Inserts a value at the specified index. |
| **RemoveAt** | `RemoveAt(@list, @index)` | Removes the element at the specified index. |
| **Clear** | `Clear(@collection)` | Removes all elements from an array or all keys and values from an object. |
| **First** | `First(@list)` | Returns the first element of an array. Throws if empty or if input is an object/primitive. |
| **FirstOrNull**| `FirstOrNull(@list)` | Returns the first element of an array or `null` if empty. Throws if input is an object/primitive. |
| **FirstOrDefault**| `FirstOrDefault(@list, @default)` | Returns the first element of an array or `default` if empty. Throws if input is an object/primitive. |
| **Distinct**| `Distinct(@list)` | Returns a new array containing unique values from the input array. Throws if input is an object/primitive. |
| **HasAny** | `HasAny(@collection)` | Returns `true` if an array or object has at least one child. Throws if input is a primitive. |
| **Sort** | `Sort(@collection, @mode)` | Returns a new sorted collection. Mode can be `"asc"` or `"desc"`. For objects, sorts by key. |

---

## 5. Objects & Dictionaries

These functions operate specifically on `SerializedObject` nodes.

| Function | Signature | Description |
| :--- | :--- | :--- |
| **ContainsKey** | `ContainsKey(@obj, @key)` | Returns `true` if the object contains the specified string key. |
| **RemoveKey** | `RemoveKey(@obj, @key)` | Removes the key from the object. |
| **GetKeys** | `GetKeys(@obj)` | Returns a `SerializedArray` containing all keys in the object. |
| **Merge** | `Merge(@target, @source)` | Shallow copies keys from `source` to `target`. Overwrites existing keys. |
| **DeepMerge** | `DeepMerge(@target, @src)` | Recursively merges `src` into `target`. Nested objects are merged, arrays are concatenated (optional behavior). |
| **RenameKey** | `RenameKey(@obj, @oldKey, @newKey)` | Renames key `oldKey` to `newKey` on the specific `obj`. |
| **GetOrCreate**| `GetOrCreate(@obj, @key, @def)`| Returns value at `key`. If missing, assigns `def` to `key` and returns `def`. |

---

## 6. Graph & Identity

Functions for interacting with the Serializer's `$id` and `$ref` conventions.

| Function | Signature | Description |
| :--- | :--- | :--- |
| **EnsureId** | `EnsureId(@node)` | Checks if the node has an `$id`. If not, generates a new GUID, assigns it, and returns it. |
| **Ref** | `Ref(@node)` | Creates a `{ "$ref" == "..." }` object pointing to the target node. Fails if target has no ID. |
| **ResolveRef** | `ResolveRef(@node)` | If `node` is a reference object (`{ $ref }`), looks up and returns the target object from the context. Returns `node` otherwise. |
| **RemapIDs** | `RemapIDs(@oldId, @newId)` | Recursively walks the entire document and replaces any `$id` or `$ref` matching `oldId` with `newId`. |

---

## 7. Type Manipulation & Conversion

| Function | Signature | Description |
| :--- | :--- | :--- |
| **ToInt** | `ToInt(@val)` | Converts a value to an Integer (Int64). |
| **ToFloat** | `ToFloat(@val)` | Converts a value to a Float (Float64). |
| **ToString** | `ToString(@val)` | Converts a value to its string representation. |
| **ToBool** | `ToBool(@val)` | Converts a value to a Boolean. |
| **TypeOf** | `TypeOf(@node)` | Returns the SerializedData type of the node as a string: `"bool"`, `"int"`, `"float"`, `"string"`, `"object"`, `"array"`, or `"null"`. |
| **IsBool** | `IsBool(@node)` | Returns `true` if the node is a boolean. |
| **IsInt** | `IsInt(@node)` | Returns `true` if the node is an integer. |
| **IsFloat** | `IsFloat(@node)` | Returns `true` if the node is a float. |
| **IsString** | `IsString(@node)` | Returns `true` if the node is a string. |
| **IsPrimitive** | `IsPrimitive(@node)` | Returns `true` if the node is primitive. |
| **IsObject** | `IsObject(@node)` | Returns `true` if the node is an object. |
| **IsArray** | `IsArray(@node)` | Returns `true` if the node is an array. |
| **IsType** | `IsType(@node, @typeName)` | Returns `true` if the node's `$type` header matches `typeName` (or resolves to it). |
| **StringToTypeObj**| `StringToTypeObj(@typeStr)` | Parses an Assembly Qualified Name string into a structured object containing `name`, `assembly`, `version`, `generics`, etc. |
| **TypeObjToString**| `TypeObjToString(@typeObj)` | Reconstructs a structured type object back into a valid Assembly Qualified Name string. |
| **ResolveType**| `ResolveType(@name)` | Resolves a simplified C# type name to its Assembly Qualified Name. |

---

## 8. Convention Structures

Functions for interacting with standardized convention keys (`$type`, `$id`, `$ref`, `$assetref`).

| Function | Signature | Description |
| :--- | :--- | :--- |
| **HasTypeHeader**| `HasTypeHeader(@obj)` | Returns `true` if the object has a `$type` key. |
| **HasId** | `HasId(@node)` | Returns `true` if the node has an `$id` property. |
| **HasRef** | `HasRef(@obj)` | Returns `true` if the object has a `$ref` key. |
| **HasAssetref**| `HasAssetref(@obj)` | Returns `true` if the node has an `$assetref` key. |

---

## 9. Logging

Functions for outputting information to the log stream.

| Function | Signature | Description |
| :--- | :--- | :--- |
| **Log** | `Log(@message)` | Logs a message to the Unity Console using `Debug.Log`. |
| **LogWarning** | `LogWarning(@message)` | Logs a warning message to the Unity Console using `Debug.LogWarning`. |
| **Trace** | `Trace()` | Logs the current evaluation path and a simulated stack trace to the console. |
