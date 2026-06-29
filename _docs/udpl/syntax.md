# UDPL Syntax Specification (v1)

## 1. Design Philosophy: "Shell-Style" Dereferencing
UDPL is designed for **Data Patching** and **Data Rewriting**, where the majority of operations involve referencing dictionary keys (strings).
To reduce syntactic noise, UDPL distinguishes between **Data Keys**, **Data Values**, and **Memory Variables** using prefixes.

*   **`identifier`** : Treated as a **String Literal** when used as a **Key** (LHS of assignment). When used on the RHS, it is treated as a **Path Dereference** (The Data Value).
*   **`@identifier`**: Treated as a **Variable** (The Memory Value).
*   **`"quoted"`**   : Treated as a **String Literal** (Explicit, required for RHS values and special chars).

### Comparison
| Concept | Standard C# | UDPL |
| :--- | :--- | :--- |
| **String Key** | `"status"` | `status` |
| **Data Lookup** | `obj["status"]` | `status` |
| **Variable** | `myVar` | `@myVar` |
| **Key Assignment** | `obj["status"]` = 5 | `status = 5` |
| **Var Assignment** | `myVar = 5` | `@myVar = 5` |
| **Special Characters** | `obj["$type"]` | `this["$type"] = 5` (Quotes required on LHS) |

---

## 2. Grammar (EBNF)

```ebnf
Program         ::= (Directive | FunctionDef | Statement)*

/* --- Directives --- */
/* Directives are UPPERCASE_OR_UNDERSCORE identifiers followed by literal arguments. */
/* They are evaluated by the runtime before script execution, regardless of position. */
Directive       ::= DirectiveIdentifier (DirectiveArgument ("," DirectiveArgument)*)? ";"
DirectiveIdentifier ::= [A-Z_]+
DirectiveArgument ::= Literal | StructureLiteral

/* --- Functions --- */
/* Defines a reusable block of logic. */
FunctionDef     ::= "FUNC" Identifier "(" ParamList? ")" Block
ParamList       ::= VariableIdentifier ("," VariableIdentifier)*

/* --- Statements --- */
Block           ::= "{" Statement* "}"

Statement       ::= SelectionStmt 
                  | IfStmt
                  | AssignmentStmt
                  | ReturnStmt
                  | ExprStmt ";"

/* --- Selection & Control Flow --- */
/* Iterates over every node found by the PathExpr */
SelectionStmt   ::= "FOREACH" PathExpr ("as" VariableIdentifier ("," VariableIdentifier)?)? ("WHERE" BooleanExpr)? Block

/* Conditional Execution */
IfStmt          ::= "IF" BooleanExpr Block ("ELSE" Block)?

/* --- Mutation --- */
/* Target = Source. Target MUST be an L-Value. */
AssignmentStmt  ::= LValue AssignOp Expression ";"
AssignOp        ::= "=" | "+=" | "-=" | "*=" | "/=" | "%=" | "??="

ReturnStmt      ::= "RETURN" Expression ";"

ExprStmt        ::= FuncCall

/* --- L-Values (Assignment Targets) --- */
/* An L-Value resolves to a Reference (Location), not a Value. */
LValue          ::= PathExpr
                  | StringLiteral       /* Syntax sugar for: this[StringLiteral] */
                  | VariableIdentifier  /* @var */

/* --- Expressions (R-Values) --- */
Expression      ::= NullCoalescingExpr

NullCoalescingExpr ::= LogicalOrExpr ("??" LogicalOrExpr)*
LogicalOrExpr   ::= LogicalAndExpr ("OR" LogicalAndExpr)*
LogicalAndExpr  ::= PatternMatchExpr ("AND" PatternMatchExpr)*
PatternMatchExpr::= RelationalExpr (("is" | "is" "not") (PatternObject | VariableIdentifier))?
RelationalExpr  ::= AdditiveExpr (("==" | "!=" | ">" | "<" | ">=" | "<=") AdditiveExpr)?
AdditiveExpr    ::= MultiplicativeExpr (("+" | "-") MultiplicativeExpr)*
MultiplicativeExpr::= UnaryExpr (("*" | "/" | "%") UnaryExpr)*
UnaryExpr       ::= ("NOT" | "!" | "-")? PrimaryExpr

PrimaryExpr     ::= Literal
                  | PathExpr            /* Data Lookup */
                  | VariableIdentifier  /* The @ Operator (Memory) */
                  | FuncCall
                  | ObjectLiteral
                  | ArrayLiteral
                  | "(" Expression ")"

FuncCall        ::= Identifier "(" (Expression ("," Expression)*)? ")"

/* --- Identifiers --- */
VariableIdentifier ::= "@" Identifier

/* --- Paths --- */
/* A path describes how to traverse from a Root to a Node. */
/* If a path starts with a Segment, 'this' is implied. */
/* PathExpr can start with a Variable to access properties of an object stored in a variable. */
PathExpr        ::= (RootIdentifier)? (Segment)*
RootIdentifier  ::= "this" | "root" | "global" | VariableIdentifier | Identifier

Segment         ::= "." (Identifier | StringLiteral)  /* Field Access (String Key) */
                  | "[" Indexer "]"                   /* Key/Index Access */

Indexer         ::= Integer                     /* Explicit Index: [0] */
                  | StringLiteral               /* Explicit Key: ["$type"] */
                  | Expression                  /* Computed Key: [$i] or [@keyVar] */
                  | RangeExpr                   /* Slice: [0..5] */
                  | "*"                         /* Wildcard: [*] */

RangeBound      ::= Integer | VariableIdentifier
RangeExpr       ::= RangeBound? ".." RangeBound?      /* 0..5, 1.., ..@end */

/* --- Literals --- */
Literal         ::= StringLiteral | Integer | Float | Boolean | "null" | "NaN" | "infinity" | "-infinity"
StringLiteral   ::= '"' ( [^"\\] | "\\" . )* '"'
Integer         ::= "-"? [0-9]+
Float           ::= "-"? [0-9]+ "." [0-9]+ (("e" | "E") ("+" | "-")? [0-9]+)?
Boolean         ::= "true" | "false" | "TRUE" | "FALSE"
Identifier      ::= [a-zA-Z_][a-zA-Z0-9_]*

ObjectLiteral    ::= "{" (ObjectProperty ("," ObjectProperty)*)? "}"
ObjectProperty   ::= (Identifier | StringLiteral) ":" Expression
ArrayLiteral     ::= "[" (Expression ("," Expression)*)? "]"

/* --- Pattern Matching --- */
PatternObject    ::= "{" (PatternProperty ("," PatternProperty)*)? "}"
PatternProperty  ::= (Identifier | StringLiteral) (Binding (Condition)? | Condition)?
Binding          ::= "as" VariableIdentifier
Condition        ::= ("==" | "!=" | ">" | "<" | ">=" | "<=") Expression
```

