using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Ooui;
using Ooui.Maui;
using Sample.Maui;
using Sample.Models;
using Sample.Services;

namespace Sample.Ooui.Wasm;

/// <summary>
/// Runs the pure MAUI app in the browser: <see cref="App"/> builds the window and the pages, the
/// Ooui handlers render them into the DOM, and messages travel through the JavaScript bridge.
/// </summary>
[SupportedOSPlatform("browser")]
public static partial class Program
{
    public static void Main ()
    {
        // The page owns the rendering, so the built-in Ooui server is not started.
        UI.ServerEnabled = false;
        WebAssemblyRuntime.SendMessages = Bridge.SendMessages;

        var builder = MauiApp.CreateBuilder ();
        builder.UseMauiApp<App> ();
        builder.UseOoui ();
        builder.Services.AddSampleApp (Backends.WebAssembly);
        builder.Build ().Publish ("/");
    }

    [JSExport]
    public static void StartSession (string sessionId, string path, string initialSize) =>
        UI.StartWebAssemblySession (sessionId, path, initialSize);

    [JSExport]
    public static void ReceiveMessageJson (string sessionId, string json) =>
        UI.ReceiveWebAssemblySessionMessageJson (sessionId, json);
}
