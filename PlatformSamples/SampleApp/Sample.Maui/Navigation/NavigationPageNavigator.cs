using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Sample.Maui.Views;
using Sample.Models;
using Sample.Services;
using Sample.ViewModels;

namespace Sample.Maui.Navigation;

/// <summary>
/// Navigates the app inside a real MAUI window: the details page is pushed onto a
/// <see cref="NavigationPage"/>, and back pops it again.
/// </summary>
/// <remarks>
/// The navigation stack is resolved when a page is shown, not when the navigator is created, so a
/// host that renders one page per request can create the navigator and the home page in any order.
/// </remarks>
public sealed class NavigationPageNavigator (
    IServiceProvider services,
    Func<INavigation?> resolveNavigation,
    ICounterService counter) : IAppNavigator
{
    /// <summary>Uses the <see cref="Microsoft.Maui.Controls.NavigationPage"/> of the live app window.</summary>
    public NavigationPageNavigator (IServiceProvider services)
        : this (
            services,
            () => Application.Current?.Windows.FirstOrDefault ()?.Page?.Navigation,
            services.GetRequiredService<ICounterService> ())
    {
    }

    public async Task NavigateAsync (AppRoute route)
    {
        if (resolveNavigation () is not { } navigation)
            return;

        if (route == AppRoute.Details) {
            if (navigation.NavigationStack.Count < 2)
                await navigation.PushAsync (CreateDetailsPage ());
            return;
        }

        await navigation.PopToRootAsync ();
    }

    public async Task GoBackAsync ()
    {
        if (resolveNavigation () is { NavigationStack.Count: > 1 } navigation)
            await navigation.PopAsync ();
    }

    // The view model comes from the container of the host, but it has to see this navigator and
    // this counter: a host that renders a page per request has neither in its container.
    DetailsPage CreateDetailsPage () =>
        new (ActivatorUtilities.CreateInstance<DetailsViewModel> (services, this, counter));
}
