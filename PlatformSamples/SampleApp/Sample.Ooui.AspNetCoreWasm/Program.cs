using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Ooui;
using Ooui.AspNetCore;
using Ooui.Maui;
using Sample.Maui;
using Sample.Models;
using Sample.Ooui.AspNetCoreWasm;
using Sample.Services;

// One MAUI app for both rendering paths: the server path builds its page tree per request from these
// services, the WebAssembly path runs App itself in the browser.
var mauiBuilder = MauiApp.CreateBuilder ();
mauiBuilder.UseMauiApp<HostApp> ();
mauiBuilder.UseOoui ();
mauiBuilder.Services.AddSampleApp (Backends.AspNetCoreWasm, useMauiNavigation: false);
var maui = mauiBuilder.Build ();

var builder = WebApplication.CreateBuilder (args);
builder.Services.AddControllers ();
builder.Services.AddOouiUserSessions ();
// Registered as IMauiContext, otherwise ASP.NET Core cannot resolve the controller parameter.
builder.Services.AddSingleton<IMauiContext> (new MauiContext (maui.Services));

var app = builder.Build ();

// UseOoui serves /ooui.js and /ooui.ws and turns the standalone Ooui server off.
app.UseOoui ();
app.UseOouiUserSessions ();

UI.HeadHtml = HostStyles.HeadHtml;

app.MapControllers ();

// The WebAssembly path: the very same MAUI app compiled for the browser and published as
// WebAssembly. It is a plain folder of static files, so a file provider is all it needs.
var bundle = WasmBundle.Find (app.Environment.ContentRootPath, app.Configuration);

// A bare "/wasm" is not a file, so it is answered before the static file middleware. The redirect to
// "/wasm/" matters, because the bundle references its files relatively. Without a bundle the answer
// explains how to create one. This cannot be an endpoint: routing also matches "/wasm/" and would
// redirect it to itself.
app.Use ((context, next) => {
    if (!HttpMethods.IsGet (context.Request.Method)
        || !context.Request.Path.Equals ("/wasm", StringComparison.OrdinalIgnoreCase))
        return next (context);

    if (bundle is not null) {
        context.Response.Redirect ("/wasm/");
        return Task.CompletedTask;
    }

    context.Response.ContentType = "text/html; charset=utf-8";
    return context.Response.WriteAsync (HostStyles.MissingBundleHtml);
});

if (bundle is not null) {
    var files = new PhysicalFileProvider (bundle);
    // Assemblies and their data files have no media type, so they serve as opaque bytes.
    var contentTypes = new FileExtensionContentTypeProvider ();
    contentTypes.Mappings[".dll"] = "application/octet-stream";
    contentTypes.Mappings[".dat"] = "application/octet-stream";
    app.UseDefaultFiles (new DefaultFilesOptions { FileProvider = files, RequestPath = "/wasm" });
    app.UseStaticFiles (new StaticFileOptions {
        FileProvider = files,
        RequestPath = "/wasm",
        ContentTypeProvider = contentTypes,
    });
    app.Logger.LogInformation ("Serving the MAUI WebAssembly bundle from {Bundle} at /wasm/.", bundle);
}
else {
    app.Logger.LogWarning (
        "No MAUI WebAssembly bundle found, so /wasm will only explain how to build one. "
        + "Run: dotnet publish PlatformSamples/SampleApp/Sample.Ooui.Wasm/Sample.Ooui.Wasm.csproj");
}

app.Run ();
