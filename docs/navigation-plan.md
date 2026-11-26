# Plan: Navigation for Ooui.Maui (NavigationPage, TabbedPage, URL sync)

Goal: MAUI pages can move between pages inside one browser session (`Navigation.PushAsync`,
`PopAsync`, `TabbedPage`) and the browser URL and back button follow along.
Work in small steps. After every step: build, run the tests, fix, and only then continue.

## 0. Ground rules (read first)

- Repo `C:\Projects\OOUI`, branch `maui-11-port`, Windows, PowerShell. Target `net11.0`, MAUI 11.
- The solution must build with **0 warnings, 0 errors**: `dotnet build Ooui.sln -c Release --nologo`.
- Do **not** commit or push unless the user says so. Never use `git stash/reset/rebase`.
- Code and comments in English, short comments only where something is non-obvious.
- Before building, stop running sample hosts, otherwise DLLs are locked. Stop by PID only:
  `Get-CimInstance Win32_Process -Filter "Name='dotnet.exe'" | Where CommandLine -like '*AspNetCoreMaui*'`
  then `Stop-Process -Id <pid>`. Port 8080 (ConsoleNet11) must be free for `dotnet test Ooui.sln`.
- PowerShell has no heredoc. Use here-strings piped to `python -` for Playwright scripts.
  Set `$env:PYTHONIOENCODING='utf-8'`. Playwright scripts go in
  `C:\Users\chris\.copilot\session-state\13218a70-2e74-4e2f-937a-91a06ac7cb74\files\` (not in the repo).
- Existing tests: `dotnet test Ooui.Maui.Tests\Ooui.Maui.Tests.csproj -c Release --nologo` (29 tests, must stay green).

## 1. How things work today (facts, verified in code)

- Handlers live in `Ooui.Maui\Handlers\`, derive from `OouiViewHandler<TVirtualView, TPlatformView>`
  (see `ContentViewHandler.cs` as the model). They define a `public static readonly IPropertyMapper Mapper`,
  create an Ooui `Element` in `CreatePlatformView()`, hook events in `ConnectHandler`, unhook in `DisconnectHandler`.
- Registration is in `Ooui.Maui\OouiMauiExtensions.cs` (add one line per handler).
- `view.ToOouiElement(context)` (in `HandlerExtensions.cs`) creates the handler and returns the Ooui element.
- `ContentPage` is rendered by `ContentViewHandler` (it implements `IContentView`).
- `OouiMauiSession` (a `Div`) creates the `Window` from `Application.CreateWindow` and renders `window.Content`.
  `Window.Content` can therefore already be a `NavigationPage` or `TabbedPage`, they just have no handler yet.
- ASP.NET sample `PlatformSamples\AspNetCoreMaui` builds a fresh page per request (`DemoController.Render`).
  `ConsoleNet11` has ONE long-lived `MainPage` per browser session. Navigation targets the second model.
- Tests create a context like this (copy from `Ooui.Maui.Tests\FormControlTests.cs`):
  `app.Services.CreateScope()`, `new MauiContext(scope.ServiceProvider)`, `view.ToOouiElement(context)`,
  and simulate browser input with `element.Receive(Message.Event(element.Id, "click", ...))`.
- Browser events reach the server as `Message.Event`. `Client.js` already sends `window` `hashchange`
  events (`initializeNavigation`, `monitorHashChanged`). Find how the server side exposes them:
  `grep -rn "hashchange" Ooui\*.cs`.
- Lesson from RadioButton: check that `Client.js` / `Ooui\*.cs` really deliver the value you rely on
  (the radio change event needed a fix in both). Verify new browser-to-server paths with Playwright, not only unit tests.

## Step 1 – Probe the MAUI 11 API (no production code)

Do not guess MAUI interface names. Use the existing probe project `MauiHandlerApiProbe` (or a scratch console
project outside the repo) and print/inspect:

1. Interfaces `IStackNavigationView`, `IStackNavigation`, `INavigationView`, `ITabbedView`.
2. What MAUI's own `NavigationPageHandler` maps: property mapper keys and the **CommandMapper**
   (`RequestNavigation`). `NavigationRequest` has `NavigationStack` (`IReadOnlyList<IView>`) and `Animated`.
3. What `TabbedPage` needs on non-native platforms (`Microsoft.Maui.Controls.TabbedPage`, its `Children`,
   `CurrentPage`). Check with reflection which interfaces the virtual view implements and which handler contract is expected.
4. Record the findings at the end of THIS file under "Findings". If an interface does not exist
   as assumed, adapt Steps 2–4 to what really exists and write the change under "Findings".

Done when: findings are written down and nothing else changed.

## Step 2 – `NavigationPageHandler` (push / pop inside one session)

Create `Ooui.Maui\Handlers\NavigationPageHandler.cs`.

Design:
- Virtual view: `Microsoft.Maui.Controls.NavigationPage` (as `IStackNavigationView`).
- Platform view: `Div` (flex column, `min-height: 100%`) containing
  1. a **bar** `Div` (back `Button` + title `Span`) – hidden when `NavigationPage.HasNavigationBar` is false
     or when the stack has one page and no title,
  2. a **host** `Div` that holds exactly the element of the top page.
- Implement a `CommandMapper` entry `nameof(IStackNavigationView.RequestNavigation)` with
  `RequestNavigation(handler, view, args)`:
  1. `args` is a `NavigationRequest`. Take `NavigationStack`.
  2. Rebuild the host: remove old child, create the element of the last page via `page.ToOouiElement(MauiContext)`,
     append it. Cache the elements per page in a `Dictionary<IView, Element>` so popping back keeps state
     (do not re-create). Remove cache entries for pages that left the stack and call `DisconnectHandlers()` on them.
  3. Update the bar title from the top page's `Title` and show the back button when `NavigationStack.Count > 1`.
  4. At the end call `VirtualView.NavigationFinished(args.NavigationStack)` (required, otherwise
     `PushAsync` never completes and the app hangs).
- Back button: on `Click` run `_ = VirtualView.PopAsync();`.
- Subscribe to the top page's `PropertyChanged` for `Title` while it is on top; unsubscribe when it leaves.
- `ConnectHandler` must render the initial stack (verify in Step 1 notes and in test 1 whether the first
  `RequestNavigation` arrives on its own).
- Register in `OouiMauiExtensions.cs`: `handlers.AddHandler<NavigationPage, NavigationPageHandler>();`
  (same pattern as the other registrations in that file).

Tests (new `Ooui.Maui.Tests\NavigationTests.cs`, 3 focused tests):
1. `NavigationPageShowsRootPage`: root `ContentPage` with a `Label`; the rendered tree contains the label text.
2. `PushAndPopSwapThePage`: `await nav.PushAsync(second)`; assert second's label is shown and first's is not;
   click the back button element via `Receive(Message.Event(id, "click", ...))`; assert the first page is back.
   If `PushAsync` needs a window, create `new Window(nav)` first.
3. `HasNavigationBarFalseHidesTheBar`.

Done when: tests green, solution builds with 0 warnings.

## Step 3 – URL / back-button sync

Goal: pushing a page changes the browser hash, the browser back button pops.
Use the **hash** (`#/pagename`) so no server routing is needed and it works for Kestrel, Kestrel+Wasm and the console host.

