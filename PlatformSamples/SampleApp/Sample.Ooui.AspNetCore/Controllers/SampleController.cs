using Microsoft.AspNetCore.Mvc;
using Microsoft.Maui;
using Ooui.AspNetCore;
using Ooui.Maui;
using Sample.Maui;
using Sample.Models;
using Sample.Services;

namespace Sample.Ooui.AspNetCore.Controllers;

/// <summary>
/// Serves the home page of the app. The page tree is new for every request, the numbers in it are
/// not: they live in the counter service of this browser.
/// </summary>
public sealed class SampleController (IMauiContext mauiContext) : Controller
{
    [HttpGet ("/")]
    public IActionResult Index ()
    {
        var services = mauiContext.Services;

        // The counter is the app's own service, but its state comes from the session of this
        // browser instead of from the process. Everything else stays exactly as in the MAUI app.
        var counter = new CounterService (new UserSessionStateStore (HttpContext.GetOouiUserSession ()));
        var (root, _) = SamplePages.CreateSessionRoot (services, counter);

        return new ElementResult (
            root.ToOouiElement (mauiContext),
            title: "The same MAUI app on ASP.NET Core") {
            BodyHeaderHtml = HostStyles.Navigation,
        };
    }
}
