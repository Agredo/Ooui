using Aurora.Maui.Navigation;
using Aurora.Maui.Views;
using Aurora.Services;
using Aurora.ViewModels;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Aurora.Maui;

public static class AuroraAppServices
{
    public static IServiceCollection AddAuroraApp(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddAuroraServices();
        services.TryAddSingleton<IAuroraNavigator, AuroraNavigator>();
        services.TryAddSingleton<ExploreViewModel>();
        services.TryAddTransient<DestinationDetailViewModel>();
        services.TryAddTransient<FavoritesViewModel>();
        services.TryAddSingleton<ExplorePage>();
        services.TryAddTransient<DestinationPage>();
        services.TryAddTransient<FavoritesPage>();
        return services;
    }
}
