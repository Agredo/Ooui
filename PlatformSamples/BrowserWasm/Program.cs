using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Ooui;

namespace BrowserWasm;

/// <summary>
/// The entry point of a pure browser build. There is no HttpListener and no WebSocket here:
/// the whole UI lives in this process, and the messages travel through JavaScript.
/// </summary>
[SupportedOSPlatform("browser")]
public static partial class Program
{
    public static void Main ()
    {
        // The browser is the host of this application, so the built in HTTP server stays off.
        UI.ServerEnabled = false;

        // Every message the Ooui sessions produce is handed to the page.
        WebAssemblyRuntime.SendMessages = Bridge.SendMessages;

        UI.Publish ("/", _ => DemoPage.Create ());

        Console.WriteLine ("Ooui is running in the browser, waiting for the page to start the session.");
    }

    /// <summary>
    /// Called by main.js once the .NET runtime is alive. The arguments come from the page.
    /// </summary>
    [JSExport]
    public static void StartSession (string sessionId, string path, string initialSize) =>
        UI.StartWebAssemblySession (sessionId, path, initialSize);

    /// <summary>
    /// Called by main.js for every user interaction (clicks, typing, resizes, navigation).
    /// </summary>
    [JSExport]
    public static void ReceiveMessageJson (string sessionId, string json) =>
        UI.ReceiveWebAssemblySessionMessageJson (sessionId, json);
}
