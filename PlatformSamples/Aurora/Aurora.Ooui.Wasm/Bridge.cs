using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace Aurora.Ooui.Wasm;

[SupportedOSPlatform("browser")]
static partial class Bridge
{
    [JSImport("globalThis.oouiWasm.receiveMessages")]
    public static partial void SendMessages(string sessionId, string json);

    [JSImport("globalThis.auroraHost.getDocumentBaseUri")]
    public static partial string GetDocumentBaseUri();
}
