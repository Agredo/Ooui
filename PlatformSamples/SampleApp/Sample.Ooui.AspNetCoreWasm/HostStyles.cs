namespace Sample.Ooui.AspNetCoreWasm;

/// <summary>
/// The shell around the rendered MAUI page. The page brings its own styles, so this only sets the
/// page background, the font, the width of the column and the link to the other rendering path.
/// </summary>
static class HostStyles
{
    public const string Navigation = """
        <header class="sample-host">
          <span class="sample-brand">Ooui <em>×</em> MAUI 11</span>
          <span class="sample-note">the same MAUI app, rendered by ASP.NET Core on this machine</span>
          <a class="sample-switch" href="/wasm/">WebAssembly build &rsaquo;</a>
        </header>
        """;

    public const string HeadHtml = """
        <style>
            :root { color-scheme: light; }
            * { box-sizing: border-box; }
            body {
                margin: 0;
                background: #f4f6fb;
                color: #172238;
                font-family: "Segoe UI", system-ui, -apple-system, sans-serif;
                line-height: 1.5;
            }
            #ooui-body { max-width: 780px; margin: 0 auto; padding: 24px 16px 56px !important; }
            .sample-host {
                display: flex; flex-wrap: wrap; align-items: baseline; gap: 12px 18px;
                padding: 14px 24px; background: #17213f; color: #d3d0ef;
            }
            .sample-brand { font-weight: 700; letter-spacing: .4px; color: #fff; }
            .sample-brand em { color: #bdb5ff; font-style: normal; }
            .sample-note { font-size: 13px; }
            .sample-switch { margin-left: auto; color: #bdb5ff; font-size: 13px; font-weight: 600; text-decoration: none; }
            .sample-switch:hover { color: #fff; }
            button { cursor: pointer; border: none; font-family: inherit; font-weight: 600; }
            button:hover { filter: brightness(1.08); }
        </style>
        """;

    public const string MissingBundleHtml = """
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="utf-8" />
          <title>Ooui &middot; no WebAssembly bundle</title>
          <style>
            body { margin: 0; min-height: 100vh; display: grid; place-items: center; background: #f4f6fb;
                   color: #172238; font-family: "Segoe UI", system-ui, sans-serif; line-height: 1.55; }
            main { max-width: 640px; padding: 32px 36px; background: #fff; border: 1px solid #dde3f0; border-radius: 18px; }
            h1 { margin: 0 0 8px; font-size: 22px; }
            p { color: #5b6982; }
            code { display: block; margin-top: 16px; padding: 14px 16px; background: #f4f6fb; border: 1px solid #dde3f0;
                   border-radius: 10px; color: #4b3ce0; font-size: 13px; white-space: pre-wrap; }
            a { color: #4b3ce0; }
          </style>
        </head>
        <body>
          <main>
            <h1>The WebAssembly half is not built yet</h1>
            <p>The server-rendered page at <a href="/">/</a> works. The bundle for the browser is produced by a
            separate publish with a runtime identifier:</p>
            <code>dotnet publish PlatformSamples/SampleApp/Sample.Ooui.Wasm/Sample.Ooui.Wasm.csproj</code>
            <p>This host picks it up from
            <code style="display:inline;padding:2px 6px">Sample.Ooui.Wasm/bin/Release/net11.0/browser-wasm/publish</code>
            and serves it under <code style="display:inline;padding:2px 6px">/wasm/</code>.</p>
          </main>
        </body>
        </html>
        """;
}
