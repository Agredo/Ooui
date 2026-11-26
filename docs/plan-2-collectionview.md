# Plan 2: CollectionView and list controls

Read `docs\navigation-plan.md` section 0 (ground rules) and section 1 (how handlers work) first. They apply here too.

Goal: `CollectionView` becomes good enough for real data pages: fast updates, header/footer, grouping,
`RefreshView`, and a simple `ListView`. Today (`Ooui.Maui\Handlers\CollectionViewHandler.cs`) every collection
change calls `Rebuild()`, which removes and re-creates ALL item elements, and header, footer and grouping are ignored.

Work in the order below. After every step: build (0 warnings), run `Ooui.Maui.Tests`, then continue.
Tests go into the existing `Ooui.Maui.Tests\CollectionViewTests.cs` (read it first and copy its helpers).

## Step 1 – Incremental updates (performance, do this first)

Problem: `OnItemsChanged` calls `Rebuild()`. Adding one item to 500 items re-sends 500 elements to the browser.

1. In `OnItemsChanged(sender, NotifyCollectionChangedEventArgs args)` switch on `args.Action`:
   - `Add`: create containers for `args.NewItems` and insert them at `args.NewStartingIndex`
     (`PlatformView.InsertBefore(newElement, referenceElement)`; append when index == count).
     Insert the same number of entries into the `containers` list at the same index.
   - `Remove`: remove the containers at `args.OldStartingIndex` (count = `OldItems.Count`), call
     `DisconnectHandlers()` on their MAUI views is optional, but remove them from `containers`.
   - `Replace`: replace the container(s) at `args.NewStartingIndex`.
   - `Move`, `Reset` and anything unexpected: fall back to `Rebuild()`.
2. When the list goes from 0 to 1 item or from 1 to 0 items, call `Rebuild()` (empty view toggles).
3. `AddItem` currently appends. Refactor it into `ItemContainer CreateContainer(object? item)` plus the
   insert/append logic, so Add can reuse it.
4. Tests: with an `ObservableCollection<string>` of 3 items, record the element of item 1, then `Add` a 4th
   and assert that the element of item 1 is **the same object** (`Assert.AreSame`) and there are 4 children.
   Same for `RemoveAt` and `Insert(0, ...)`.

## Step 2 – Header and Footer

`ItemsView.Header`, `ItemsView.Footer` (object) and `HeaderTemplate`, `FooterTemplate` (DataTemplate) exist on `StructuredItemsView`.

1. Add mapper entries for `Header`, `HeaderTemplate`, `Footer`, `FooterTemplate` (all call `Rebuild()`).
2. The platform view must hold: header element, then items, then footer element. Keep header and footer elements
   in fields and make the insert index in Step 1 `+1` when a header exists. Simplest: put items into an inner
   `Div itemsHost` and make the platform view an outer `Div` (header, itemsHost, footer). Then move the existing
   layout styling (flex/grid, spacing from `UpdateLayout`) to `itemsHost`. Update every `PlatformView.Children` use accordingly.
3. Header/footer content: if the value is a `View`, render it; if it is a string, render a `Label`; if a template
   exists, create it and set `BindingContext` to the header/footer object.
4. Tests: header string appears before the first item, footer after the last, and a template header gets its BindingContext.

## Step 3 – Grouping

1. When `VirtualView.IsGrouped` is true, `ItemsSource` is a list of lists (each implements `IEnumerable`, usually
   a class deriving from `List<T>` with a `Name`/`Key` property).
2. Render per group: a group header (`GroupHeaderTemplate`, default: `Label` with `group.ToString()`), then the group's
   items, then `GroupFooterTemplate` if set. Keep a flat `containers` list for selection, and keep one small
   `Div` per group (`role="group"`).
3. Keep it simple: on any change in grouped mode call `Rebuild()` (also observe each group collection that is an
   `INotifyCollectionChanged`). Incremental updates stay flat-mode only.
4. Tests: two groups with two items each produce 2 group headers + 4 item containers; selection of an item in a group
   sets `SelectedItem`.

## Step 4 – EmptyViewTemplate and EmptyView polish

`EmptyViewTemplate` exists on `ItemsView`. In `AddEmptyView`, when `EmptyViewTemplate` is set, create it and set
`BindingContext` to the `EmptyView` object. Test: empty list + template shows template content.

## Step 5 – `RefreshView` handler

Browsers have no pull-to-refresh, so render an explicit control.

1. Create `Ooui.Maui\Handlers\RefreshViewHandler.cs`. Virtual view: `Microsoft.Maui.Controls.RefreshView`
   (an `IRefreshView`, it is a `ContentView`; derive from `ContentViewHandler` pattern, see `ContentViewHandler.cs`).
2. Platform view: `Div` with a thin bar (`Button` "Refresh" + `Span` status text) on top and the content below.
3. Button click: set `IsRefreshing = true` and run `Command` (the command should set `IsRefreshing = false` when done).
   While `IsRefreshing` is true: button disabled, status text "Refreshing…". Handle `IsRefreshing` changes via
   `PropertyChanged` (like `RadioButtonHandler` does) because the mapper may not be invoked.
4. Register `handlers.AddHandler<RefreshView, RefreshViewHandler>();` in `OouiMauiExtensions.cs`.
5. Test: click button → command executed once and `IsRefreshing` true; set `IsRefreshing = false` → button enabled again.

