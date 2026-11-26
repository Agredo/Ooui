using AspNetCoreMaui;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Ooui;
using Ooui.Maui;

// The MAUI app supplies the handlers and the dispatcher, it never renders pixels itself.
var maui = MauiApp.CreateBuilder ()
    .UseMauiApp<DemoApp> ()
    .UseOoui ()
    .Build ();

var builder = WebApplication.CreateBuilder (args);
builder.Services.AddControllers ();
// One session per browser, kept in memory, so state survives navigation and reloads.
builder.Services.AddOouiUserSessions ();
// Register the context as IMauiContext, otherwise ASP.NET cannot resolve the controller parameter.
builder.Services.AddSingleton<IMauiContext> (new MauiContext (maui.Services));

var app = builder.Build ();

// UseOoui serves /ooui.js and /ooui.ws and turns the standalone Ooui server off.
app.UseOoui ();
app.UseOouiUserSessions ();

UI.HeadHtml = DemoStyles.HeadHtml;

app.MapControllers ();
app.Run ();
