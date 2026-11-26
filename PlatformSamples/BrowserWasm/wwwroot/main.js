import { dotnet } from './dotnet.js';

// .NET 11 does not generate a boot manifest for plain Wasm apps, so the list of runtime resources
// is produced by the build (GenerateWasmBootConfig in BrowserWasm.csproj) and loaded here.
const bootConfig = await fetch ('./wasm-boot.json').then (r => {
    if (!r.ok) {
        throw new Error (`wasm-boot.json could not be loaded (${r.status}). Publish the app first.`);
    }
    return r.json ();
});

// The page owns the runtime. Client.js has already published globalThis.oouiWasm, which the
// .NET side uses to push messages into the DOM.
const runtime = await dotnet
    .withDiagnosticTracing (false)
    .withConfig (bootConfig)
    .create ();

// Main() only publishes the pages; the runtime has to stay alive afterwards, so not runMainAndExit.
await runtime.runMain ();

const exports = await runtime.getAssemblyExports ('BrowserWasm');
globalThis.oouiWasm.start (exports.BrowserWasm.Program, 'main');
