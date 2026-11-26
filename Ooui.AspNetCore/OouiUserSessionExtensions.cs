using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ooui.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class OouiUserSessionServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the store that keeps one <see cref="OouiUserSession"/> per browser.
        /// </summary>
        public static IServiceCollection AddOouiUserSessions (this IServiceCollection services, Action<OouiUserSessionOptions> configure = null)
        {
            if (services == null) throw new ArgumentNullException (nameof (services));

            var options = new OouiUserSessionOptions ();
            configure?.Invoke (options);

            services.TryAddSingleton (options);
            services.TryAddSingleton<OouiUserSessions> ();
            return services;
        }
    }
}

namespace Microsoft.AspNetCore.Builder
{
    public static class OouiUserSessionApplicationBuilderExtensions
    {
        /// <summary>
        /// Loads the <see cref="OouiUserSession"/> of the browser into the request, so pages can
        /// keep state across navigation, reloads and new page instances.
        /// </summary>
        public static IApplicationBuilder UseOouiUserSessions (this IApplicationBuilder app)
        {
            if (app == null) throw new ArgumentNullException (nameof (app));
            return app.UseMiddleware<OouiUserSessionMiddleware> ();
        }
    }
}