- Add `Ooui.Maui\NavigationUrl.cs` with a helper that turns a page into a segment: `page.StyleId` if set,
  otherwise the type name in kebab-case (`SettingsPage` → `settings`). The stack is the source of truth,
  the hash is derived from it (`#/a/b/c`).
- In `NavigationPageHandler.RequestNavigation`, after the stack changed, set the browser hash. Find the existing
  Ooui API first (`grep -rn "location" Ooui\*.cs Ooui.Maui\*.cs`). `Client.js` `msgSet` supports dotted keys, so a
  `set` message for `window` with key `location.hash` should work – verify. If the server has no API for it, add the
  smallest possible helper in `Ooui`.
- On `hashchange` from the browser: parse the hash; if it equals the stack minus its last page
  (`#/a/b` after `#/a/b/c`) call `PopAsync()`; otherwise ignore (deep links are out of scope).
  Avoid loops: remember the last hash the server set and ignore the echo event for it.
- One test: simulate the hashchange event and assert the stack was popped.

Done when: pushing changes the hash and the browser back button returns (checked in Step 6).

## Step 4 – `TabbedPageHandler`

Create `Ooui.Maui\Handlers\TabbedPageHandler.cs` (use the contract found in Step 1).

- Platform view: `Div` (flex column) = **tab bar** `Div` (one `Button` per child page, text = `Page.Title`,
  active tab gets accent styling and `aria-selected="true"`) + **content** `Div` with the current page element.
