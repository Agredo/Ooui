using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Ooui;
using Ooui.AspNetCore;
using Ooui.Maui;
using Sample.Maui;
using Sample.Models;
using Sample.Ooui.AspNetCore;
using Sample.Services;

// The MAUI app is built once: it supplies the handlers and the dispatcher. Its pages are created by
// the controller, one tree per request.
var mauiBuilder = MauiApp.CreateBuilder ();
mauiBuilder.UseMauiApp<HostApp> ();
mauiBuilder.UseOoui ();
mauiBuilder.Services.AddSampleApp (Backends.AspNetCore, useMauiNavigation: false);
var maui = mauiBuilder.Build ();

var builder = WebApplication.CreateBuilder (args);
builder.Services.AddControllers ();
// One session per browser, so the counter survives a reload.
builder.Services.AddOouiUserSessions ();
// Registered as IMauiContext, otherwise ASP.NET Core cannot resolve the controller parameter.
builder.Services.AddSingleton<IMauiContext> (new MauiContext (maui.Services));

var app = builder.Build ();

app.UseOoui ();
app.UseOouiUserSessions ();

UI.HeadHtml = HostStyles.HeadHtml;

app.MapControllers ();
app.Run ();
