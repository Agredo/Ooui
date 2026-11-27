using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;
using Aurora.Maui;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Ooui;
using Ooui.Maui;

namespace Aurora.Ooui.Wasm;

[SupportedOSPlatform("browser")]
public static partial class Program
{
    public static void Main()
    {
        UI.ServerEnabled = false;
        WebAssemblyRuntime.SendMessages = Bridge.SendMessages;
        RelativeImageSourceConverter.DocumentBaseUri = Bridge.GetDocumentBaseUri();
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.UseOoui();
        builder.Services.AddAuroraApp();
        builder.Build().Publish("/");
    }

    [JSExport]
    public static void StartSession(string sessionId, string path, string initialSize) =>
        UI.StartWebAssemblySession(sessionId, path, initialSize);

    [JSExport]
    public static void ReceiveMessageJson(string sessionId, string json) =>
        UI.ReceiveWebAssemblySessionMessageJson(sessionId, json);
}