- Rebuild the tab bar when `Children` changes (`ObservableCollection`; subscribe in `ConnectHandler`, unsubscribe in
  `DisconnectHandler`) and when `CurrentPage` changes (`PropertyChanged` for `nameof(TabbedPage.CurrentPage)`).
- Click on a tab: set `VirtualView.CurrentPage = page`. The `CurrentPage` change drives the render, so both
  directions stay consistent.
- Cache page elements per page (like Step 2) so tab switches keep state.
- Register: `handlers.AddHandler<TabbedPage, TabbedPageHandler>();`
- Tests (2): tabs render with titles; clicking the second tab shows the second page and updates `CurrentPage`.

## Step 5 – Sample (ConsoleNet11, XAML)

Do NOT change the existing `MainPage` behaviour.

1. Add `PlatformSamples\ConsoleNet11\Pages\DetailsPage.xaml(.cs)`: small ContentPage with a Title, a Label showing a
   value passed through the constructor, and a Button "Back" calling `Navigation.PopAsync()`.
2. In `MainPage.xaml` add a Button "Open details" (AutomationId `open-details`); its handler does
   `await Navigation.PushAsync(new Pages.DetailsPage(model.ProjectName))`.
3. In `App.cs` wrap: `new Window(new NavigationPage(new MainPage()))`.
4. XAML stays compiled, 0 warnings.

## Step 6 – Verify in a real browser

1. Stop hosts, build, start `ConsoleNet11` detached on 8080
   (`dotnet PlatformSamples\ConsoleNet11\bin\Release\net11.0\ConsoleNet11.dll`).
2. Playwright script (copy helpers from `verify_console.py` in the session `files` folder) must check:
   - click `open-details` → details text visible, main page gone, URL hash changed;
   - click the in-app back button → main page is back **with the edited project name still in the Entry**;
   - push again, then `page.go_back()` → main page is back;
   - no JS errors (`page.on("pageerror")`).
3. Re-run `verify_console.py` (13 checks) – must still pass. Keep its selectors working (hide the bar on the root page).
4. Full suite: `dotnet build Ooui.sln -c Release --nologo`, then
   `dotnet test Ooui.sln -c Release --nologo --no-build` (port 8080 free!). Expected: all green, 0 warnings.

## Step 7 – Docs

- `README.md`: add `NavigationPage` and `TabbedPage` to the control table, one paragraph on hash-based sync,
  and update the "not implemented" sentence. `Shell` stays listed as not implemented.

## Out of scope (do not start without asking)

`Shell`, `FlyoutPage`, deep links that rebuild a stack from a URL, page transition animations,
server-side routing in the ASP.NET sample, WebView.

## Definition of done

- New handlers, registration and about 6 new tests green; previous 29 + 78 tests still green.
- `dotnet build Ooui.sln -c Release` has 0 warnings and 0 errors.
- Playwright run in Step 6 passes with no JS errors.
- README updated. Nothing committed until the user approves.
- Report to the user in German, short: what works, what was verified, what is still open.

## Findings (Step 1, verified against MAUI 11.0.0 and the Ooui sources)

MAUI contracts (reflection over `Microsoft.Maui.dll` and `Microsoft.Maui.Controls.dll`, 11.0.0):

- `Microsoft.Maui.IStackNavigationView : IView, IElement, ITransform, IStackNavigation`.
  `IStackNavigation` declares `void RequestNavigation(NavigationRequest)` and
  `void NavigationFinished(IReadOnlyList<IView>)`.
- There is **no** `INavigationView` interface in MAUI. The platform-agnostic handler is
  `Microsoft.Maui.Handlers.NavigationViewHandler : ViewHandler<IStackNavigationView, object>`;
  its property mapper is empty and its command mapper has exactly one key: `RequestNavigation`
  (argument is a `NavigationRequest`). `INavigationViewHandler` only exposes `VirtualView` and `PlatformView`.
  Step 2 therefore uses `IStackNavigationView`/`NavigationPage` and the key `RequestNavigation`.
