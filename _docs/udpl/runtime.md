# UDPL Runtime Execution Model

## 1. Memory Model

### 1.1 The Node Reference (`NodeRef`)
UDPL operates on a DOM (Document Object Model) of `SerializedData`. To support mutation (changing values in the tree), we utilize a **Container-Aware Pointer** system called `NodeRef`.

There are two types of NodeRefs:

#### A. Bound NodeRef (Mutable)
Points to a specific location inside a parent container.

*   **Structure:**
    *   `SerializedData Parent`: The container holding this node.
    *   `SerializedData Instance`: The current node itself (used for Identity tracking).
    *   `string Key`: (Objects only) The dictionary key.

*   **Resolution Strategy:**
    *   **Object Member:** Resolved via `Parent[Key]`. Keys are stable.
    *   **Array Element:** Resolved via **Instance Identity**.
        *   To find the current index of the element, the runtime scans the `Parent` array for the `Instance` using **Reference Equality**.
        *   *Reason:* Indices are volatile (shift on insert/delete). Values are ambiguous (multiple `5`s). Object Identity is unique and persistent.

*   **Write Behavior (`this = val`):**
    1.  **Locate:** Find the slot in `Parent` (via Key or Identity Scan).
    2.  **Swap:** Replace `Parent[slot]` with `val`.
    3.  **Rebind:** Update `NodeRef.Instance = val`. This ensures subsequent operations on this variable track the *new* object.

*   **Delete Behavior (`Delete(this)`):** Removes the entry from the Parent. Assigning `null` (`this = null`) merely sets the value to `null` and does *not* remove the key or array element.

#### B. Detached NodeRef (Read-Only/Literal)
Wraps a loose `SerializedData` value that is not attached to the document tree (e.g., a literal `5`, a result of a math operation, or a newly created object not yet assigned).
*   **Structure:** `Instance`. `Parent` is null.
*   **Write Behavior:** Throws `ReadOnlyMemoryException`. You cannot assign *to* a literal.

### 1.2 The Scope Stack
The interpreter maintains a stack of execution scopes.
```csharp
class Scope {
    public NodeRef This; // The context for implicit lookups
    public Dictionary<string, SerializedData> Variables; // Local vars (@var). should be null initially to not allocate unnecessarily.
    public Scope Parent;

    public void AssignVariable(string name, SerializedData value) {
        // If it exists in this or any parent scope, update it there.
        // Otherwise, declare it in the current scope.
    }
}
```
*   **Implicit Declaration & Scope-Level Hoisting:** Variables are not explicitly declared. The runtime identifies all variables used within a Function or Migration and implicitly initializes them to a `default` value at the **start of that scope**.
*   **Initialization Values:**
    *   Numeric context: `0`.
    *   Boolean context: `false`.
    *   Reference/Object context: `null`.
*   This ensures that variables (like `@total`) are preserved across loop iterations even if their first "assignment" occurs inside a nested block.

### 1.3 The Global Context (`global`)
There is a persistent `SerializedObject` accessible via the `global` root identifier.
*   **Persistence:** It exists for the lifetime of the patching session (across multiple files).
*   **Usage:** Useful for passing flags, version numbers, or configuration between included scripts.
*   **Syntax:** `global.version`, `global.debug = true`, `global.my_namespace.my_value = 42;`.

### 1.4 Assignment Semantics & Ownership
UDPL treats the Document as a **Tree**, not a Graph. To prevent aliasing bugs (where modifying one part of the tree accidentally affects another), the runtime enforces strict ownership.

*   **Implicit Deep Clone:** When assigning an Object or Array from a variable/expression into the Document, the runtime performs a **Deep Clone**.
    *   `@template = { hp == 100 };`
    *   `ships[*].stats = @template;`
    *   *Result:* Each ship gets a distinct copy of the stats object. Modifying one ship's stats later does not affect others or the `@template` variable.
*   **Explicit Reference:** To share data, you must use the ID/Reference system explicitly.
    *   `ships[*].target = Reference(@boss);`
    *   *Result:* Creates a `{ "$ref" == "..." }` node pointing to the target.

