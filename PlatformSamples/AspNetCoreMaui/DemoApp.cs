using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace AspNetCoreMaui;

public sealed class DemoApp : Application
{
    // Every request builds its own page, so the application window never holds any content.
    protected override Window CreateWindow (IActivationState? activationState) =>
        new (new ContentPage ());
}
