using Microsoft.AspNetCore.Mvc;
using Microsoft.Maui;
using Ooui.AspNetCore;
using Ooui.Maui;

namespace AspNetCoreMaui.Controllers;

public sealed class DemoController (IMauiContext mauiContext) : Controller
{
    [HttpGet ("/")]
    public IActionResult Index () => Render ("Ooui × MAUI 11 on ASP.NET Core", Demos.Overview ());

    [HttpGet ("/counter")]
    public IActionResult Counter () => Render ("Counter · Ooui", Demos.Counter ());

    [HttpGet ("/form")]
    public IActionResult Form () => Render ("Form · Ooui", Demos.Form ());

    [HttpGet ("/controls")]
    public IActionResult Controls () => Render ("Controls · Ooui", Demos.Controls ());

    // Referenced by the Image control on the controls page.
    [HttpGet ("/logo.svg")]
    public IActionResult Logo () => Content (Demos.LogoSvg, "image/svg+xml");

    // Every call builds a fresh page: one browser tab, one MAUI page, one Ooui session.
    ElementResult Render (string title, Microsoft.Maui.Controls.ContentPage page) =>
        new (page.ToOouiElement (mauiContext), title: title);
}
