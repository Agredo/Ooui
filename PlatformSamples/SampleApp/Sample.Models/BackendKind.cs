namespace Sample.Models;

/// <summary>The Ooui web backends this sample can run on.</summary>
public enum BackendKind
{
    /// <summary>The app itself is compiled to WebAssembly and runs inside the tab.</summary>
    WebAssembly,

    /// <summary>Kestrel renders the pages and keeps them alive over a WebSocket.</summary>
    AspNetCore,

    /// <summary>A Kestrel host that also serves the WebAssembly bundle of the same app.</summary>
    AspNetCoreWasm,
}