---

## 2. The Resolution Engine (Path Evaluator)

The core engine component is `PathResolver`. It takes a `Context` (list of NodeRefs) and a `PathExpression`, and returns a new `Context`.

### 2.1 The Snapshot Rule
When resolving a path that contains a wildcard `[*]` (specifically for Arrays), the runtime employs a **Snapshot Strategy**.

1.  **Resolution:** The path is resolved to a list of `NodeRef`s.
2.  **Snapshot:** This list captures the **Object Identities** of the children at that moment.
3.  **Execution:** The `FOREACH` loop iterates through these refs.

**Behavior under Mutation:**
*   **Deletion:** If the loop body deletes the current node (`Delete(this);`), the `NodeRef` becomes detached. Subsequent operations in the same iteration on `this` will fail (safe). Future iterations are unaffected because they track their own Identities.
*   **Insertion:** If the loop inserts new items into the parent array, they are **not** visited (because they were not in the snapshot).
*   **Reordering:** If the list is sorted, the loop continues to visit the original items in their *original* order (because the snapshot preserved that order), effectively visiting "Objects" rather than "Indices".

### 2.2 Array Slicing
When resolving a range expression like `[0..5]`, the upper bound is **exclusive**. This means `[0..5]` will select elements at indices 0, 1, 2, 3, and 4.
*   **Array Only:** Range accessors are only valid for Arrays. Objects cannot be sliced.
*   **Integer Bounds:** The bounds of a range must evaluate to integers. This means you can use integer literals (e.g., `[0..5]`) or variables containing integers (e.g., `[@start..@end]`).
*   **No Implicit Lookups:** Implicit `this` lookups (e.g., `[min..max]`) or explicit string keys (e.g., `["min".."max"]`) are **not** allowed in range bounds. This prevents self-referential ambiguity, as arrays do not have named children.

### 2.3 Handling Path Resolution
When a path expression is encountered on the right-hand side:
1.  **Resolve:** The `path` is resolved to a list of `NodeRef`s.
2.  **Extract:** The `.Instance` value is extracted from each ref.
3.  **Return:** A `SerializedData` (Primitive, Object, or Array).

### 2.4 Function Cardinality
*   **Scalar Functions (1:1):** Expect a single node. Throws `AmbiguousPathException` if multiple nodes provided.
*   **Aggregate Functions (N:1):** Consumes the entire list.

### 2.5 Evaluation Cardinality
RHS expressions are evaluated **once per target (LHS) node**.

*   `items[*].id = Guid();`
    *   **Result:** `Guid()` is called N times. Every item gets a unique ID.
*   `@g = Guid(); items[*].id = @g;`
    *   **Result:** `Guid()` is called once. Every item gets the *same* ID.

---

## 3. User Defined Functions (UDF)

### 3.1 Definition & Scoping
*   **Global Definition:** User-Defined Functions (UDFs) can only be defined in the global scope.
*   **Function Scoping:** When a function is invoked, it receives a fresh scope. Variables are local to the function.
*   **`this` Context:** UDFs do **not** inherit `this` automatically. `this` is undefined inside a function unless explicitly passed as an argument.
*   **Pass-by-NodeRef:** Parameters are passed as lists of NodeRefs. Assigning to a parameter's properties (`@param.field = 5`) modifies the source data via the Bound NodeRef mechanism. Assigning to the parameter itself (`@param = 5`) merely overwrites the local variable binding and does **not** mutate the document.
*   **Replacing via `this`:** If `this` is passed to a function (e.g., `Func(this)`), the function can replace the calling node by assigning to the parameter: `@p = @x; return;`. This is functionally equivalent to returning a new value to be assigned by the caller.

### 3.2 The Include System
The `INCLUDE` directive allows modularizing scripts.
*   **Syntax:** `INCLUDE "file.udpl";`
1.  **Pass 1 (Resolution & Deduplication):** The engine scans all includes and builds a dependency graph, ensuring each file is loaded exactly once (topological sort).
    *   **Strict Mode:** If an included file is missing, an exception is thrown.
    *   **Default Mode:** If an included file is missing, it is ignored.
