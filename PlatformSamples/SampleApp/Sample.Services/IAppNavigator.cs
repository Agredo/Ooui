using Sample.Models;

namespace Sample.Services;

/// <summary>
/// The only thing the view models need to change pages. Each host decides what navigating means:
/// pushing a MAUI page, or setting the location of the browser window.
/// </summary>
public interface IAppNavigator
{
    Task NavigateAsync (AppRoute route);

    Task GoBackAsync ();
}
