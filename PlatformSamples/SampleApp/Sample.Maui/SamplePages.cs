using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Sample.Maui.Navigation;
using Sample.Maui.Views;
using Sample.Services;
using Sample.ViewModels;

namespace Sample.Maui;

/// <summary>Builds the page tree of the app, so every host starts at the same place.</summary>
public static class SamplePages
{
    /// <summary>
    /// The app as a window: the home page inside a <see cref="NavigationPage"/>. The hosts that keep
    /// one app alive (WebAssembly, the built-in Ooui server) use this.
    /// </summary>
    public static Page CreateRootPage (IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull (services);
        return new NavigationPage (services.GetRequiredService<MainPage> ());
    }

    /// <summary>
    /// The app as the root of a single response. The page tree is new for every request, so the
    /// caller passes the counter of the request and gets the navigator back that pushes onto this
    /// tree.
    /// </summary>
    public static (NavigationPage Root, IAppNavigator Navigator) CreateSessionRoot (
        IServiceProvider services,
        ICounterService counter)
    {
        ArgumentNullException.ThrowIfNull (services);
        ArgumentNullException.ThrowIfNull (counter);
        NavigationPage? root = null;
        var navigator = new NavigationPageNavigator (services, () => root?.Navigation, counter);
        var viewModel = ActivatorUtilities.CreateInstance<MainViewModel> (services, navigator, counter);
        root = new NavigationPage (new MainPage (viewModel));
        return (root, navigator);
    }
}
