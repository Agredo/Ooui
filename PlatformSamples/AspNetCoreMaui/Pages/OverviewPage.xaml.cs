using Microsoft.Maui.Controls;
using Ooui.AspNetCore;

namespace AspNetCoreMaui.Pages;

public partial class OverviewPage : ContentPage
{
    public OverviewPage (SiteState state, OouiUserSession session)
    {
        InitializeComponent ();

        var name = string.IsNullOrWhiteSpace (state.Name) ? "not entered yet" : state.Name;
        StateLabel.Text = $"Counter: {state.Count} · pages seen: {state.Pages} · name: {name}";
        SessionLabel.Text = $"Session {session.Id[..8]} · stored in memory while you keep browsing. "
            + "Navigation, reload and the back button all keep it.";
    }
}
