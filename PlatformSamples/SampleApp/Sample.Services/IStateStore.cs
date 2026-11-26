namespace Sample.Services;

/// <summary>
/// Where a service keeps state that has to outlive a single page or request. The scope is up to the
/// host: a WebAssembly tab shares one store, the ASP.NET Core host uses one store per browser.
/// </summary>
public interface IStateStore
{
    /// <summary>Returns the value stored under <paramref name="key"/>, creating it once if needed.</summary>
    T GetOrAdd<T> (string key, Func<T> factory) where T : class;

    /// <summary>Drops the value stored under <paramref name="key"/>.</summary>
    void Remove (string key);
}
