using Microsoft.Extensions.DependencyInjection;

namespace Aurora.Services;

public static class AuroraServices
{
    public static IServiceCollection AddAuroraServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IDestinationService, DestinationService>();
        services.AddSingleton<IFavoritesService, FavoritesService>();
        services.AddSingleton<ITripService, TripService>();
        return services;
    }
}
