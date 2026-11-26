using System;

namespace Ooui
{
    /// <summary>
    /// The bridge from <see cref="WebAssemblySession"/> to the JavaScript host.
    /// A browser backend assigns <see cref="SendMessages"/> while starting up and
    /// forwards incoming browser events to <see cref="UI.ReceiveWebAssemblySessionMessageJson"/>.
    /// </summary>
    public static class WebAssemblyRuntime
    {
        /// <summary>
        /// Renders messages in the browser. The arguments are the session id and the
        /// JSON array of messages. Without a bridge a web assembly session cannot
        /// deliver anything to the page.
        /// </summary>
        public static Action<string, string> SendMessages { get; set; }
    }
}
