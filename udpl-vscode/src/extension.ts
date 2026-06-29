import * as path from 'path';
import { workspace, ExtensionContext } from 'vscode';
import {
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    Executable
} from 'vscode-languageclient/node';

let client: LanguageClient;

export function activate(context: ExtensionContext) {
    // The server is implemented in C# and compiled to an executable
    // For development, we assume it's in a 'server' directory
    const serverModule = context.asAbsolutePath(
        path.join('server', 'UDPL.LanguageServer.exe')
    );

    // If the extension is launched in debug mode then the debug server options are used
    // Otherwise the run options are used
    const serverOptions: ServerOptions = {
        run: { command: serverModule, args: [] },
        debug: { command: serverModule, args: [] }
    };

    // Options to control the language client
    const clientOptions: LanguageClientOptions = {
        // Register the server for UDPL files
        documentSelector: [{ scheme: 'file', language: 'udpl' }],
        synchronize: {
            // Notify the server about file changes to '.udpl files contained in the workspace
            fileEvents: workspace.createFileSystemWatcher('**/*.udpl')
        }
    };

    // Create the language client and start the client.
    client = new LanguageClient(
        'udplLanguageServer',
        'UDPL Language Server',
        serverOptions,
        clientOptions
    );

    // Start the client. This will also launch the server
    client.start();
}

export function deactivate(): Thenable<void> | undefined {
    if (!client) {
        return undefined;
    }
    return client.stop();
}