## Step 6 – Scroll events and `ScrollTo` (small)

1. `ItemsView.Scrolled` / `RemainingItemsThreshold` / `RemainingItemsThresholdReached`: the container has `overflow:auto`.
   Add a `scroll` event listener on `itemsHost` (Ooui supports the `scroll` event, see `scrollEvents` in `Client.js`,
   the value has `scrollTop`, `scrollHeight`, `clientHeight`). Compute remaining distance; when below
   `RemainingItemsThreshold` × average item height, call `((IItemsView)VirtualView).RemainingItemsThresholdReached()`
   once until the item count grows.
2. Skip `ScrollTo` unless it is trivial. Mention in the report that it is not implemented.
3. One test using `Receive(Message.Event(id, "scroll", {...}))` on the element.

## Step 7 – `ListView` (simple)

README lists `ListView` as unsupported. Add the smallest useful version.

1. `Ooui.Maui\Handlers\ListViewHandler.cs` for `Microsoft.Maui.Controls.ListView`.
2. Supported: `ItemsSource` (+ `INotifyCollectionChanged`), `ItemTemplate` (DataTemplate with `ViewCell`: render
   `cell.View`; `TextCell`: render title + detail labels), `SelectedItem`, events `ItemTapped`/`ItemSelected`,
   `Header`/`Footer`, `HasUnevenRows` ignored, `SeparatorVisibility`/`SeparatorColor` as a bottom border.
3. Reuse code from `CollectionViewHandler` by extracting a small shared helper class only if it removes duplication;
   otherwise copy the minimal parts. Do not refactor unrelated code.
4. Register in `OouiMauiExtensions.cs`. Tests: 3 items with a `ViewCell` template render 3 rows; clicking a row raises
   `ItemTapped` and sets `SelectedItem`.

## Step 8 – Sample and browser check

1. `PlatformSamples\AspNetCoreMaui\Pages\PeoplePage.xaml` (find it, it already uses a `CollectionView`):
   add a grouped second list ("People by team") with `IsGrouped="True"`, a header and a footer, and a "Add person" button
   that adds one item (shows incremental update). Keep existing `AutomationId`s so `verify_pages.py` stays valid.
2. Playwright script (copy helpers from `verify_pages.py` in the session `files` folder): grouped headers visible,
   adding a person increases the row count by 1 and does NOT re-create other rows (compare an element handle's
   `id` attribute before and after), no JS errors.
3. Run `verify_pages.py` (26 checks) – must still pass. Full build + `dotnet test Ooui.sln` as in the navigation plan.

## Step 9 – Docs

README: update the CollectionView limitation sentence (remove "grouping, headers and footers"), add `RefreshView`
and `ListView` to the control table. Keep "no virtualization" if still true.

## Out of scope

Virtualization (windowing), drag and drop reorder, swipe actions, `CarouselView`, `IndicatorView`.

## Definition of done

New tests green, existing tests green, 0 build warnings, Playwright check passes, README updated, nothing committed
without user approval. Final report to the user in German, short.

## Result

Steps 1–9 are implemented and verified:

- `CollectionViewHandler` renders header, an inner `Div.ooui-items` scroll host and footer; `Add`, `Remove` and
  `Replace` insert, drop or renew only the affected containers, while `Move`, `Reset` and the 0↔1 item transitions
  rebuild the list.
- Header, footer, `HeaderTemplate`, `FooterTemplate`, `EmptyViewTemplate` and grouping (`Div.ooui-group` per group
  with `GroupHeaderTemplate`/`GroupFooterTemplate`, a default `Label` header) are honoured; grouped lists rebuild as a
  whole and observe every group collection.
- The scroll host raises `Scrolled` with the visible rows and calls `RemainingItemsThresholdReached` once per item
  count. `ScrollTo` is not implemented, because the browser owns the scroll position.
- `RefreshViewHandler` draws a "Refresh" button plus a status line instead of a pull gesture, runs `Command` once,
  keeps the button disabled while `IsRefreshing` is true and colours the status text with `RefreshColor`.
- `ListViewHandler` covers `ItemsSource` (rebuilding on change), `ViewCell`/`TextCell` templates, `ItemTapped`,
  `ItemSelected`, `SelectedItem`, header/footer and `SeparatorVisibility`/`SeparatorColor`.
- Tests: `AddingAnItemKeepsTheExistingContainers`, `RemovingAnItemKeepsTheOtherContainers`,
  `ReplacingAnItemOnlyRenewsThatContainer`, `HeaderAndFooterSurroundTheItems`, `HeaderTemplateBindsTheHeaderData`,
  `GroupedSourceRendersOneBlockPerGroup`, `EmptyViewTemplateBindsTheEmptyViewData`,
  `ScrollingNearTheEndRaisesTheThresholdEvent`, `ScrollingReportsTheVisibleRows`, the eight `ListView` tests and the
  five `RefreshView` tests.
- Sample: `PeoplePage.xaml` gained a grouped "People by team" list with header and footer plus an "Add member"
  button that appends to one team.
- Browser (`verify_people_grouping.py`, 13 checks): group headers, footer, adding a member grows one group without
  re-creating the other rows, and no page errors; `verify_pages.py` (26 checks) still passes.
