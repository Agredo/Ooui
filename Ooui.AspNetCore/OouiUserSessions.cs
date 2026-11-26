using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Ooui.AspNetCore
{
    /// <summary>Configures how <see cref="OouiUserSessions"/> keeps browsers apart.</summary>
    public sealed class OouiUserSessionOptions
    {
        /// <summary>Gets or sets the name of the cookie that carries the session id.</summary>
        public string CookieName { get; set; } = "ooui.sid";

        /// <summary>Gets or sets how long a session may stay unused before it is dropped.</summary>
        public TimeSpan IdleTimeout { get; set; } = TimeSpan.FromMinutes (30);

        /// <summary>Gets or sets how often expired sessions are looked for.</summary>
        public TimeSpan SweepInterval { get; set; } = TimeSpan.FromMinutes (1);
    }

    /// <summary>
    /// Keeps one <see cref="OouiUserSession"/> per browser. Register it once per application:
    /// the state has to live as long as the browser keeps coming back.
    /// </summary>
    public sealed class OouiUserSessions
    {
        readonly ConcurrentDictionary<string, OouiUserSession> sessions =
            new ConcurrentDictionary<string, OouiUserSession> (StringComparer.Ordinal);
        readonly OouiUserSessionOptions options;
        long lastSweepTicks;

        public OouiUserSessions (OouiUserSessionOptions options)
        {
            this.options = options ?? throw new ArgumentNullException (nameof (options));
            lastSweepTicks = DateTime.UtcNow.Ticks;
        }

        /// <summary>Gets the number of sessions that are currently alive.</summary>
        public int Count => sessions.Count;

        /// <summary>
        /// Returns the session for <paramref name="id"/> or starts a new one when the id is
        /// unknown, expired or missing. The id of the returned session is the one the caller
        /// has to send back to the browser as a cookie.
        /// </summary>
        public OouiUserSession GetOrCreate (string id)
        {
            SweepIfDue ();
            if (!string.IsNullOrEmpty (id) && sessions.TryGetValue (id, out var existing)) {
                existing.Touch ();
                return existing;
            }
            var created = new OouiUserSession (NewId ());
            sessions[created.Id] = created;
            return created;
        }

        /// <summary>Ends and removes the session with <paramref name="id"/>.</summary>
        public bool Remove (string id)
        {
            if (string.IsNullOrEmpty (id)) return false;
            if (!sessions.TryRemove (id, out var removed)) return false;
            removed.Dispose ();
            return true;
        }

        /// <summary>Removes every session that was not used within the idle timeout.</summary>
        public void Sweep ()
        {
            var deadline = DateTime.UtcNow - options.IdleTimeout;
            foreach (var entry in sessions) {
                if (entry.Value.LastAccess < deadline && sessions.TryRemove (entry.Key, out var expired)) {
                    expired.Dispose ();
                }
            }
        }

        void SweepIfDue ()
        {
            var now = DateTime.UtcNow;
            var last = new DateTime (Interlocked.Read (ref lastSweepTicks), DateTimeKind.Utc);
            if (now - last < options.SweepInterval) return;
            Interlocked.Exchange (ref lastSweepTicks, now.Ticks);
            Sweep ();
        }

        static string NewId () => Convert.ToHexString (System.Security.Cryptography.RandomNumberGenerator.GetBytes (16)).ToLowerInvariant ();
    }
}
