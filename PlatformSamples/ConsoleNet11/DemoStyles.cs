namespace ConsoleNet11;

static class DemoStyles
{
    public const string Html = """
        <style>
            :root { color-scheme: light; }
            * { box-sizing: border-box; }
            body { margin: 0; background: #f5f7fc; color: #172238; font-family: Inter, "Segoe UI", sans-serif; }
            #ooui-body { padding: 32px 24px 48px !important; }
            #ooui-body > div { max-width: 1160px; margin: auto; }
            [data-automation-id="hero"] { padding: 32px; border-radius: 24px; background: linear-gradient(125deg, #17213f, #38316b 58%, #5141a0); box-shadow: 0 20px 55px #2a275127; }
            [data-automation-id="hero-title"] { font-size: clamp(32px, 5vw, 54px) !important; line-height: 1.1; letter-spacing: -1.5px; max-width: 720px; }
            [data-automation-id="feature-grid"] { grid-template-columns: minmax(260px, .85fr) minmax(300px, 1.15fr); }
            [data-automation-id="overview-grid"] { grid-template-columns: repeat(3, minmax(0, 1fr)); }
            [data-automation-id="workload-grid"] { grid-template-columns: minmax(220px, 1fr) minmax(220px, 1fr); }
            [data-automation-id="activity"] { height: 265px; padding-right: 8px; }
            input:not([type="checkbox"]):not([type="range"]), textarea, select { width: 100%; min-height: 46px; padding: 12px 14px; border: 1px solid #d9dfeb; border-radius: 10px; outline: none; background: #fff; color: #172238; font: inherit; transition: border-color .15s, box-shadow .15s; }
            input::placeholder, textarea::placeholder { color: var(--ooui-placeholder-color, #7a859c); }
            input:focus, textarea:focus, select:focus { border-color: #6854d9; box-shadow: 0 0 0 3px #6854d91c; }
            input[type="checkbox"] { width: 22px; height: 22px; margin: 0; accent-color: #6854d9; }
            input[type="range"] { width: 100%; accent-color: #6854d9; cursor: pointer; }
            textarea { resize: vertical; }
            button { padding: 12px 18px; border-radius: 10px; cursor: pointer; font-family: inherit; transition: filter .15s, transform .15s; }
            button:hover { filter: brightness(1.06); transform: translateY(-1px); }
            button:active { transform: translateY(0); }
            progress { width: 100%; height: 10px; accent-color: #6854d9; }
            .ooui-activity { animation: ooui-spin .9s linear infinite; font-size: 28px; }
            .ooui-switch { appearance: none; width: 46px !important; height: 26px !important; border-radius: 20px; background: #ccd3df; position: relative; cursor: pointer; transition: background .2s; }
            .ooui-switch::after { content: ""; position: absolute; top: 3px; left: 3px; width: 20px; height: 20px; border-radius: 50%; background: white; transition: transform .2s; }
            .ooui-switch:checked { background: #6854d9; }
            .ooui-switch:checked::after { transform: translateX(20px); }
            @keyframes ooui-spin { to { transform: rotate(360deg); } }
            @media (max-width: 800px) {
                #ooui-body { padding: 16px 12px 32px !important; }
                [data-automation-id="hero"] { padding: 24px; }
                [data-automation-id="feature-grid"], [data-automation-id="overview-grid"], [data-automation-id="workload-grid"] { grid-template-columns: minmax(0, 1fr) !important; }
                [data-automation-id="feature-grid"] > *, [data-automation-id="overview-grid"] > *, [data-automation-id="workload-grid"] > * { grid-row-start: auto !important; grid-column-start: auto !important; grid-column-end: auto !important; }
                [data-automation-id="activity"] { height: 230px; }
            }
        </style>
        """;
}
