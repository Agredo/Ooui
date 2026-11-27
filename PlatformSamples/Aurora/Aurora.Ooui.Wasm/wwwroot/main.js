import { dotnet } from './dotnet.js';

const bootConfig = await fetch('./wasm-boot.json').then(response => {
    if (!response.ok) throw new Error(`Aurora could not start (${response.status}). Publish the app first.`);
    return response.json();
});
const runtime = await dotnet.withDiagnosticTracing(false).withConfig(bootConfig).create();
await runtime.runMain();
const exports = await runtime.getAssemblyExports('Aurora.Ooui.Wasm');
globalThis.oouiWasm.start(exports.Aurora.Ooui.Wasm.Program, 'main');
document.getElementById('loading').hidden = true;
