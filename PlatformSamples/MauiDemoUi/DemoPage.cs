using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using System.Globalization;

namespace MauiDemoUi;

/// <summary>
/// The one page that every host renders. It contains no HTML and no host specific code:
/// the Ooui handlers turn this hierarchy into elements and attributes, once inside Kestrel
/// and once inside WebAssembly. Only <see cref="RenderMode"/> tells the visitor where it runs.
/// </summary>
public class DemoPage : ContentPage
{
    static readonly Color Ink = Color.FromArgb ("#eef2f9");
    static readonly Color Muted = Color.FromArgb ("#8492aa");
    static readonly Color Accent = Color.FromArgb ("#8f7dff");
    static readonly Color Strong = Color.FromArgb ("#6854d9");
    static readonly Color Sunken = Color.FromArgb ("#101a2b");
    static readonly Color Edge = Color.FromArgb ("#253350");

    readonly Label counter = new () { AutomationId = "counter", Text = "0", FontSize = 44, TextColor = Accent, HorizontalOptions = LayoutOptions.Center };
    readonly Label greeting = new () { AutomationId = "greeting", Text = "Type your name", TextColor = Muted, HorizontalOptions = LayoutOptions.Center };
    readonly Label switchNote = Small ("Switch is on", "switch-note");
    readonly Label sliderNote = Small ("Slider at 0.35", "slider-note");
    readonly Label checkNote = Small ("CheckBox is off", "check-note");

    public DemoPage (string renderMode = "unknown")
    {
        RenderMode = renderMode;
        Title = "MAUI on Ooui";

        var increment = Button ("Increment", Strong, "increment");
        increment.Clicked += (_, _) => counter.Text = (int.Parse (counter.Text) + 1).ToString ();

        var reset = Button ("Reset", Sunken, "reset");
        reset.TextColor = Ink;
        reset.Clicked += (_, _) => counter.Text = "0";

        var name = new Entry { Placeholder = "Your name", AutomationId = "name" };
        name.TextChanged += (_, e) => greeting.Text = string.IsNullOrWhiteSpace (e.NewTextValue)
            ? "Type your name"
            : $"Hello, {e.NewTextValue}.";

        var toggle = new Switch { IsToggled = true };
        toggle.Toggled += (_, e) => switchNote.Text = e.Value ? "Switch is on" : "Switch is off";

        var slider = new Slider { Value = 0.35 };
        slider.ValueChanged += (_, e) => sliderNote.Text =
            "Slider at " + e.NewValue.ToString ("0.00", CultureInfo.InvariantCulture);

        var check = new CheckBox ();
        check.CheckedChanged += (_, e) => checkNote.Text = e.Value ? "CheckBox is on" : "CheckBox is off";

        Content = new ScrollView {
            Content = new VerticalStackLayout {
                Padding = new Thickness (8),
                Spacing = 12,
                Children = {
                    new Border {
                        BackgroundColor = Sunken,
                        Stroke = Edge,
                        StrokeThickness = 1,
                        StrokeShape = new RoundRectangle { CornerRadius = 999 },
                        Padding = new Thickness (14, 7),
                        HorizontalOptions = LayoutOptions.Start,
                        Content = new Label {
                            AutomationId = "render-mode",
                            Text = ModeLabel (renderMode),
                            TextColor = Accent,
                            FontSize = 12,
                            CharacterSpacing = 1.6,
                        },
                    },
                    new Label {
                        Text = "Microsoft.Maui.Controls, rendered by Ooui",
                        TextColor = Ink,
                        FontSize = 26,
                        FontAttributes = FontAttributes.Bold,
                        AutomationId = "title",
                    },
                    new Label {
                        Text = ModeNote (renderMode),
                        TextColor = Muted,
                        FontSize = 15,
                        LineHeight = 1.4,
                    },
                    counter,
                    new HorizontalStackLayout {
                        Spacing = 12,
                        HorizontalOptions = LayoutOptions.Center,
                        Children = { increment, reset },
                    },
                    Heading ("Two-way binding"),
                    name,
                    greeting,
                    Heading ("More handlers"),
                    new Grid {
                        ColumnDefinitions = new ColumnDefinitionCollection (
                            new ColumnDefinition (GridLength.Star),
                            new ColumnDefinition (GridLength.Star)),
                        ColumnSpacing = 16,
                        Children = {
                            new ProgressBar { Progress = 0.6, VerticalOptions = LayoutOptions.Center },
                            new ActivityIndicator { IsRunning = true, Color = Accent, VerticalOptions = LayoutOptions.Center },
                        },
                    },
                    new HorizontalStackLayout {
                        Spacing = 12,
                        Children = { toggle, switchNote },
                    },
                    slider,
                    sliderNote,
                    new HorizontalStackLayout {
                        Spacing = 12,
                        Children = { check, checkNote },
                    },
                    new BoxView { Color = Strong, HeightRequest = 4 },
                },
            },
        };
    }

    /// <summary>"server" when Kestrel rendered this page, "browser" when WebAssembly did.</summary>
    public string RenderMode { get; }

    static string ModeLabel (string renderMode) => renderMode switch {
        "server" => "RENDERED BY KESTREL",
        "browser" => "RENDERED BY WEBASSEMBLY",
        _ => "RENDERED BY OOUI",
    };

    static string ModeNote (string renderMode) => renderMode switch {
        "server" => "This page was built in C# on the server. The HTML above came over HTTP, and every click "
                    + "travels back over this page's WebSocket. Open /wasm/ to see the same class render itself "
                    + "in your browser.",
        "browser" => "This page was built in C# inside this tab. There is no server: MAUI created the view "
                     + "hierarchy, Ooui turned it into HTML, and your input went back through the handler "
                     + "pipeline. The server-rendered version is at /.",
        _ => "This page is written with MAUI controls and translated into HTML by Ooui.",
    };

    static Label Heading (string text) => new () {
        Text = text,
        TextColor = Ink,
        FontSize = 18,
        FontAttributes = FontAttributes.Bold,
    };

    static Label Small (string text, string automationId) => new () {
        Text = text,
        AutomationId = automationId,
        TextColor = Muted,
        FontSize = 13,
        VerticalOptions = LayoutOptions.Center,
    };

    static Button Button (string text, Color background, string automationId) => new () {
        Text = text,
        AutomationId = automationId,
        BackgroundColor = background,
        TextColor = Colors.White,
        CornerRadius = 10,
        HeightRequest = 42,
        Padding = new Thickness (18, 0),
    };
}
