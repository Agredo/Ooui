namespace AspNetCoreMaui;

static class DemoStyles
{
    public const string NavigationHtml = """
        <header class="ooui-nav">
          <a class="ooui-brand" href="/">Ooui <span>×</span> MAUI 11</a>
          <nav>
            <a href="/">Overview</a>
            <a href="/counter">Counter</a>
            <a href="/form">Form</a>
            <a href="/controls">Controls</a>
          </nav>
        </header>
        """;

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
            @keyframes ooui-spin { to { transform: rotate(360deg); } }
            @media (max-width: 720px) {
                #ooui-body { padding: 16px 12px 40px !important; }
            }
        </style>
        """;
}
