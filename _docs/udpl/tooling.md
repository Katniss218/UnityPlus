# UDPL Tooling & VS Code Integration

To provide a professional development experience for UDPL, the language will support a dedicated VS Code extension. This extension will provide syntax highlighting, linting, and language intelligence (autocomplete, hover info) by leveraging the existing C# parser via the Language Server Protocol (LSP).

## Architecture Overview

The VS Code support is divided into three distinct components:

1.  **Syntax Highlighting (TextMate Grammar)**: Fast, regex-based highlighting that runs natively in VS Code.
2.  **Language Server (C# LSP)**: A background process written in C# that reuses the actual game engine's UDPL parser for perfect parity in linting and semantics.
3.  **Extension Wrapper (TypeScript)**: A lightweight glue layer that registers the grammar and manages the lifecycle of the C# Language Server.

---

## 1. Syntax Highlighting: TextMate Grammars

VS Code uses TextMate grammars (`.tmLanguage.json`) for high-performance syntax highlighting.

### Key Token Mappings (TextMate Grammar)

TextMate grammars handle the primary "first-pass" highlighting. To ensure compatibility with most VS Code themes (like One Dark, Monokai, or Solarized), we map UDPL concepts to standard scopes.

| UDPL Concept | Pattern | TextMate Scope |
| :--- | :--- | :--- |
| **Directives** | `STRICT`, `INCLUDE`, `MIGRATION` | `keyword.other.directive.udpl` |
| **Control Flow** | `IF`, `ELSE`, `FOREACH`, `WHERE`, `RETURN`, `TO`, `as`, `is`, `not` | `keyword.control.udpl` |
| **Function Def** | `FUNC` | `storage.type.function.udpl` |
| **Function Name** | `myFunc` in `FUNC myFunc()` | `entity.name.function.udpl` |
| **Variables** | `@[a-zA-Z_]\w*` | `variable.other.readwrite.udpl` |
| **Properties** | `key` in `key: value` or `obj.key` | `variable.other.property.udpl` |
| **Language Vars** | `root`, `global`, `this` | `variable.language.udpl`, `variable.language.this.udpl` |
| **Booleans** | `true`, `false` | `constant.language.boolean.udpl` |
| **Null** | `null` | `constant.language.null.udpl` |
| **Comparison** | `==`, `!=`, `<=`, `>=`, `<`, `>` | `keyword.operator.comparison.udpl` |
| **Assignment** | `=`, `+=`, `-=`, `*=`, `/=`, `%=`, `??=` | `keyword.operator.assignment.udpl` |
| **Logical** | `AND`, `OR`, `NOT`, `!`, `??` | `keyword.operator.logical.udpl` |
| **Braces** | `{`, `}` | `punctuation.section.block.begin/end.udpl` |
| **Brackets** | `[`, `]` | `punctuation.section.bracket.begin/end.udpl` |
| **Parentheses** | `(`, `)` | `punctuation.section.parenthesis.begin/end.udpl` |
| **Strings** | `"[^"\\]*(\\.[^"\\]*)*"` | `string.quoted.double.udpl` |
| **Numbers** | `\b\d+(\.\d+)?\b` | `constant.numeric.udpl` |
| **Comments** | `#.*$` | `comment.line.number-sign.udpl` |

---

## 2. Semantic Highlighting (LSP)

While TextMate is fast, it lacks context. The C# Language Server will emit **Semantic Tokens** to provide high-fidelity highlighting that understands the symbol table.

### Semantic Token Types & Modifiers

The LSP will return tokens using the following standard types, which allow themes to apply more specific styling than regex allows:

| Token Type | Modifier | Application |
| :--- | :--- | :--- |
| `function` | `defaultLibrary` | **BFL Functions**: `NewGuid`, `Log`, `Delete`, `Append`, etc. |
| `function` | `declaration` | **User Functions**: The name following the `FUNC` keyword. |
| `parameter` | | **Function Parameters**: Variables defined in a `FUNC` signature. |
| `variable` | | **Variables**: All usages of `@` variables. |
| `property` | | **Object Keys**: Identifiers accessed via dot notation (e.g., `root.players`). |
| `keyword` | | **BFL Constants**: Semantic verification of keywords. |

### Why Semantic Highlighting?
1.  **BFL vs User Functions**: Regex cannot easily distinguish between a built-in BFL function and a user-defined one. LSP knows the difference.
2.  **Member Access**: Highlighting `players` in `root.players` as a `property` requires a stateful parser.
3.  **Variable Scope**: Semantic tokens can highlight a variable differently if it is a parameter vs a local assignment.

---

## 3. Linting & Semantics: Language Server Protocol (LSP)

Instead of reimplementing the UDPL parser in TypeScript, we will use a **C# Language Server**. This ensures that the editor's feedback exactly matches the runtime behavior of the game engine.

### Implementation Stack
*   **Library**: `OmniSharp.Extensions.LanguageServer` (NuGet).
*   **Backend**: A standalone C# Console Application (`UDPL.LanguageServer.exe`) that references the core `UDPL.Parser.dll`.

### How it Works
1.  **Document Sync**: When a `.udpl` file is opened or edited, VS Code sends the text content to the C# Language Server.
2.  **Parsing**: The server runs the actual `UdplParser.Parse(text)` method.
3.  **Diagnostics**: If the parser encounters a syntax error or a semantic violation (e.g., `TypeMismatchException`), the server converts these into `Diagnostic` objects.
4.  **Reporting**: The server sends the diagnostics back to VS Code, which renders them as red squiggly lines.

### Example Diagnostic Mapping
```csharp
// Mapping a Parser Exception to an LSP Diagnostic
var diagnostics = parserErrors.Select(e => new Diagnostic {
    Range = new Range(e.Line, e.Column, e.Line, e.Column + e.Length),
    Severity = DiagnosticSeverity.Error,
    Message = e.Message, // e.g., "Right hand side must evaluate to a single object"
    Source = "UDPL Linter"
});
```

---

## 3. The Extension Wrapper (TypeScript)

A lightweight VS Code extension written in TypeScript acts as the entry point.

### Responsibilities
*   **Activation**: Launches the C# Language Server executable when a UDPL file is opened.
*   **Communication**: Uses the `vscode-languageclient` NPM package to establish a JSON-RPC connection with the C# server.
*   **Configuration**: Provides user settings for the extension (e.g., path to the C# server, strict mode defaults).
