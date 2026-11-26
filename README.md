# Ooui Web Framework <img src="https://raw.githubusercontent.com/praeclarum/Ooui/master/Documentation/Icon.png" height="32"> 

[![Build Status](https://github.com/praeclarum/Ooui/actions/workflows/build.yml/badge.svg)](https://github.com/praeclarum/Ooui/actions/workflows/build.yml)

| Version | Package | Description |
| ------- | ------- | ----------- |
| [![NuGet Package](https://img.shields.io/nuget/v/Ooui.svg)](https://www.nuget.org/packages/Ooui) | [Ooui](https://www.nuget.org/packages/Ooui) | Core library with HTML elements and a server |
| [![NuGet Package](https://img.shields.io/nuget/v/Ooui.AspNetCore.svg)](https://www.nuget.org/packages/Ooui.AspNetCore) | [Ooui.AspNetCore](https://www.nuget.org/packages/Ooui.AspNetCore) | Integration with ASP.NET Core (`net11.0` in this fork) |
| [![NuGet Package](https://img.shields.io/nuget/v/Ooui.Forms.svg)](https://www.nuget.org/packages/Ooui.Forms) | [Ooui.Forms](https://www.nuget.org/packages/Ooui.Forms) | Xamarin.Forms backend using Ooui ([Status](Documentation/OouiFormsStatus.md)) |
| `Ooui.Maui` (experimental, source only) | [Ooui.Maui](Ooui.Maui/Ooui.Maui.csproj) | Experimental .NET MAUI 11 browser backend |
| [![NuGet Package](https://img.shields.io/nuget/v/Ooui.Wasm.svg)](https://www.nuget.org/packages/Ooui.Wasm) | [Ooui.Wasm](https://www.nuget.org/packages/Ooui.Wasm) | Package your app into a web assembly (not built on this branch, see below) |

Ooui (pronounced *weee!*) is a small cross-platform UI library for .NET that uses web technologies.

It presents a classic object-oriented UI API that controls a dumb browser. With Ooui, you get the full power of your favorite .NET programming language *plus* the ability to interact with your app using any device.


## Try it Online

Head on over to [http://ooui.mecha.parts](http://ooui.mecha.parts) to tryout the samples.

You can also load [https://s3.amazonaws.com/praeclarum.org/wasm/index.html](https://s3.amazonaws.com/praeclarum.org/wasm/index.html) to try the WebAssembly mode of Ooui running Xamarin.Forms. (That's Xamarin.Forms running right in your browser!)


## Try the Samples Locally

```bash
git clone git@github.com:praeclarum/Ooui.git
cd Ooui

dotnet run --project Samples/Samples.csproj
```

This will open the default starting page for the Samples. Now point your browser at [http://localhost:8080/shared-button](http://localhost:8080/shared-button)

You should see a button that tracks the number of times it was clicked. The source code for that button is shown in the example below.


## Building and testing this branch

Everything in the solution targets .NET 11 and builds warning-free:

```bash
dotnet build Ooui.sln
dotnet test Ooui.sln
```

`Ooui.Wasm.Build.Tasks` is excluded from the solution: it compiles the vendored `mono/linker` sources from an uninitialised git submodule and cannot build without it. Initialise the submodule and add the project back if you need the legacy web assembly packaging:

```bash
git submodule update --init Ooui.Wasm.Build.Tasks/linker
dotnet sln Ooui.sln add Ooui.Wasm.Build.Tasks/Ooui.Wasm.Build.Tasks.csproj
```


## .NET MAUI 11 (experimental)

The `Ooui.Maui` project hosts a MAUI view hierarchy on the server and renders it with Ooui elements. It uses native MAUI 11 handlers rather than the retired Compatibility renderers. Add project references to `Ooui` and `Ooui.Maui`, enable `UseMaui`, and target `net11.0`.

```csharp
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Ooui.Maui;

var app = MauiApp.CreateBuilder()
    .UseMauiApp<App>()
    .UseOoui()
    .Build();

app.Publish();
Thread.Sleep(Timeout.Infinite);
```

Run the included interactive showcase and open <http://localhost:8080/>. It demonstrates data binding, validation and focus, form editing and reset, save/busy feedback, activity filtering, scrolling, responsive cards, and independent browser sessions:

```bash
dotnet run --project PlatformSamples/ConsoleNet11/ConsoleNet11.csproj
```

Each WebSocket connection creates its own window and view hierarchy. Closing the connection disconnects the handlers and destroys that window. Shared application services are still shared, so per-user state belongs in the window or page rather than in singleton services.

### Current control support

| Area | Controls and behavior |
| ---- | --------------------- |
| Pages and layout | `ContentPage`, `ContentView`, vertical/horizontal stacks, `Grid` (automatic and explicit placement), `FlexLayout`, `ScrollView`, `Border`, `BoxView` |
| Forms | `Entry`, `Editor`, `CheckBox`, `Switch`, `Slider`, `Stepper`, `Picker`, `DatePicker`, `TimePicker`, `SearchBar` |
| Display and feedback | `Label`, `Button`, `Image`, `ProgressBar`, `ActivityIndicator`, focus requests |
| Collections | `CollectionView` with `ItemsSource` (including `INotifyCollectionChanged`), `ItemTemplate`, `EmptyView`, single/multiple selection, vertical/horizontal/grid `ItemsLayout` with `ItemSpacing` |

Common text, font, color, sizing, padding, transformation, interaction, and enabled/visibility properties are mapped by the browser handlers. Browser edits update MAUI properties and bindings, while server-side changes update live DOM form values.

### Limitations

This remains an experimental browser backend, **not a complete MAUI web platform**. `Shell`, navigation, and accessibility/localization infrastructure are not implemented. Layout uses browser CSS rather than complete MAUI text measurement and native layout semantics. `CollectionView` renders every item at once (no virtualization) and ignores grouping, headers and footers, scroll events, and `RefreshView`. `Image` currently supports URL and file-name sources, `DatePicker`/`TimePicker` do not apply custom `Format` values, and `Stepper` is represented by a number input. The built-in server hosts application logic and per-session state on the server; it is not a client-side MAUI runtime. Production deployment, authentication, reconnect behavior, and scale-out require additional application design and validation.

Run the focused handler and session tests with:

```bash
dotnet test Ooui.Maui.Tests/Ooui.Maui.Tests.csproj
```


## ASP.NET Core (net11.0)

`Ooui.AspNetCore` hosts Ooui elements inside your own ASP.NET Core app instead of the built-in server. On this branch it targets `net11.0` and uses a `Microsoft.AspNetCore.App` framework reference instead of the retired `Microsoft.AspNetCore.Mvc` 2.2 / `Microsoft.AspNetCore.WebSockets` 2.2 packages.

```csharp
var builder = WebApplication.CreateBuilder (args);
builder.Services.AddControllers ();
var app = builder.Build ();

app.UseOoui ();     // serves /ooui.js and /ooui.ws, disables the built-in server
app.MapControllers ();
app.Run ();
```

Any action can then return an Ooui element. `ElementResult` starts a pending session, renders the initial HTML, and upgrades the matching web socket request:

```csharp
public class CounterController : Controller
{
    public IActionResult Index ()
    {
        var count = 0;
        var label = new Label { Text = "0" };
        var button = new Button { Text = "Increase" };
        button.Click += (s, e) => label.Text = (++count).ToString ();

        var div = new Div ();
        div.AppendChild (label);
        div.AppendChild (button);
        return new ElementResult (div, title: "Counter");
    }
}
```

`UseOoui()` sets `UI.ServerEnabled = false`, so Kestrel is the only listener. Because `ElementResult` accepts any `Ooui.Element`, it also works with elements produced by `Ooui.Maui` — a MAUI page renders through `page.ToOouiElement (mauiContext)` and keeps live web socket updates.

The sample runs on `net11.0`:

```bash
dotnet run --project PlatformSamples/AspNetCoreMvc/AspNetCoreMvc.csproj --urls http://localhost:8099
```

Then open <http://localhost:8099/Samples/Clicker>. Note that this sample still uses its original Xamarin.Forms pages alongside plain Ooui elements.

### MAUI pages in ASP.NET Core

`PlatformSamples/AspNetCoreMaui` goes the other way round: it hosts `Microsoft.Maui.Controls` pages, renders them with `Ooui.Maui`, and serves them from Kestrel. Every request creates one MAUI page, so pages are cheap and rebuildable — the numbers and drafts a visitor expects to keep live in a per browser session (see below).

```bash
dotnet run --project PlatformSamples/AspNetCoreMaui/AspNetCoreMaui.csproj   # http://localhost:8090
```

| Route | Shows |
| --- | --- |
| `/` | `Grid`/`Border`/`Image` hero, `FlexLayout` cards |
| `/counter` | `Label` + `Button` round trip, per browser state |
| `/form` | `Entry`, `Editor`, `Picker`, `DatePicker`, `TimePicker`, `Slider`, `Stepper`, `CheckBox`, `Switch` and server-side validation |
| `/controls` | `ProgressBar`, `ActivityIndicator`, `BoxView`, `SearchBar`, nested stack layouts |
| `/reset` | clears the session and redirects to `/` |

The wiring is two lines of setup plus a controller action:

```csharp
var maui = MauiApp.CreateBuilder ()
    .UseMauiApp<App> ()
    .UseOoui ()                     // MAUI handlers + dispatcher
    .Build ();

builder.Services.AddSingleton<IMauiContext> (new MauiContext (maui.Services));
builder.Services.AddOouiUserSessions ();

SiteState BrowserState => HttpContext.GetOouiUserSession ()
    .GetOrAdd (SiteState.Key, () => new SiteState ());

public IActionResult Index () =>
    new ElementResult (Demos.Overview (BrowserState, HttpContext.GetOouiUserSession ()).ToOouiElement (mauiContext), title: "Ooui + MAUI");
```

Registering the context **as `IMauiContext`** is what lets the ASP.NET Core container resolve it in the controller. Navigation pages, Shell, `CollectionView` and gestures are not supported yet, so the sample sticks to plain `ContentPage` roots.

### Multiple pages and session state

Every request builds a fresh MAUI page, so nothing survives on the element tree. State that has to outlive a page belongs to the browser instead, which is what `OouiUserSessions` provides:

```csharp
builder.Services.AddOouiUserSessions (o => {
    o.CookieName = "ooui.sid";                     // default
    o.IdleTimeout = TimeSpan.FromMinutes (30);     // default
});

app.UseOoui ();
app.UseOouiUserSessions ();   // sets and reads the session cookie
```

`UseOouiUserSessions` gives each browser a cookie (`HttpOnly`, `SameSite=Lax`, `Secure` on HTTPS) and puts the matching session into `HttpContext.Items`, so any action can reach it:

```csharp
public IActionResult Counter ()
{
    var session = HttpContext.GetOouiUserSession ();
    var state = session.GetOrAdd (SiteState.Key, () => new SiteState ());   // one instance per browser

    var label = new Label { Text = state.Count.ToString () };
    var button = new Button { Text = "Increase" };
    button.Click += (s, e) => {
        state.Count++;
        label.Text = state.Count.ToString ();
    };

    return new ElementResult (Page (label, button), title: "Counter",
        bodyHeaderHtml: DemoStyles.Navigation (Request.Path));
}
```

Because the state is per browser, deep links, reloads and the back button all show the same page again; a second browser is a second session. The sample calls `session.Clear()` from `/reset` and redirects back to `/`; the browser keeps its cookie and its session id, only the stored values are gone.

`ElementResult.BodyHeaderHtml` renders page chrome for this response only — the sample uses it to mark the current route with `aria-current="page"` instead of the application wide `UI.BodyHeaderHtml`.

Two limits are worth knowing: the store lives in the memory of one process, so scaling out needs sticky sessions or a distributed implementation of the store, and disposed values are only released by the idle sweep (every `SweepInterval`, default one minute). Values that arrive from the server are also part of the initial HTML (`<input value="…">`, textarea content), so a reload never flashes an empty form while the web socket connects.


## Example App

Here is the complete source code to a fully collaborative button clicking app.

```csharp
using System;
using Ooui;

class Program
{
    static void Main(string[] args)
    {
        // Create the UI
        var button = new Button("Click me!");

        // Add some logic to it
        var count = 0;
        button.Click += (s, e) => {
            count++;
            button.Text = $"Clicked {count} times";
        };

        // Publishing makes an object available at a given URL
        // The user should be directed to http://localhost:8080/shared-button
        UI.Publish ("/shared-button", button);

        // Don't exit the app until someone hits return
        Console.ReadLine ();
    }
}
```

Make sure to add a reference to Ooui before you start running!

```bash
dotnet add package Ooui
dotnet run
```

With just that code, a web server that serves the HTML and web socket logic necessary for an interactive button will start.



## The Many Ways to Ooui

Ooui has been broken up into several packages to increase the variety of ways that it can be used. Here are some combinations to help you decide which way is best for you.

<table>
<thead><tr><th>Ooui</th><th>Ooui.AspNetCore</th><th>Ooui.Forms</th><th>Ooui.Wasm</th><th></th></tr></thead>

<tr>
<td>&check;</td><td></td><td></td><td></td><td><a href="https://github.com/praeclarum/Ooui/wiki/Web-DOM-with-the-Built-in-Web-Server">Web DOM with the Built-in Web Server</a></td>
</tr>

<tr>
<td>&check;</td><td>&check;</td><td></td><td></td><td>Web DOM with ASP.NET Core</td>
</tr>

<tr>
<td>&check;</td><td>&check;</td><td>&check;</td><td></td><td>Xamarin.Forms with ASP.NET Core</td>
</tr>

<tr>
<td>&check;</td><td></td><td>&check;</td><td></td><td>Xamarin.Forms with the built-in web server</td>
</tr>

<tr>
<td>&check;</td><td></td><td></td><td>&check;</td><td><a href="https://github.com/praeclarum/Ooui/wiki/Web DOM-with-Web-Assembly">Web DOM with Web Assembly</a></td>
</tr>

<tr>
<td>&check;</td><td></td><td>&check;</td><td>&check;</td><td><a href="https://github.com/praeclarum/Ooui/wiki/Xamarin.Forms-with-Web-Assembly">Xamarin.Forms with Web Assembly</a></td>
</tr>

</table>

In addition to the combinations above, the experimental `Ooui.Maui` project can be combined with `Ooui.AspNetCore`: register the Ooui handlers with `UseMauiApp<App>().UseOoui()`, expose the MAUI services as an `IMauiContext`, and return `new ElementResult (page.ToOouiElement (mauiContext))` from a controller. `PlatformSamples/AspNetCoreMaui` is a runnable example of exactly that combination.


## How it works

When the user requests a page, the page will connect to the server using a web socket. This socket is used to keep the server's in-memory model of the UI (the one you work with as a programmer) in sync with the actual UI shown to the user in their browser. This is done using a simple messaging protocol with JSON packets.

When the user clicks or otherwise interacts with the UI, those events are sent back over the web socket so that your code can deal with them.

In the case of web assembly, this same dataflow takes place. However, sockets are not used as all communication is done locally in the browser process.


## Contributing

Ooui is open source and I love merging PRs. Please fork away, and please obey the .editorconfig file. :-) Try to file issues for things that you want to work on *before* you start the work so that there's no duplicated effort. If you just want to help out, check out the issues and dive in!
