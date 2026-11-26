using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace BrowserWasm;

/// <summary>
/// The .NET side of the JavaScript bridge. <c>Client.js</c> publishes the receiving object as
/// <c>globalThis.oouiWasm</c>, so no ES module import is needed.
/// </summary>
[SupportedOSPlatform("browser")]
static partial class Bridge
{
    [JSImport ("globalThis.oouiWasm.receiveMessages")]
    public static partial void SendMessages (string sessionId, string json);
}
