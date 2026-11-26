namespace Sample.Ooui.AspNetCore;

/// <summary>
/// The shell around the rendered MAUI page. The page brings its own styles, so this only sets the
/// page background, the font and the width of the column.
/// </summary>
static class HostStyles
{
    public const string Navigation = """
        <header class="sample-host">
          <span class="sample-brand">Ooui <em>×</em> MAUI 11</span>
          <span class="sample-note">the same MAUI app, rendered by ASP.NET Core on this machine</span>
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
            button { cursor: pointer; border: none; font-family: inherit; font-weight: 600; }
            button:hover { filter: brightness(1.08); }
        </style>
        """;
}