## 3. Semantic Rules

### 3.1 Data Lookup (Path Expressions)
In UDPL, any unquoted identifier on the right-hand side of an expression is treated as a **Data Lookup** (a path expression).
*   **Usage:** `path.to.value`
*   **Implicit `this`:** If a path starts with an identifier that is not a special keyword (`this`, `root`, `global`), it implicitly refers to a property on the current `this` context. For example, `health` is equivalent to `this.health`.
*   **Keywords as Keys:** If you need to access a property named "global" or "this", you must use the indexer syntax: `this["global"]` or `this["this"]`.
*   **Quoted Strings as Paths:** A `"quoted value"` by itself is always treated as a string literal. To use a quoted string as a path segment, it must be a child segment, e.g., `this."quoted value"` or `parent["quoted value"]`.
*   **Assigning to `this`:** You can replace the current context node entirely by assigning to `this` (e.g., `this = @newNode;`). This replaces the node in its parent container. If `this` is the root of the document, the behavior depends on the host application (typically replacing the entire document).
*   **Single Object RHS:** The right-hand side of an assignment must evaluate to a single object or value. Expressions like `vessels[*].hp += vessels[*].bonus` are **not allowed** and will throw an error ("right hand side must be a path that evaluates to a single object"). If you need to process a list, use a `FOREACH` loop or functions like `Append()`.
*   **Auto-Vivification:** In default mode, assignments to missing paths will **auto-vivify** (create) intermediate objects. For example, `a.b.c = 1` will create object `a` and object `b` if they do not exist. Auto-vivification only creates objects, not arrays. If an intermediate node exists but is a primitive or array, it throws a `TypeMismatchException`.
*   **STRICT Mode Auto-Vivification:** When `STRICT` mode is enabled, auto-vivification is **disabled**. You must explicitly initialize intermediate nodes using the `??=` operator.
    *   *Example:* `matrix ??= []; matrix[0] ??= {}; matrix[0].hp = 10;`

