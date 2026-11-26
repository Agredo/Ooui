using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Sample.Ooui.AspNetCore;

/// <summary>
/// The application of this host. ASP.NET Core creates the page tree of every response itself, so the
/// window of the app is never shown and never holds any content.
/// </summary>
public sealed class HostApp : Application
{
    protected override Window CreateWindow (IActivationState? activationState) =>
        new (new ContentPage ());
}
