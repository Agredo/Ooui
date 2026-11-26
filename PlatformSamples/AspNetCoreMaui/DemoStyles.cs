namespace AspNetCoreMaui;

static class DemoStyles
{
    /// <summary>
    /// Renders the navigation bar for one response and marks the page that is currently shown.
    /// </summary>
    public static string Navigation (string currentPath)
    {
        var builder = new System.Text.StringBuilder ();
        builder.Append ("<header class=\"ooui-nav\">\n");
        builder.Append ("  <a class=\"ooui-brand\" href=\"/\">Ooui <span>×</span> MAUI 11</a>\n");
        builder.Append ("  <nav>");
        var page = Normalize (currentPath);
        foreach (var (path, label) in Links) {
            var current = string.Equals (page, path, System.StringComparison.OrdinalIgnoreCase)
                ? " aria-current=\"page\""
                : "";
            builder.Append ("\n    <a href=\"").Append (path).Append ('"').Append (current).Append ('>').Append (label).Append ("</a>");
        }
        builder.Append ("\n  </nav>\n</header>");
        return builder.ToString ();
    }

    static readonly (string Path, string Label)[] Links = {
        ("/", "Overview"),
        ("/counter", "Counter"),
        ("/form", "Form"),
        ("/people", "People"),
        ("/controls", "Controls"),
        ("/reset", "Reset session"),
    };

    static string Normalize (string path)
    {
        if (string.IsNullOrEmpty (path)) return "/";
        return path.Length > 1 ? path.TrimEnd ('/') : path;
    }

    public const string HeadHtml = """
        <style>
            :root { color-scheme: light; }
            * { box-sizing: border-box; }
            body { margin: 0; background: #f5f7fc; color: #172238; font-family: Inter, "Segoe UI", sans-serif; }
            #ooui-body { padding: 28px 24px 56px !important; max-width: 1040px; margin: auto; }
            .ooui-nav { display: flex; align-items: center; justify-content: space-between; gap: 16px; flex-wrap: wrap;
                        padding: 16px 24px; background: #17213f; }
            .ooui-nav a { color: #d3d0ef; text-decoration: none; font-size: 15px; }
            .ooui-nav a:hover { color: #fff; }
            .ooui-nav a[aria-current="page"] { color: #fff; font-weight: 600; box-shadow: inset 0 -2px 0 #8f7dff; }
            .ooui-nav nav { display: flex; gap: 20px; flex-wrap: wrap; }
            .ooui-brand { font-weight: 700; letter-spacing: .5px; color: #fff !important; }
            .ooui-brand span { color: #8f7dff; }
            input:not([type="checkbox"]):not([type="range"]), textarea, select {
                width: 100%; min-height: 44px; padding: 11px 13px; border: 1px solid #d9dfeb; border-radius: 10px;
                background: #fff; color: #172238; font: inherit; outline: none; transition: border-color .15s, box-shadow .15s; }
            input:focus, textarea:focus, select:focus { border-color: #6854d9; box-shadow: 0 0 0 3px #6854d91c; }
            input[type="checkbox"] { width: 22px; height: 22px; margin: 0; accent-color: #6854d9; }
            input[type="range"] { width: 100%; height: 28px; accent-color: #6854d9; cursor: pointer; }
            textarea { resize: vertical; }
            button { padding: 11px 18px; border-radius: 10px; cursor: pointer; font-family: inherit;
                     transition: filter .15s, transform .15s; }
            button:hover { filter: brightness(1.06); transform: translateY(-1px); }
            button:active { transform: translateY(0); }
            button:disabled { opacity: .55; cursor: default; transform: none; }
            progress { width: 100%; height: 10px; accent-color: #6854d9; }
            .ooui-activity { animation: ooui-spin .9s linear infinite; font-size: 26px; }
            .ooui-switch { appearance: none; width: 46px !important; height: 26px !important; border-radius: 20px;
                           background: #ccd3df; position: relative; cursor: pointer; transition: background .2s; }
            .ooui-switch::after { content: ""; position: absolute; top: 3px; left: 3px; width: 20px; height: 20px;
                                  border-radius: 50%; background: #fff; transition: transform .2s; }
            .ooui-switch:checked { background: #6854d9; }
            .ooui-switch:checked::after { transform: translateX(20px); }
            [role="listbox"] > [role="option"], [role="list"] > [role="listitem"] {
                border-radius: 16px; transition: box-shadow .15s; }
            [role="listbox"] > [role="option"] { cursor: pointer; }
            [role="listbox"] > [role="option"]:hover { box-shadow: 0 0 0 2px #d9d3ff; }
            [role="listbox"] > [role="option"][aria-selected="true"] { box-shadow: 0 0 0 2px #6854d9; }
            [role="listbox"] > [role="option"]:focus-visible,
            [role="list"] > [role="listitem"]:focus-visible { outline: 2px solid #8f7dff; outline-offset: 2px; }
            @keyframes ooui-spin { to { transform: rotate(360deg); } }
            @media (max-width: 720px) {
                #ooui-body { padding: 16px 12px 40px !important; }
            }
        </style>
        """;
}
