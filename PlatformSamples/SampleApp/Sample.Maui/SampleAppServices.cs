using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sample.Maui.Navigation;
using Sample.Maui.Views;
using Sample.Models;
using Sample.Services;
using Sample.ViewModels;

namespace Sample.Maui;

/// <summary>Registers the views and view models of the app in the container of a host.</summary>
public static class SampleAppServices
{
    /// <summary>
    /// Adds the app to a container: views, view models and everything in <see cref="SampleServices"/>.
    /// </summary>
    /// <param name="services">The container of the host.</param>
    /// <param name="backend">The backend the host renders this instance of the app with.</param>
    /// <param name="useMauiNavigation">
    /// <c>true</c> when the host keeps one live app, so the app can push pages onto its
    /// <see cref="Microsoft.Maui.Controls.NavigationPage"/>. Hosts that render a page per request
    /// pass <c>false</c> and register their own <see cref="IAppNavigator"/>.
    /// </param>
    public static IServiceCollection AddSampleApp (
        this IServiceCollection services,
        BackendInfo backend,
        bool useMauiNavigation = true)
    {
        ArgumentNullException.ThrowIfNull (services);
        services.AddSampleServices (backend);
        services.TryAddTransient<MainViewModel> ();
        services.TryAddTransient<DetailsViewModel> ();
        services.TryAddTransient<MainPage> ();
        services.TryAddTransient<DetailsPage> ();
        if (useMauiNavigation)
            services.TryAddSingleton<IAppNavigator, NavigationPageNavigator> ();
        return services;
    }
}
