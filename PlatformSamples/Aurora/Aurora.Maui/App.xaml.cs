using Aurora.Maui.Views;
using Microsoft.Maui.Controls;

namespace Aurora.Maui;

public partial class App : Application
{
    readonly IServiceProvider services;

    public App(IServiceProvider services)
    {
        this.services = services;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var navigation = new NavigationPage(services.GetRequiredService<ExplorePage>())
        {
            BarBackgroundColor = Color.FromArgb("#173F35"),
            BarTextColor = Colors.White,
        };
        NavigationPage.SetHasNavigationBar(navigation, false);
        return new Window(navigation);
    }
}