### 3.2 Accessor Semantics (`[]` and `[*]`)
The indexer syntax `[]` behaves differently depending on whether the target is an Object or an Array.

#### Wildcard Accessor (`[*]`)
*   **Arrays:** Enumerates all elements in the array.
*   **Objects:** Enumerates all *values* in the object. This allows you to iterate over all values of an object using the same syntax as an array.
*   **Key-Value Iteration:** When using `FOREACH` with a wildcard, you can bind both the key/index and the value: `FOREACH dict[*] as @key, @value`. For objects, `@key` is the string key; for arrays, `@key` is the integer index.

#### Key/Index Accessors
*   `["hello"]` is a valid **Object** accessor. It looks up the key `"hello"`.
*   `[1]` is a valid **Array** accessor. It looks up the element at index `1`.
    *   `[1]` is **NOT** a valid Object accessor. To access a key named `"1"`, you must use `["1"]`.
*   `[@strVar]` is a valid **Object** accessor (assuming `@strVar` contains a string).
*   `[@intVar]` is a valid **Array** accessor (assuming `@intVar` contains an integer).
*   `[@floatVar]` is **NOT** a valid Array accessor, even if the float has no fractional part (e.g., `2.0`). Array indices must be integers. `[2.0]` as a literal is also invalid.

#### Range Accessors (`[start..end]`)
Range accessors are **only valid for Arrays**. Arrays have indexed children, whereas Objects have named children.
*   `[0..5]` is a valid Array range accessor.
*   `[@intVar1..@intVar2]` is a valid Array range accessor. Same as `[@var.min..@var.max]` is also valid, provided that min and max are integers.
*   `arr[1..2+3]` is **NOT** valid. Range bounds must be integer literals or variables containing integers. Expressions are not allowed.
*   `[{"min": 4, "max": 5}]` is **NOT** a valid range accessor.
*   `[min..max]` and related, like `this["min"]`, etc are **NOT** valid range accessors. (`this` lookups like `min`, `this.min` are not allowed in range bounds to prevent self-referential ambiguity).
*   *Rule of Thumb:* Range bounds must be integer literals or variables containing integers.

### 3.3 The `@` Operator (Variable)
The `@` token identifies **Local Variables**. Variables can hold Primitives or References to Objects (SerializedData).

#### A. Declaration and Scoping
*   Variables do not require explicit declaration.
*   **Scope-Level Hoisting:** Variables are scoped to the **Function** or **Migration** in which they appear.
*   **Implicit Initialization:** Every variable used in a scope is implicitly initialized to a `default` value at the **very start of that scope**, regardless of where it first appears in the code.
    *   Numeric context: Initialized to `0`.
    *   Boolean context: Initialized to `false`.
    *   Object/Array/String context: Initialized to `null`.
*   This ensures that variables used inside loops or conditional blocks are preserved across iterations and accessible throughout the entire scope, mirroring C# local variable behavior.

#### B. Direct Access vs Path Assignment
*   **Read:** `val = @myVar;` (Copies the value/ref from the variable).
*   **Write to Variable:** `@myVar = 5;` (Overwrites the local variable binding. It does **not** mutate the document, even if `@myVar` previously held a reference to a document node).
*   **Write to Document:** `@myVar.prop = 5;` (Mutates the document at the referenced location).

#### C. Nested Access (Variables as Roots)
If a variable holds an Object, you can traverse it. The variable acts as the Root of the path.
*   **Read Field:** `val = @myObj.field;` 
    *   `@myObj.field` is the Path (Location). 
*   **Write Field:** `@myObj.field = 5;`
    *   `@myObj.field` is the Path (Target).

#### D. Dynamic Keys (Variables as Indices)
Variables can be used inside `[]` to dynamically select keys or indices.
*   **Array Index:** `item = items[@i];`
*   **Dictionary Key:** `val = data[@keyString];`

### 3.4 Implicit Context
*   **LHS Implicit Root:** If an L-Value starts with an Identifier or String Literal, `this` is the implied root.
    *   `hp = 10` == `this.hp = 10`