- `Microsoft.Maui.Controls.NavigationPage : Page, IStackNavigationView, IStackNavigation, ...` implements both
  `RequestNavigation` and `NavigationFinished` **explicitly** and drives the handler through its internal
  `SendHandlerUpdateAsync`. The handler must call `NavigationFinished` with the stack it rendered, otherwise
  `PushAsync` never completes. Stack-relevant API: `NavigationPage.CurrentPage`, `NavigationPage.RootPage`,
  `NavigationPage.Peek(index)`, attached `NavigationPage.GetHasNavigationBar(page)`, `Title` (`ITitledElement`),
  `BarBackgroundColor`, `BarTextColor`.
- `ITabbedView` is an empty marker interface. `Microsoft.Maui.Handlers.TabbedViewHandler :
  ViewHandler<ITabbedView, object>` has an empty property mapper, an empty command mapper, and
  `ITabbedViewHandler` only exposes `VirtualView`/`PlatformView`. There is no navigation request for tabs:
  the handler pulls from the virtual view.
- `Microsoft.Maui.Controls.TabbedPage : MultiPage<Page>, ITabbedView, IItemsView<Page>, IMultiPageController<Page>`
  with `IList<Page> Children`, `Page CurrentPage`, `BarBackgroundColor`, `BarTextColor`, `SelectedTabColor`,
  `UnselectedTabColor`. `CurrentPage` raises `PropertyChanged`. Step 4 uses those members directly.
- `OouiViewHandler<TVirtualView, TPlatformView>` offers a constructor overload with a `CommandMapper`, so a new
  handler can chain `OouiViewMapper.CommandMapper` and add a `RequestNavigation` entry.

Ooui plumbing (relevant for Steps 2 and 3):

- Server → browser: `Message.Set("window", "location.hash", value)` works. `Client.js` `msgSet` splits the key on
  `.` and walks `window.location.hash`; `getNode("window")` returns the real `window`. `Ooui.Window.Location`
  exists but sets the whole `location` string, which is not what we need.
- Browser → server: `Client.js` sends `{m:"event", id:"window", k:"hashchange", v:window.location}` once after
  connecting and on every `hashchange`. The value arrives as a `JObject` containing `hash`.
  **Today that message is dropped**: sessions call `element.Receive(message)`, and `EventTarget`/`Node` only
  dispatch to elements inside the tree, so `TargetId == "window"` finds nobody. Step 3 therefore adds the
  smallest possible delivery path in `Ooui`: a `Window` event can be received on the document of the session's
  root element (the tree root forwards unmatched `window` events to `OwnerDocument.Window`).

## Status (implemented)

All steps ran in order; the findings above were confirmed and the plan was followed without changes to the
out-of-scope list.

- Step 2: `Ooui.Maui\Handlers\NavigationPageHandler.cs` (`IStackNavigationView`, `RequestNavigation`/`NavigationFinished`,
  title bar, back button, `HasNavigationBar`, per-page element cache).
- Step 3: `Ooui.Maui\NavigationUrl.cs`, `Ooui\Window.SetHash/ReplaceHash`, a `window` event path through
  `Element.TriggerEventFromMessage`, the `hash` payload in `EventTarget` and `Client.js`; pushes use
  `location.hash`, pops use `history.replaceState`, so the browser back button pops the server-side stack.
- Step 4: `Ooui.Maui\Handlers\TabbedPageHandler.cs` (`ITabbedView`, tab bar from `Children`/`CurrentPage`).
- Step 5: `PlatformSamples\ConsoleNet11\Pages\DetailsPage.xaml(.cs)`, "Open details" button, `App.cs` wraps `MainPage`
  in a `NavigationPage`.
- Step 6: Playwright checks: root hash `#/main`, push to `#/main/details`, in-app back keeps the edited entry,
  browser back works, no JS errors; the existing 13-check `verify_console.py` still passes.
- Step 7: `README.md` (control table, hash-sync paragraph, limitations).
- Result: 45 MAUI tests + 78 core tests green, 0 warnings, nothing committed.
