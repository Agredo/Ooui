using MauiDemoUi;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Maui;
using Ooui.AspNetCore;
using Ooui.Maui;

namespace AspNetCoreMauiWasm.Controllers;

public sealed class DemoController (IMauiContext mauiContext) : Controller
{
    // Every request builds a fresh DemoPage, exactly the class the WebAssembly sample runs.
    // Clicks keep landing in that same instance for as long as the page's WebSocket stays open.
    [HttpGet ("/")]
    public IActionResult Index () => new ElementResult (
        new DemoPage ("server").ToOouiElement (mauiContext),
        title: "Ooui × MAUI 11 × WebAssembly") {
        BodyHeaderHtml = DemoStyles.Navigation (),
    };
}
