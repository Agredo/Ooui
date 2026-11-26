namespace AspNetCoreMaui;

/// <summary>The small SVG logo on the controls page, served by the controller.</summary>
static class LogoSvg
{
    public const string Svg = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 96 96">
          <rect x="4" y="4" width="88" height="88" rx="22" fill="#17213f"/>
          <circle cx="48" cy="54" r="19" fill="none" stroke="#8f7dff" stroke-width="9"/>
          <path d="M34 28h28" stroke="#8f7dff" stroke-width="9" stroke-linecap="round"/>
        </svg>
        """;
}
