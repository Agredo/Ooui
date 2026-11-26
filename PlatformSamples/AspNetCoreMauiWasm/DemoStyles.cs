namespace AspNetCoreMauiWasm;

/// <summary>
/// The page shell around the server-rendered MAUI page. It deliberately uses the same palette as
/// <c>BrowserWasmMaui/wwwroot/index.html</c>, so switching between the two rendering paths of the
/// same <c>DemoPage</c> does not feel like switching applications.
/// </summary>
static class DemoStyles
{
    /// <summary>Header with the switch between both rendering paths, rendered above the page.</summary>
    public static string Navigation () => """
        <header class="ooui-nav">
          <a class="ooui-brand" href="/">Ooui <span>&times;</span> MAUI 11</a>
          <nav>
            <a href="/" aria-current="page">Rendered by Kestrel</a>
            <a href="/wasm/">Same page in WebAssembly</a>
          </nav>
        </header>
        """;

    public const string MissingBundleHtml = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <title>Ooui &middot; no WebAssembly bundle</title>
          <style>
            body { margin: 0; min-height: 100vh; display: grid; place-items: center; background: #0e1826;
                   color: #eef2f9; font-family: "Segoe UI", system-ui, sans-serif; line-height: 1.55; }
            main { max-width: 640px; padding: 32px 36px; background: #172238; border: 1px solid #253350; border-radius: 18px; }
            h1 { margin: 0 0 8px; font-size: 22px; }
            p { color: #8492aa; }
            code { display: block; margin-top: 16px; padding: 14px 16px; background: #101a2b; border: 1px solid #253350;
                   border-radius: 10px; color: #8f7dff; font-size: 13px; white-space: pre-wrap; }
            a { color: #8f7dff; }
          </style>
        </head>
        <body>
          <main>
            <h1>The WebAssembly half is not built yet</h1>
            <p>The server-rendered page at <a href="/">/</a> works. The bundle for the browser is produced by a
            separate publish with a runtime identifier:</p>
            <code>dotnet publish PlatformSamples/BrowserWasmMaui/BrowserWasmMaui.csproj</code>
            <p>This host picks it up from <code style="display:inline;padding:2px 6px">BrowserWasmMaui/bin/Release/net11.0/browser-wasm/publish</code>
            and serves it under <code style="display:inline;padding:2px 6px">/wasm/</code>.</p>
          </main>
        </body>
        </html>
        """;

    public const string HeadHtml = """
        <style>
            :root { color-scheme: dark; }
            * { box-sizing: border-box; }
            body {
                margin: 0; padding: 0 0 64px;
                background: radial-gradient(1200px 600px at 15% -10%, #1d2b47 0%, #0e1826 55%);
                color: #eef2f9;
                font-family: "Segoe UI", system-ui, -apple-system, sans-serif;
                line-height: 1.55;
            }
            .ooui-nav {
                display: flex; align-items: center; justify-content: space-between; gap: 16px; flex-wrap: wrap;
                padding: 14px 24px; background: #101a2b; border-bottom: 1px solid #253350;
            }
            .ooui-nav a { color: #8492aa; text-decoration: none; font-size: 14px; }
            .ooui-nav a:hover { color: #eef2f9; }
            .ooui-nav a[aria-current="page"] { color: #eef2f9; font-weight: 600; box-shadow: inset 0 -2px 0 #8f7dff; }
            .ooui-nav nav { display: flex; gap: 20px; flex-wrap: wrap; }
            .ooui-brand { font-weight: 700; letter-spacing: .5px; color: #eef2f9 !important; }
            .ooui-brand span { color: #8f7dff; }
            #ooui-body {
                max-width: 760px; margin: 28px auto 0; padding: 32px 34px 40px !important;
                background: #172238; border: 1px solid #253350; border-radius: 18px;
            }
            input:not([type="checkbox"]):not([type="range"]), textarea, select {
                width: 100%; min-height: 44px; padding: 11px 14px; border: 1px solid #253350; border-radius: 10px;
                background: #101a2b; color: #eef2f9; font: inherit; outline: none;
                transition: border-color .15s, box-shadow .15s;
            }
            input:focus, textarea:focus, select:focus { border-color: #6854d9; box-shadow: 0 0 0 3px #6854d933; }
            input[type="range"] { width: 100%; height: 28px; margin: 0; accent-color: #6854d9; cursor: pointer; }
            progress { width: 100%; height: 10px; accent-color: #6854d9; }
            button { font-family: inherit; border: 1px solid transparent; }
            button:hover { filter: brightness(1.12); }
            .ooui-switch { appearance: none; width: 46px !important; height: 26px !important; border-radius: 20px;
                           background: #2b3a58; position: relative; cursor: pointer; transition: background .2s; }
            .ooui-switch::after { content: ""; position: absolute; top: 3px; left: 3px; width: 20px; height: 20px;
                                  border-radius: 50%; background: #fff; transition: transform .2s; }
            .ooui-switch:checked { background: #6854d9; }
            .ooui-switch:checked::after { transform: translateX(20px); }
            .ooui-activity, .ooui-activity-indicator { animation: ooui-spin .9s linear infinite; }
            @keyframes ooui-spin { to { transform: rotate(360deg); } }
            @media (max-width: 720px) {
                #ooui-body { margin: 0; border-radius: 0; border-left: 0; border-right: 0; padding: 20px 16px 32px !important; }
            }
        </style>
        """;
}
