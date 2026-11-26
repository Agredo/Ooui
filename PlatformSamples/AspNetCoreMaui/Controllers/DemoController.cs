using Microsoft.AspNetCore.Mvc;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Ooui.AspNetCore;
using Ooui.Maui;

namespace AspNetCoreMaui.Controllers;

public sealed class DemoController (IMauiContext mauiContext) : Controller
{
    // The state of this browser, not of this request. It survives navigation and reloads.
    SiteState BrowserState {
        get {
            var state = HttpContext.GetOouiUserSession ().GetOrAdd (SiteState.Key, () => new SiteState ());
            state.Pages++;
            return state;
        }
    }

    [HttpGet ("/")]
    public IActionResult Index () => Render ("Ooui × MAUI 11 on ASP.NET Core", Demos.Overview (BrowserState, HttpContext.GetOouiUserSession ()));

    [HttpGet ("/counter")]
    public IActionResult Counter () => Render ("Counter · Ooui", Demos.Counter (BrowserState));

    [HttpGet ("/form")]
    public IActionResult Form () => Render ("Form · Ooui", Demos.Form (BrowserState));

    [HttpGet ("/people")]
    public IActionResult People () => Render ("People · Ooui", Demos.People (BrowserState));

    [HttpGet ("/controls")]
    public IActionResult Controls () => Render ("Controls · Ooui", Demos.Controls ());

    // Referenced by the Image control on the controls page.
    [HttpGet ("/logo.svg")]
    public IActionResult Logo () => Content (Demos.LogoSvg, "image/svg+xml");

    // Throws the state of this browser away and starts over with a fresh session.
    [HttpGet ("/reset")]
    public IActionResult Reset ()
    {
        HttpContext.GetOouiUserSession ().Clear ();
        return Redirect ("/");
    }

    // Every request builds a fresh page: state lives in the user session, never in the page.
    ElementResult Render (string title, ContentPage page) =>
        new (page.ToOouiElement (mauiContext), title: title) {
            BodyHeaderHtml = DemoStyles.Navigation (Request.Path),
        };
}
