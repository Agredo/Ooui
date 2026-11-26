using System.Collections.Concurrent;

namespace Sample.Services;

/// <summary>
/// The default store: state lives as long as the process (WebAssembly) or the registered singleton
/// (ASP.NET Core host). Hosts that need a narrower scope replace it with their own implementation.
/// </summary>
public sealed class InMemoryStateStore : IStateStore
{
    readonly ConcurrentDictionary<string, object> values = new ();

    public T GetOrAdd<T> (string key, Func<T> factory) where T : class
    {
        ArgumentNullException.ThrowIfNull (key);
        ArgumentNullException.ThrowIfNull (factory);
        return (T) values.GetOrAdd (key, _ => factory ());
    }

    public void Remove (string key)
    {
        ArgumentNullException.ThrowIfNull (key);
        values.TryRemove (key, out _);
    }
}
