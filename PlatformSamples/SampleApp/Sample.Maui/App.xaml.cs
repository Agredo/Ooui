using Microsoft.Maui.Controls;

namespace Sample.Maui;

/// <summary>
/// The MAUI application of the sample: one window with the home page inside a
/// <see cref="NavigationPage"/>, so the second page is pushed on top of it.
/// </summary>
public partial class App : Application
{
    readonly IServiceProvider services;

    /// <summary>Creates the application with the container that resolves its pages.</summary>
    public App (IServiceProvider services)
    {
        this.services = services;
        InitializeComponent ();
    }

    protected override Window CreateWindow (IActivationState? activationState) =>
        new (SamplePages.CreateRootPage (services));
}
