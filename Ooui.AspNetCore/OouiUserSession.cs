using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ooui.AspNetCore
{
    /// <summary>
    /// State that belongs to one browser rather than to one page. It survives navigation,
    /// reloads and new page instances, so it is the place for counters, form drafts and
    /// preferences. Keep it small: the session lives in the memory of the server process.
    /// </summary>
    public sealed class OouiUserSession : IDisposable
    {
        readonly ConcurrentDictionary<string, object> items = new ConcurrentDictionary<string, object> (StringComparer.Ordinal);
        long lastAccessTicks;

        internal OouiUserSession (string id)
        {
            Id = id;
            lastAccessTicks = DateTime.UtcNow.Ticks;
        }

        /// <summary>Gets the identifier that is stored in the session cookie.</summary>
        public string Id { get; }

        /// <summary>Gets the UTC time of the last request that used this session.</summary>
        public DateTime LastAccess => new DateTime (Interlocked.Read (ref lastAccessTicks), DateTimeKind.Utc);

        /// <summary>Gets the number of stored values.</summary>
        public int Count => items.Count;

        internal void Touch () => Interlocked.Exchange (ref lastAccessTicks, DateTime.UtcNow.Ticks);

        /// <summary>
        /// Returns the stored value for <paramref name="key"/>, creating and storing it with
        /// <paramref name="factory"/> when the session does not have it yet.
        /// </summary>
        public T GetOrAdd<T> (string key, Func<T> factory) where T : class
        {
            if (key == null) throw new ArgumentNullException (nameof (key));
            if (factory == null) throw new ArgumentNullException (nameof (factory));
            return (T)items.GetOrAdd (key, _ => factory ());
        }

        /// <summary>Stores <paramref name="value"/> under <paramref name="key"/>.</summary>
        public void Set<T> (string key, T value) where T : class
        {
            if (key == null) throw new ArgumentNullException (nameof (key));
            if (value == null) throw new ArgumentNullException (nameof (value));
            items[key] = value;
        }

        /// <summary>Reads a stored value without creating one.</summary>
        public bool TryGetValue<T> (string key, out T value) where T : class
        {
            if (key != null && items.TryGetValue (key, out var stored) && stored is T typed) {
                value = typed;
                return true;
            }
            value = null;
            return false;
        }

        /// <summary>Removes a stored value, disposing it when it is disposable.</summary>
        public bool Remove (string key)
        {
            if (key == null) return false;
            if (!items.TryRemove (key, out var removed)) return false;
            DisposeIfNeeded (removed);
            return true;
        }

        /// <summary>Removes every stored value, disposing the ones that are disposable.</summary>
        public void Clear ()
        {
            foreach (var key in items.Keys) {
                if (items.TryRemove (key, out var removed))
                    DisposeIfNeeded (removed);
            }
        }

        /// <summary>Removes every stored value and ends the session.</summary>
        public void Dispose () => Clear ();

        static void DisposeIfNeeded (object value)
        {
            if (value is IDisposable disposable) {
                try {
                    disposable.Dispose ();
                }
                catch (Exception) {
                    // A value that cannot be disposed must not break the navigation of the browser.
                }
            }
        }
    }
}
