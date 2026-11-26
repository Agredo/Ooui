# Plan 3: Forms (formats, keyboards, validation)

Read `docs\navigation-plan.md` section 0 (ground rules) and section 1 first. They apply here too.

Goal: forms behave like real web forms. Dates show in the format the MAUI developer chose, mobile keyboards are
right, and validation (including the Community Toolkit behaviors) works and is shown.

## Step 1 – Audit (no production code)

Read the mappers of `EntryHandler.cs`, `EditorHandler.cs`, `PickerHandler.cs`, `SliderHandler.cs`, `SwitchHandler.cs`,
`DatePickerHandler.cs`, `TimePickerHandler.cs` in `Ooui.Maui\Handlers\`. Write a table at the end of this file
("Findings"): property | MAUI type | mapped? Check at least these:

- Entry: `IsPassword`, `MaxLength`, `Keyboard`, `ReturnType`, `ClearButtonVisibility`, `IsReadOnly`,
  `IsSpellCheckEnabled`, `IsTextPredictionEnabled`, `CursorPosition`, `SelectionLength`, `HorizontalTextAlignment`,
  `Completed` event (Enter key).
- Editor: `AutoSize`, `MaxLength`, `IsReadOnly`, `Keyboard`.
- Picker: `Title`, `TitleColor`, `ItemDisplayBinding`, `SelectedItem`.
- DatePicker/TimePicker: `Format`, `TextColor`.
- Slider: `MinimumTrackColor`, `MaximumTrackColor`, `ThumbColor`. Switch: `OnColor`, `ThumbColor`.

Implement only the missing ones from Steps 2–4. Skip what is already mapped.

## Step 2 – Keyboard, MaxLength, password, Enter

Map to HTML attributes through `PlatformView.SetAttribute(name, value)` / `RemoveAttribute(name)` (see how
`DatePickerHandler.MapRange` does it).

| MAUI | HTML |
|---|---|
| `Keyboard.Email` | `type="email"` (Entry uses `Ooui.Input`, set `Type = InputType.Email` if the enum has it, otherwise `SetAttribute("type","email")`) |
| `Keyboard.Numeric` | `inputmode="decimal"` |
| `Keyboard.Telephone` | `type="tel"` |
| `Keyboard.Url` | `type="url"` |
| `Keyboard.Text`/default | remove the attributes again |
| `MaxLength` | `maxlength` (remove when `>= int.MaxValue`) |
| `IsReadOnly` | `readonly` |
| `IsSpellCheckEnabled` | `spellcheck="true/false"` |
| `ReturnType` | `enterkeyhint` (`Done`→`done`, `Go`→`go`, `Next`→`next`, `Search`→`search`, `Send`→`send`) |

`Entry.Completed` (Enter key): `Client.js` sends `keyup` events with the key (`keyboardEvents` only has keydown and
keypress). Use the existing `Ooui.Input` key events (check `Ooui\Element.cs`/`Input.cs` for a `KeyUp`/`KeyDown`
event and `Message` value). On Enter call `((IEntry)VirtualView).Completed()`. Check that the browser really sends
the key: write a Playwright check, do not trust only the unit test (a previous RadioButton bug hid exactly there).

Tests: one test per group above (attribute present with the expected value, removed when reset).

## Step 3 – `Format` for DatePicker and TimePicker

A native `<input type="date">` always shows the browser locale, so `Format="dd MMMM yyyy"` cannot be applied
directly. Solution: overlay.

1. When `Format` is the default (null/empty or `"d"` for DatePicker, `"t"` for TimePicker) keep the current native input.
2. Otherwise render a wrapper `Div` (position relative, display inline-block) with:
   - a visible `Span` showing `Date.ToString(Format, CultureInfo.CurrentCulture)` styled like a text field
     (border, padding, same font; reuse the styles of the Entry handler if there are helper methods in `OouiStyle`),
   - the native `Input` absolutely positioned over it with `opacity: 0; inset: 0; width/height: 100%; cursor: pointer`.
   Clicking opens the native picker, the change event updates the span.
3. This requires the platform view type to change from `Input` to `Div`. Do it minimally: keep
   `OouiViewHandler<IDatePicker, Input>` and instead wrap in the **parent chain** is not possible, so change the
   generic argument to `Div` and keep the `Input` in a field. Update `MapDate`, `MapRange`, `MapFont`, `OnChange`
   accordingly. Existing tests in `FormControlTests.cs` that cast the element to `Input` must be adapted
   (find with `grep -n "DatePicker\|TimePicker" Ooui.Maui.Tests\*.cs`). Keep the DOM of the default case simple:
   always use the wrapper (span hidden when no Format) so there is only one code path.
4. Map `Format` (`nameof(IDatePicker.Format)`, `ITimePicker.Format`) and refresh the span when `Date`/`Time`, `Format` change.
5. Tests: `Format="dd.MM.yyyy"` shows `05.03.2027` for 2027-03-05; the native input still has `2027-03-05`;
   a change event from the input updates `DatePicker.Date` and the span text.

## Step 4 – Validation display with Community Toolkit behaviors

The behaviors (`EmailValidationBehavior`, `TextValidationBehavior`, `NumericValidationBehavior`,
`RequiredStringValidationBehavior`) live on the virtual view and need no handler support. They set
`IsValid`/`Flags` and drive the VisualStateManager states `Valid` / `Invalid`. Verify this works, add a sample.

1. Find the MVVM sample (`PlatformSamples\ConsoleNet11\MainPage.xaml` or the AspNetCoreMaui `FormPage.xaml`).
   Check the package reference to `CommunityToolkit.Maui` is available in that project; if not, add the NuGet
   `CommunityToolkit.Maui` (latest stable for MAUI 11) and call `UseMauiCommunityToolkit()` ONLY if needed for the
   behaviors; behaviors work without it. If the toolkit does not support net11 yet, STOP this step and report it.
2. Add an email `Entry` with `<toolkit:EmailValidationBehavior x:Name="EmailCheck" Flags="ValidateOnValueChanged" />` and a
   `Label` bound to `EmailCheck.IsValid` (via `Source={x:Reference EmailCheck}`) that shows "Looks good" or an error text,
   and a `Button` whose `IsEnabled` is bound to the validity.
3. `VisualStateManager`: add `Invalid` state setter `TextColor=Red` on the entry style and verify it renders. If the
   `VisualStateManager` does not update the browser (handler mapping for `TextColor` fires?), fix the mapper trigger and add a test.
4. Playwright: type an invalid address → error label visible, button disabled; type a valid one → label changes, button enabled.

## Step 5 – Slider and Switch colors (only if the audit says missing)

`Slider`: `accent-color` CSS covers thumb and track on modern browsers: map `ThumbColor`/`MinimumTrackColor` to it
(use `MinimumTrackColor` first, else `ThumbColor`). `Switch`: `OnColor` → `accent-color`. Tests: style value present.

## Step 6 – Docs and verification

- README: update the DatePicker sentence (remove "do not apply custom Format values") and mention keyboard/validation support.
- Full build (0 warnings), all tests, Playwright checks from Steps 2 and 4, `verify_console.py`, `verify_mvvm.py`,
  `verify_pages.py` still pass.

## Out of scope

Localized date pickers independent of the browser locale, rich text editors, masked input, autocomplete lists.

## Definition of done

New tests green, existing tests green, 0 warnings, Playwright checks pass, README updated, nothing committed without
user approval. Final report to the user in German, short.

## Findings (filled in during Step 1)

| Property | MAUI type | Mapped before | Now |
|---|---|---|---|
| `Entry.IsPassword` | `IEntry` | yes → `type=password` | unchanged |
| `Entry.MaxLength` | `ITextInput` | `maxlength`, but `int.MaxValue` became `maxlength="undefined"` | `>= int.MaxValue` or `< 0` removes the attribute |
| `Entry.Keyboard` | `ITextInput` | `Email`/`Tel`/`Url` → input type; `Numeric` → `type=number` (spinners, invalid text dropped) | `Numeric` → `inputmode="decimal"` on a text input |
| `Entry.ReturnType` | `Entry` | no | `enterkeyhint` (`go`, `next`, `search`, `send`, `done`, else `enter`) |
| `Entry.ClearButtonVisibility` | `Entry` | no | still no (see out of scope) |
| `Entry.IsReadOnly` | `ITextInput` | yes → `readonly` | unchanged |
| `Entry.IsSpellCheckEnabled` | `ITextInput` | no | `spellcheck` |
| `Entry.IsTextPredictionEnabled` | `ITextInput` | no | folded into `spellcheck`/`autocomplete` by the browser, nothing to map |
| `Entry.CursorPosition`, `SelectionLength` | `IEntry` | no (DOM only, no MAUI property write-back) | still no (see out of scope) |
| `Entry.HorizontalTextAlignment` | `ITextAlignment` | yes → `text-align` | unchanged |
| `Entry.Completed` | event | raised on `change` (double fired after Enter) | Enter raises it once, `change` keeps the blur case |
| `Editor.AutoSize` | `Editor` | no | no (textareas do not grow in the browser) |
| `Editor.MaxLength` | `ITextInput` | yes | same `int.MaxValue` rule as `Entry` |
| `Editor.IsReadOnly` | `ITextInput` | yes | unchanged |
| `Editor.Keyboard` | `ITextInput` | no | no (a textarea uses the text keyboard) |
| `Picker.Title`, `TitleColor` | `Picker` | yes | unchanged |
| `Picker.ItemDisplayBinding` | `Picker` | yes | unchanged |
| `Picker.SelectedItem` | `Picker` | yes | unchanged |
| `DatePicker.Format`, `TimePicker.Format` | `IDatePicker`, `ITimePicker` | no — the native input always used the browser locale | formatted `Span` over the (still native) input |
| `DatePicker.TextColor`, `TimePicker.TextColor` | `ITextStyle` | yes | moved to the display `Span` |
| `Slider.MinimumTrackColor`, `ThumbColor` | `ISlider` | `ThumbColor` only | `MinimumTrackColor` first, then ThumbColor → `accent-color` |
| `Slider.MaximumTrackColor` | `ISlider` | no | no (one `accent-color` per input) |
| `Switch.OnColor` | `Microsoft.Maui.Controls.Switch` | no (`ISwitch` has no `OnColor`) | `OnColor` first, then `TrackColor`/`ThumbColor` → `accent-color` |
| `Switch.ThumbColor` | `ISwitch` | yes | unchanged |

## Result

Steps 2–6 are implemented and verified:

- `EntryHandler`/`EditorHandler`: `spellcheck`, `enterkeyhint`, the `maxlength` rules, `inputmode` for numeric
  keyboards, and a `Completed` that fires once for Enter (`keydown`) and again for a plain blur (`change`).
- `DatePickerHandler`/`TimePickerHandler` render `OouiViewHandler<..., Div>` with a hidden `Span` plus the native
  input as an invisible overlay; `OouiStyle.HasCustomFormat`/`SetOverlay` switch between both modes. Time formats
  are applied through `DateTime.Today.Add(time)` because `TimeSpan.ToString("HH.mm")` throws.
- `SwitchHandler`/`SliderHandler` merge their colors into `accent-color`.
- `PlatformSamples/AspNetCoreMaui/Pages/FormPage.xaml` uses a `CommunityToolkit.Maui` 17.0.0
  `EmailValidationBehavior` with `ValidateOnValueChanged`, an `IsValid`-bound submit button and a `Valid`/`Invalid`
  visual state group (the toolkit starts in the `Invalid` state, so the hint reads as a hint, and the placeholder
  keeps its own color).
- Tests: `EntryMapsKeyboardMaxLengthReturnTypeAndSpellCheck`, `EntryRaisesCompletedOnTheEnterKey`,
  `EntryCompletesOnceForEnterAndAgainAfterTyping`, `DatePickerKeepsTheNativeValueAndShowsCustomFormatsInASpan`,
  `TimePickerKeepsTheNativeValueAndShowsCustomFormatsInASpan`, `SliderAndSwitchMergeTheirColorsIntoAccentColor`.
- Browser (`verify_forms.py`, 25 checks): `enterkeyhint`, `type=email`, `spellcheck=false`, `maxlength`,
  Enter → `Completed` with focus moving on, invalid/valid email → hints, submit state and text color, both picker
  spans against the native value, `accent-color` on slider and switch.

