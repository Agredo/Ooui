# Plan 4: WebView, FlyoutPage and more containers

Read `docs\navigation-plan.md` section 0 (ground rules) and section 1 first. They apply here too.
`TabbedPage` is already covered in `docs\navigation-plan.md` (Step 4), do not duplicate it.
Do this plan AFTER the navigation plan, because `FlyoutPage` reuses its page-caching idea.

Goal: three quick wins that make sites look complete: `WebView` (embed content), `FlyoutPage` (side menu), `TableView`-free
settings pages are NOT in scope.

## Step 1 – Probe (no production code)

1. Does Ooui have an iframe element? `grep -rn "iframe" Ooui\*.cs`. If there is no `Iframe` class, create
   `Ooui\Iframe.cs` modelled on `Ooui\Image.cs` (a small `Element("iframe")` subclass with `Source` → attribute `src`,
   `SourceDocument` → `srcdoc`, `Sandbox` → `sandbox`, and a `Load` event via `AddEventListener("load", ...)`).
   Check `Client.js` `elementEvents` already contains `load` (it does).
2. Which interfaces does MAUI 11 define: `IWebView`, `IWebViewHandler`, `IFlyoutView`, `IFlyoutViewHandler`?
   Inspect with the `MauiHandlerApiProbe` project. Note command names in the CommandMapper of `WebViewHandler`
   (`GoBack`, `GoForward`, `Reload`, `Eval`, `EvaluateJavaScriptAsync`).
3. Write findings at the end of this file.

## Step 2 – `WebViewHandler`

Create `Ooui.Maui\Handlers\WebViewHandler.cs` for `Microsoft.Maui.Controls.WebView`.

- Platform view: the `Iframe` (full width, `border: 0`, `min-height: 240px` unless `HeightRequest` is set; `OouiViewMapper`
  handles size).
- Mapper `Source` (`IWebView.Source`):
  - `UrlWebViewSource` → set `src` (only `http://`, `https://` and relative URLs; anything else, e.g. `javascript:`, is **not
    set** and the iframe stays blank. This is a security rule, add a test).
  - `HtmlWebViewSource` → set `srcdoc` to its `Html`.
- **Security default**: always set `sandbox="allow-scripts allow-forms allow-popups"` WITHOUT `allow-same-origin`, so embedded
  content cannot read the Ooui page. Document it in a comment.
- Events: iframe `load` → `((IWebView)VirtualView).Navigated(WebNavigationEvent.NewPage, source, url, WebNavigationResult.Success)`.
  `Navigating` cannot be intercepted from the parent; do not raise it (note in README).
- CommandMapper (if the probe shows these names): `Reload` → re-assign `src`; `GoBack`/`GoForward` → not possible
  cross-origin: leave unimplemented and make `CanGoBack`/`CanGoForward` false.
- Register: `handlers.AddHandler<WebView, WebViewHandler>();`.
- Tests: Url source sets `src`; Html source sets `srcdoc`; `javascript:alert(1)` leaves `src` unset; sandbox attribute present
  and does not contain `allow-same-origin`.

## Step 3 – `FlyoutPageHandler`

Create `Ooui.Maui\Handlers\FlyoutPageHandler.cs` for `Microsoft.Maui.Controls.FlyoutPage`.

- Platform view: a flex row `Div` with an `aside` panel (flyout, `Flyout` page, width 280px, border-right) and a `main`
  area (detail, flex 1).
- Narrow screens: add a hamburger `Button` (☰) in a top bar that toggles `IsPresented`; the aside is then
  `position: fixed`, full height, shown only when `IsPresented` is true. Use a CSS media query? Ooui has only inline
  styles, so decide by `FlyoutLayoutBehavior`: `Split` = always visible, `Popover` = toggled (default for `Default`),
  and expose `IsPresented` both ways (`PropertyChanged` for `IsPresented`, click handlers set it).
- Detail page: cache by page like in the navigation plan Step 2; `Detail` changes → swap. Clicking a link in the flyout
  usually sets `Detail` in user code, so just rerender.
- Register: `handlers.AddHandler<FlyoutPage, FlyoutPageHandler>();`.
- Tests: flyout and detail pages both render; toggle button flips `IsPresented`; setting `Detail` swaps content;
  `Popover` hides the aside when not presented (`display:none`).

## Step 4 – `ListView` is in plan 2, `TableView` is out of scope

Do not add them here.

## Step 5 – Sample and browser check

1. `PlatformSamples\AspNetCoreMaui`: add `Pages\EmbedPage.xaml` (route `/embed`, add to `DemoController` and
   `DemoStyles.Navigation`, same pattern as `/controls`): a `WebView` with `HtmlWebViewSource` (small inline HTML with
   a heading and a button that changes text via script) plus a `Label` explaining the sandbox.
2. Playwright: iframe exists with `sandbox` attribute; `frame_locator` can find the heading; the inline script button works
   (script is allowed); no JS errors on the parent page.
3. If the navigation plan is done, add a `FlyoutPage` sample in `ConsoleNet11` only if it is a 20-line change,
   otherwise skip the sample and rely on the tests.
4. Full build (0 warnings), all tests, existing verify scripts still pass.

## Step 6 – Docs

README: add `WebView` and `FlyoutPage` to the control table, remove `WebView` from the "not supported" text in
`PlatformSamples\AspNetCoreMaui\Pages\ControlsPage.xaml` ("Not supported yet" card), mention the sandbox default and that
`Navigating` is not raised.

## Out of scope

Cookies for WebView, JavaScript interop (`EvaluateJavaScriptAsync`), `HybridWebView`, `BlazorWebView`.

## Definition of done

New tests green (about 8), existing tests green, 0 warnings, Playwright check passes, README updated, nothing committed
without user approval. Final report to the user in German, short.

## Findings (fill in during Step 1)

_(empty)_
