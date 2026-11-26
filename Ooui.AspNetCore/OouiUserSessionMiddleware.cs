using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Ooui.AspNetCore
{
    /// <summary>
    /// Gives every request access to the <see cref="OouiUserSession"/> of its browser and makes
    /// sure the browser gets a session cookie when it does not have one yet.
    /// </summary>
    public sealed class OouiUserSessionMiddleware
    {
        internal const string SessionKey = "OouiUserSession";

        readonly RequestDelegate next;
        readonly OouiUserSessionOptions options;

        public OouiUserSessionMiddleware (RequestDelegate next, OouiUserSessionOptions options)
        {
            this.next = next ?? throw new ArgumentNullException (nameof (next));
            this.options = options ?? throw new ArgumentNullException (nameof (options));
        }

        public async Task InvokeAsync (HttpContext context)
        {
            var store = context.RequestServices.GetRequiredService<OouiUserSessions> ();
            var sessionId = context.Request.Cookies.TryGetValue (options.CookieName, out var cookieValue) ? cookieValue : null;
            var session = store.GetOrCreate (sessionId);

            if (!string.Equals (session.Id, sessionId, StringComparison.Ordinal)) {
                context.Response.Cookies.Append (options.CookieName, session.Id, new CookieOptions {
                    HttpOnly = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax,
                    Secure = context.Request.IsHttps,
                    Path = "/",
                    MaxAge = options.IdleTimeout,
                });
            }

            context.Items[SessionKey] = session;

            await next (context);
        }
    }

    public static class OouiUserSessionHttpContextExtensions
    {
        /// <summary>
        /// Returns the session of the current browser. Requires
        /// <c>UseOouiUserSessions()</c> in the request pipeline.
        /// </summary>
        public static OouiUserSession GetOouiUserSession (this HttpContext context)
        {
            if (context == null) throw new ArgumentNullException (nameof (context));
            if (context.Items.TryGetValue (OouiUserSessionMiddleware.SessionKey, out var value) && value is OouiUserSession session)
                return session;
            throw new InvalidOperationException ("No Ooui user session is available. Add app.UseOouiUserSessions() to the request pipeline.");
        }
    }
}
