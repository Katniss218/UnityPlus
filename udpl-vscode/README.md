# UDPL VS Code Extension

This extension provides comprehensive language support for the UnityPlus Data Programming Language (UDPL).

## Features

- **Syntax Highlighting**: Robust TextMate grammar for all UDPL keywords, variables, and literals.
- **Language Server Integration**: Powered by a C# Language Server for perfect parity with the game engine's runtime logic.
- **Intelligent Indentation**: Auto-indentation for blocks and brackets.
- **Bracket Matching**: Automatic pairing of braces, brackets, and parentheses.

## Project Structure

```text
.
├── src/
│   └── extension.ts          # Extension entry point & LSP client setup
├── syntaxes/
│   └── udpl.tmLanguage.json  # TextMate grammar for syntax highlighting
├── language-configuration.json # Bracket matching and indentation rules
├── package.json              # Extension manifest
├── tsconfig.json             # TypeScript configuration
└── README.md                 # This file
```

## Architecture

The extension follows the standard VS Code Language Server Protocol (LSP) architecture:

1. **Client**: A TypeScript-based VS Code extension that handles TextMate highlighting and launches the server.
2. **Server**: A C# executable (`./server/UDPL.LanguageServer.exe`) that leverages the actual UDPL parser to provide semantic analysis, linting, and hover info.

## Development

### Prerequisites

- [Node.js](https://nodejs.org/) (v16.x or higher)
- [TypeScript](https://www.typescriptlang.org/)
- [Visual Studio Code](https://code.visualstudio.com/)
- [.NET SDK](https://dotnet.microsoft.com/download) (for server development)

### Getting Started

1. **Install Dependencies**:
   ```bash
   cd udpl-vscode
   npm install
   ```

2. **Compile the Extension**:
   ```bash
   npm run compile
   ```

3. **Debug the Extension**:
   - Open the `udpl-vscode` folder in VS Code.
   - Press `F5` to open a new "Extension Development Host" window.
   - Create or open a `.udpl` file to activate the extension.

## Build Process

### 1. Grammar & Configuration
Edits to `syntaxes/udpl.tmLanguage.json` or `language-configuration.json` are applied immediately upon reloading the Extension Development Host.

### 2. TypeScript Client
The client source in `src/extension.ts` must be compiled using `npm run compile`. It uses `vscode-languageclient` to manage the lifecycle of the C# server.

### 3. C# Language Server
The server component must be compiled separately and placed in the `server/` directory as `UDPL.LanguageServer.exe`. The client is currently configured to look for the executable at that path relative to the extension root.

## Release
To package the extension into a `.vsix` file for distribution:
1. Install `vsce` globally: `npm install -g @vscode/vsce`
2. Run `vsce package` in the extension root.