*   **No RHS Implicit String:** String literals in expressions MUST be quoted.
    *   `tag = "Player"` == `this["tag"] = "Player"`
    *   `tag = Player` -> **Error** (Player is treated as an undefined function or variable).

### 3.5 Implicit List Execution & Iteration
Paths in UDPL evaluate to a **List of Nodes**.
*   `vessels[*].id = 1;`
    *   Finds all `vessels`.
    *   Finds `id` on each vessel.
    *   Sets all of them to `1`.

When using `FOREACH`, you can optionally bind the current iteration node to a variable using `as @var`, or bind both key and value using `as @key, @value`. If omitted, the node is still implicitly bound to `this`. `this` is bound always, regardless whether or not any other variable is bound.
*   `FOREACH vessels[*] as @v { @v.id = 1; }`
*   `FOREACH vessels[*] as @i, @v { log = "Index: " + @i; }`
*   **Null Arrays:** If a `FOREACH` target evaluates to `null` or an empty array, the loop body executes `0` times without throwing an error.
*   This is especially useful for nested loops to access the outer loop's node.

### 3.6 Function Return Safety
*   **Value Context:** If a function is called in an expression (`x = Func()`), it **MUST** return a value. If execution ends without a `RETURN`, the runtime throws an error.
*   **Void Context:** If a function is called as a statement (`Func();`), return values are discarded. It is safe to return nothing.
*   **Method Calls Not Supported:** UDPL does not support object-oriented method calls (e.g., `myObj.Func()`). All functions are global and must be called with the target object as an argument: `Func(myObj)`.

