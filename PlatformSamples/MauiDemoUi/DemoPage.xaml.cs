using System.Globalization;
using Microsoft.Maui.Controls;

namespace MauiDemoUi;

/// <summary>
/// The one page that every host renders. It contains no HTML and no host specific code: the Ooui
/// handlers turn the XAML into elements and attributes, once inside Kestrel and once inside
/// WebAssembly. Only <see cref="RenderMode"/> tells the visitor where it runs.
/// </summary>
public partial class DemoPage : ContentPage
{
    public DemoPage (string renderMode = "unknown")
    {
        RenderMode = renderMode;
        InitializeComponent ();

        RenderModeLabel.Text = ModeLabel (renderMode);
        ModeNoteLabel.Text = ModeNote (renderMode);

        // Assigned after InitializeComponent so that the handlers below find their labels.
        ToggleSwitch.IsToggled = true;
        SliderControl.Value = 0.35;
    }

    /// <summary>"server" when Kestrel rendered this page, "browser" when WebAssembly did.</summary>
    public string RenderMode { get; }

    void OnIncrementClicked (object? sender, EventArgs e) =>
        CounterLabel.Text = (int.Parse (CounterLabel.Text, CultureInfo.InvariantCulture) + 1)
            .ToString (CultureInfo.InvariantCulture);

    void OnResetClicked (object? sender, EventArgs e) => CounterLabel.Text = "0";

    void OnNameChanged (object? sender, TextChangedEventArgs e) =>
        GreetingLabel.Text = string.IsNullOrWhiteSpace (e.NewTextValue)
            ? "Type your name"
            : $"Hello, {e.NewTextValue}.";

    void OnToggled (object? sender, ToggledEventArgs e) =>
        SwitchNoteLabel.Text = e.Value ? "Switch is on" : "Switch is off";

    void OnSliderChanged (object? sender, ValueChangedEventArgs e) =>
        SliderNoteLabel.Text = "Slider at " + e.NewValue.ToString ("0.00", CultureInfo.InvariantCulture);

    void OnCheckedChanged (object? sender, CheckedChangedEventArgs e) =>
        CheckNoteLabel.Text = e.Value ? "CheckBox is on" : "CheckBox is off";

    static string ModeLabel (string renderMode) => renderMode switch {
        "server" => "RENDERED BY KESTREL",
        "browser" => "RENDERED BY WEBASSEMBLY",
        _ => "RENDERED BY OOUI",
    };

    static string ModeNote (string renderMode) => renderMode switch {
        "server" => "This XAML page was inflated in C# on the server. The HTML above came over HTTP, and "
                    + "every click travels back over this page's WebSocket. Open /wasm/ to see the same "
                    + "XAML inflate itself in your browser.",
        "browser" => "This XAML page was inflated in C# inside this tab. There is no server: MAUI created "
                     + "the view hierarchy, Ooui turned it into HTML, and your input went back through the "
                     + "handler pipeline. The server-rendered version is at /.",
        _ => "This page is written in XAML with MAUI controls and translated into HTML by Ooui.",
    };
}