2.  **Pass 2 (Execution):** The engine stitches the ASTs together. Raw statements in helper files are injected directly into the transaction buffer of the main script.

### 3.3 Return Values
*   **Value Context (Expression):** Must execute `RETURN`. Missing return throws exception.
*   **Void Context (Statement):** Return value is discarded. Implicit void return allowed.

### 3.4 Directives & Transactions
Directives (e.g., `MIGRATION`, `INCLUDE`, `STRICT`) are UPPERCASE_OR_UNDERSCORE identifiers followed by literal arguments. They are **always executed first** by the runtime before any functions or statements are evaluated, regardless of their position in the script.

#### MIGRATION
The `MIGRATION` directive defines the execution context and transaction boundary of a script.
*   **Syntax:** `MIGRATION "Name" [, ...args];`
*   If a script contains `MIGRATION`, the engine opens a transaction buffer.
*   All mutations apply to this buffer.
*   If the script completes successfully, the buffer commits. If an error occurs (or `Throw()` is called), the buffer is discarded.
*   **Simulation (Dry Run):** The engine can execute a migration in "Dry Run" mode (e.g., via a directive argument or engine flag). Mutations are tracked and logged to the output stream, but the buffer is discarded at the end instead of being committed.
*   Helper files (libraries) should omit the `MIGRATION` header. Attempting to execute a helper file directly will result in an error.

#### Runtime Validation
*   The runtime validates the directive name and arguments.
*   Unknown directives will cause the runtime to throw an exception.
*   Some directives may be restricted to a single occurrence, while others can be repeated.

---

## 4. Strict Math & Coercion
UDPL avoids JavaScript-style loose typing to prevent data corruption.

### 4.1 Arithmetic (`+`, `-`, `*`, `/`)
*   **Rule:** Both operands must be Numeric (Int/Float/Double).
*   **Behavior:** If operands are valid, performs math. If both are `int`, returns `int`. If any is `float`, returns `float`.
*   **Error:** If any operand is Non-Numeric (String, Object, Null), throws `TypeMismatchException`.

### 4.2 Relational Operators (`==`, `!=`, `<`, etc.)
*   **Strict Type Matching:** UDPL requires operands to have the **exact same type**.
*   **Error:** Comparing an `int` to a `float` (e.g., `1 == 1.0`) throws a `TypeMismatchException`. Comparing a `string` to a `null` (except via `IsNull()`) also throws.
*   **Rationale:** Prevents logic bugs caused by unexpected data types in the document.

### 4.3 Concatenation
*   **Rule:** The `+` operator supports string concatenation.
*   **Behavior:** If both operands are strings, they are concatenated.
*   **Error:** If operands are a mix of string and number (or other types), it throws a `TypeMismatchException` because UDPL does not perform implicit type coercion.

### 4.4 Strict Mode (`STRICT;`)
By default, UDPL is "soft" to allow scripts to run against slightly varied data structures. The `STRICT;` directive changes this behavior:
*   **Navigation:** If a path segment (e.g., `.field`) cannot be resolved, an exception is thrown instead of returning `null`.
*   **Auto-Vivification:** Disabled. Intermediate nodes must be explicitly created.
*   **Assignment:** If an assignment fails (e.g., trying to assign to a non-existent path), an exception is thrown.
*   **Types:** Implicit type conversions and comparisons are strictly forbidden.

---

## 5. Identity Management

Object Identity in the document is managed via the special `$id` and `$ref` keys.

*   **New Objects:** Object literals `{}` are anonymous. They do not have an `$id` by default.
*   **Minting ID:** Assigning to the special key `"$id"` mints an identity.
    *   `@obj["$id"] = NewGuid();`
*   **EnsureId(node):** (Proposed BFL) Checks if a node has an ID. If not, generates and assigns one, returning it. If yes, returns existing.
    *   `refId = EnsureId(@target);`

---