### 3.7 Safe-by-Default Navigation
UDPL paths are inherently null-safe (similar to optional chaining `?.` in C#).
*   If a key in a path does not exist, the path evaluates to `null`.
*   Navigating further into a `null` value simply returns `null`.
*   Expanding a `null` value (e.g., `null[*]`) results in a single `null`, it does not multiply the number of pivots.
*   If a `FOREACH` target evaluates to `null` or an empty array, the loop body executes `0` times without throwing an error.

### 3.8 Structural Pattern Matching (`is`, `is not`)
The `is` and `is not` operators allow for deep structural validation and variable binding against an object.
*   **Evaluation Order:** Properties in a pattern object are evaluated strictly **left-to-right**.
*   **Binding Order:** Variable binding occurs **before** any conditions within that property match are evaluated.
*   **Presence Match:** `"health"` or `health` (Checks if the key exists).
*   **Exact Match:** `"type" == "Weapon"` (Checks if the key exists and equals "Weapon").
*   **Condition Match:** `"health" > 50` (Checks if the key exists and meets the condition).
*   **Variable Binding:** `"damage" as @dmg` (Binds the value of "damage" to `@dmg` if the key exists).
*   **Binding + Condition:** `"speed" as @spd > 10` (Binds to `@spd` only if it's > 10, otherwise the match fails).

#### Negation and Binding
Negated matches (`is not`) can still bind variables. Because binding happens before the condition is evaluated, the variable state depends on whether the key exists:
*   `this is not { "type" == "Weapon" }` (Matches if type is NOT Weapon).
*   `this is not { "damage" as @dmg }` (Matches if the key "damage" does NOT exist. If it does exist, the match fails and `@dmg` is NOT bound).
*   `this is not { "damage" as @dmg > 5 }`:
    *   If `"damage"` is missing: The inner match fails. `is not` succeeds. `@dmg` is `null` (unbound).
    *   If `"damage"` is `3`: The inner match fails (`3 > 5` is false). `is not` succeeds. `@dmg` is bound to `3`.
    *   If `"damage"` is `10`: The inner match succeeds (`10 > 5` is true). `is not` fails. `@dmg` is bound to `10`.
*   `this is { "hello" != true, "hello" as @var }` (Binds every object where "hello" exists and is not `true`, and stores its value in `@var`; Important to note that the value captured by @var is not necessarily `true`, it can be `"abc"` or `5.7` or `{ ... }`, etc).

```udpl
FOREACH vessels[*].gameobjects as @go WHERE this is { "type" == "Engine", "health" > 50, "thrust" as @thr } {
    # If matched, @thr is bound and ready to use.
    power = @thr * 2;
}
```

### 3.9 Migrations & Transactions
The `MIGRATION` directive defines the execution context and transaction boundary of a script.
*   **Syntax:** `MIGRATION "Name" [, ...args];`
*   If a script contains `MIGRATION`, the engine opens a transaction buffer.
*   All mutations apply to this buffer.
*   If the script completes successfully, the buffer commits. If an error occurs (or `Throw()` is called), the buffer is discarded.
*   **No Nested Migrations:** Migrations can be executed sequentially, but **cannot be nested**. Each migration defines a strict, isolated transaction boundary.
*   **Concurrency & Isolation:** Each migration runs in complete isolation. Concurrent executions of the same migration on the same data are not allowed and must be handled by the host application's locking mechanism.
*   Helper files (libraries) should omit the `MIGRATION` header. Attempting to execute a helper file directly will result in an error.

### 3.10 The 2-Pass Include System
The `INCLUDE` directive allows modularizing scripts.
*   **Syntax:** `INCLUDE "file.udpl";`
1.  **Pass 1 (Resolution & Deduplication):** The engine scans all includes and builds a dependency graph, ensuring each file is loaded exactly once (topological sort). No `#pragma once` is needed.
    *   **Strict Mode:** If a file is missing, an exception is thrown.
    *   **Default Mode:** If a file is missing, it is ignored (includes nothing).
2.  **Pass 2 (Execution):** The engine stitches the ASTs together. Raw statements in helper files are injected directly into the transaction buffer of the main script.

### 3.11 Strict Mode (`STRICT`)
The `STRICT` directive replaces soft failures with exceptions.
*   **Syntax:** `STRICT;`
*   **Navigation:** Lookup failures (missing keys) throw an exception instead of returning `null`.
*   **Modification:** Failed mutations (e.g. type mismatches during assignment) throw an exception.
*   **Includes:** Missing files throw an exception.

### 3.12 Directive Execution Order
Directives (e.g., `MIGRATION`, `INCLUDE`, `STRICT`) can be placed anywhere in the script. However, they are **always executed first** by the runtime before any functions or statements are evaluated.
*   The runtime validates the directive name and arguments.
*   Unknown directives will cause the runtime to throw an exception.
*   Some directives may be restricted to a single occurrence (e.g., `MIGRATION`), while others can be repeated (e.g., `INCLUDE`).

### 3.14 Strict Type Checking
UDPL enforces strict type safety for all operations to prevent data corruption from unexpected input.
*   **Relational Operators (`==`, `!=`, `<`, etc.):** Both operands must have the **exact same type**. Comparing an `int` to a `float` (e.g., `1 == 1.0`) will throw a `TypeMismatchException`.
*   **Defensive Coding:** Users should use predicates (e.g., `IsString(@val)`) or explicit casts (e.g., `ToFloat(@intVal)`) to ensure types match before comparison.

## 4. Example Script

```udpl
MIGRATION "V1_TO_V2", "dryrun";

INCLUDE "helpers.udpl";

# Functions do not inherit 'this'. 
# You must pass the target object as an argument (@ship).
FUNC MigrateShip(@ship, @newType) {
    
    # Modify the object passed in via the variable.
    @ship.type = @newType; 
    
    # quoted key required because '$' is a special char in the key name
    @ship["$id"] = NewGuid(); 
}

# dynamic paths can be resolved using the BFL
@myPath = "vessels[*].gameobjects";

FOREACH ResolvePath(root, @myPath) as @go {
    
    # Structural Pattern Matching with Variable Binding
    FOREACH @go WHERE this is { "type" == "OldShip", "health" as @hp > 0 } {
        
        # Call function, explicitly passing 'this' (Void Context)
        MigrateShip(this, "NewShip");
        
        # Math using bound variable
        health = @hp + 50;
        
        # Structure creation using variables and derefs
        @maxHp = 100;
        
        # Create a complex object using a mix of literals, derefs, and variables
        stats = {
            hp: health,
            max: @maxHp
        };
        
        # Accessing Global state
        IF global.debugMode == true {
            log = "Processed " + name;
        }

        # Type Manipulation via BFL
        @t = StringToTypeObj(this["$type"]);
        IF @t.name == "OldNamespace.OldShip" {
            @t.name = "NewNamespace.NewShip";
            this["$type"] = TypeObjToString(@t);
        }

        # Cleanup
        Delete(health);
    }
}
```