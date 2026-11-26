using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Sample.Ooui.AspNetCoreWasm;

/// <summary>
/// The application of the server rendering path. Every response builds its own page tree, so the
/// window of the app is never shown and never holds any content. The WebAssembly path under
/// <c>/wasm</c> runs <see cref="Sample.Maui.App"/> instead, in the browser.
/// </summary>
public sealed class HostApp : Application
{
    protected override Window CreateWindow (IActivationState? activationState) =>
        new (new ContentPage ());
}
