namespace AspNetCoreMaui;

/// <summary>
/// Everything the demo keeps for one browser. It lives in the Ooui user session, so it survives
/// navigation, reloads and new page instances.
/// </summary>
sealed class SiteState
{
    public const string Key = "site";

    public int Count { get; set; }
    public string CounterNote { get; set; } = "No clicks yet.";

    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Notes { get; set; } = "";
    public string FormNote { get; set; } = "Nothing saved yet.";

    public int Pages { get; set; }
}
