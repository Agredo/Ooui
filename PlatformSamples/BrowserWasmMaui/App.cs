using MauiDemoUi;
using Microsoft.Maui.Controls;

namespace BrowserWasmMaui;

public class App : Application
{
    // The same DemoPage class that PlatformSamples/AspNetCoreMauiWasm renders in Kestrel.
    protected override Window CreateWindow(IActivationState activationState) =>
        new(new DemoPage("browser"));
}