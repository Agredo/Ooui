namespace Sample.Models;

/// <summary>The three backends the sample app is hosted on, in the order the pages list them.</summary>
public static class Backends
{
    public static BackendInfo WebAssembly { get; } = new (
        BackendKind.WebAssembly,
        "WebAssembly",
        "The app runs in the tab and the Ooui handlers write straight into the DOM.",
        "Sample.Ooui.Wasm");

    public static BackendInfo AspNetCore { get; } = new (
        BackendKind.AspNetCore,
        "ASP.NET Core",
        "Every request renders a page in Kestrel and keeps it live over a WebSocket.",
        "Sample.Ooui.AspNetCore");

    public static BackendInfo AspNetCoreWasm { get; } = new (
        BackendKind.AspNetCoreWasm,
        "ASP.NET Core + WebAssembly",
        "One process, both paths: Kestrel at / and the very same app as a bundle at /wasm/.",
        "Sample.Ooui.AspNetCoreWasm");

    public static IReadOnlyList<BackendInfo> All { get; } = [WebAssembly, AspNetCore, AspNetCoreWasm];
}
