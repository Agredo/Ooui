using Microsoft.AspNetCore.Mvc;
using Microsoft.Maui;
using Ooui.AspNetCore;
using Ooui.Maui;
using Sample.Maui;
using Sample.Services;

namespace Sample.Ooui.AspNetCoreWasm.Controllers;

/// <summary>
/// Serves the server rendering of the app at <c>/</c>. The WebAssembly build under <c>/wasm/</c> is
/// static files, so it needs no action of its own.
/// </summary>
public sealed class SampleController (IMauiContext mauiContext) : Controller
{
    [HttpGet ("/")]
    public IActionResult Index ()
    {
        var services = mauiContext.Services;

        // The counter is the app's own service, but its state comes from the session of this
        // browser instead of from the process.
        var counter = new CounterService (new UserSessionStateStore (HttpContext.GetOouiUserSession ()));
        var (root, _) = SamplePages.CreateSessionRoot (services, counter);

        return new ElementResult (
            root.ToOouiElement (mauiContext),
            title: "The same MAUI app on ASP.NET Core and WebAssembly") {
            BodyHeaderHtml = HostStyles.Navigation,
        };
    }
}
