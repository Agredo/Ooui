using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Sample.Models;

namespace Sample.Services;

/// <summary>Registers the services every host of the sample app needs.</summary>
public static class SampleServices
{
    /// <summary>
    /// Adds the models' services for one backend. The store and the counter are registered with
    /// <c>TryAdd</c>, so a host can replace them with a version that has its own lifetime.
    /// </summary>
    public static IServiceCollection AddSampleServices (this IServiceCollection services, BackendInfo backend)
    {
        ArgumentNullException.ThrowIfNull (services);
        ArgumentNullException.ThrowIfNull (backend);
        services.TryAddSingleton (backend);
        services.TryAddSingleton<IReadOnlyList<BackendInfo>> (_ => Backends.All);
        services.TryAddSingleton<IStateStore, InMemoryStateStore> ();
        services.TryAddSingleton<ICounterService, CounterService> ();
        return services;
    }
}
