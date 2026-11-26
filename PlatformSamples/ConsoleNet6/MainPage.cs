using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ConsoleNet6;

class MainPage : ContentPage
{
    static readonly Color Ink = Color.FromArgb("#172238");
    static readonly Color Muted = Color.FromArgb("#6f7990");
    static readonly Color Accent = Color.FromArgb("#6854d9");
    static readonly Color Line = Color.FromArgb("#e0e5f0");

    readonly LaunchModel model = new();
    readonly Label status = new() { Text = "No changes saved yet.", TextColor = Ink, FontSize = 16, AutomationId = "save-status" };
    readonly Label eventCount = Small("0 events this session");
    readonly VerticalStackLayout activity = new() { Spacing = 0 };
    readonly ActivityIndicator busy = new() { IsRunning = false, Color = Accent, HeightRequest = 30 };
    readonly Button save = Primary("Save launch plan");
    readonly Entry project;
    readonly Picker role;
    readonly DatePicker launchDate = new() { Date = DateTime.Today.AddDays(14), AutomationId = "launch-date" };
    readonly TimePicker launchTime = new() { Time = new TimeSpan(10, 0, 0), AutomationId = "launch-time" };
    readonly CheckBox preview = new() { IsChecked = true, Color = Accent, AutomationId = "preview-ready" };
    readonly Slider progress = new() { Minimum = 0, Maximum = 100, Value = 68, ThumbColor = Accent, MinimumTrackColor = Accent, AutomationId = "progress-slider" };
    readonly ProgressBar progressBar = new() { Progress = .68, ProgressColor = Accent, HeightRequest = 10 };
    readonly Switch notifications = new() { OnColor = Accent, AutomationId = "notifications" };
    readonly Editor notes = new() { Placeholder = "What makes this project special?", HeightRequest = 100, MaxLength = 400, AutomationId = "project-notes" };
    readonly SearchBar search = new() { Placeholder = "Filter the activity feed…", AutomationId = "activity-search" };
    readonly ScrollView feed;
    readonly List<string> events = new()
    {
        "✓  Design tokens synchronized · just now",
        "✓  Grid and FlexLayout rendered in the browser",
        "✓  Form bindings connected to your server model",
        "✓  Session-specific state created",
        "✓  Progress and feedback controls ready",
        "✓  WebSocket event pipeline verified",
        "→  Your next idea starts here",
    };
    int interactions;
    bool saving;

    public MainPage()
    {
        Title = "Ooui × MAUI 11 · Build for the web";
        BackgroundColor = Color.FromArgb("#f5f7fc");

        project = new Entry { Placeholder = "e.g. Northstar", ReturnType = ReturnType.Done, AutomationId = "project-name" };
        project.SetBinding(Entry.TextProperty, new Binding(nameof(LaunchModel.ProjectName), BindingMode.OneWayToSource, source: model));
        project.Completed += (_, _) => Save();

        role = new Picker { Title = "Choose a project type", AutomationId = "project-role" };
        role.ItemsSource = new[] { "Marketing site", "Customer portal", "Internal dashboard" };
        role.SelectedIndex = 0;
        role.SetBinding(Picker.SelectedItemProperty, new Binding(nameof(LaunchModel.Role), BindingMode.OneWayToSource, source: model));

        progress.SetBinding(Slider.ValueProperty, new Binding(nameof(LaunchModel.Progress), BindingMode.TwoWay, source: model));
        var progressLabel = new Label { FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ink };
        progressLabel.SetBinding(Label.TextProperty, new Binding(nameof(LaunchModel.ProgressText), source: model));
        progressBar.SetBinding(ProgressBar.ProgressProperty, new Binding(nameof(LaunchModel.Progress), source: model, converter: new PercentageConverter()));
        notifications.SetBinding(Switch.IsToggledProperty, new Binding(nameof(LaunchModel.Notifications), BindingMode.OneWayToSource, source: model));
        notifications.IsToggled = true;
        notes.SetBinding(Editor.TextProperty, new Binding(nameof(LaunchModel.Notes), BindingMode.OneWayToSource, source: model));

        save.Clicked += (_, _) => Save();
        var reset = Secondary("Reset");
        reset.Clicked += (_, _) => Reset();
        search.TextChanged += (_, _) => Filter();
        search.SearchButtonPressed += (_, _) => Filter();

        feed = new ScrollView { Orientation = ScrollOrientation.Vertical, HeightRequest = 265, AutomationId = "activity", Content = activity };
        AddActivity("Your session is ready. Try editing the project name.");
        Filter();

        Content = new VerticalStackLayout
        {
            Spacing = 24,
            Children =
            {
                Hero(),
                new Grid
                {
                    AutomationId = "feature-grid", ColumnSpacing = 24, RowSpacing = 24,
                    ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
                    RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto) },
                    Children =
                    {
                        CardContent("Launch a new idea", "An interactive form, powered by your .NET model.", new VerticalStackLayout
                        {
                            Spacing = 16,
                            Children = { Caption("PROJECT NAME"), project, Caption("PROJECT TYPE"), role, Caption("LAUNCH DATE"), launchDate, Caption("LAUNCH TIME"), launchTime,
                                new HorizontalStackLayout { Spacing = 10, Children = { preview, Body("Ready for a private preview") } } },
                        }),
                        CardContent("Make it feel alive", "Two-way input, live progress and useful feedback.", new VerticalStackLayout
                        {
                            Spacing = 14,
                            Children = { progressLabel, progressBar, progress, new Label { Text = "Drag to update readiness", FontSize = 13, TextColor = Muted }, Caption("NOTES"), notes,
                                new HorizontalStackLayout { Spacing = 10, Children = { notifications, Body("Notify the launch team") } } },
                        }),
                    },
                },
                new Grid
                {
                    AutomationId = "overview-grid", ColumnSpacing = 20, RowSpacing = 20,
                    ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star), new(GridLength.Star) },
                    RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto) },
                    Children =
                    {
                        CardContent("Server-driven by design", "Your state stays in .NET. Ooui takes care of the browser.", new VerticalStackLayout
                        {
                            Spacing = 10, Children = { new Label { Text = "●  Connected", TextColor = Color.FromArgb("#31846a"), FontAttributes = FontAttributes.Bold }, Small("Each WebSocket has its own page, bindings and activity feed."), eventCount },
                        }),
                        CardContent("A real MAUI workflow", "Use familiar controls, bindings and layout—not a JavaScript UI clone.", new VerticalStackLayout
                        {
                            Spacing = 12, Children = { new Label { Text = "C# → HTML + CSS", FontSize = 23, FontAttributes = FontAttributes.Bold, TextColor = Accent }, Small("MAUI 11 handlers translate your views into native browser elements.") },
                        }),
                        CardContent("Ready to explore", "Small controls, a coherent foundation, and room to grow.", new VerticalStackLayout
                        {
                            Spacing = 12, Children = { new BoxView { Color = Accent, HeightRequest = 4, CornerRadius = 2 }, new Label { Text = "18 controls. One .NET app.", FontSize = 18, FontAttributes = FontAttributes.Bold, TextColor = Ink }, Small("This sample is a demonstration, not a complete MAUI web platform.") },
                        }),
                    },
                },
                new Grid
                {
                    AutomationId = "workload-grid", ColumnSpacing = 24, RowSpacing = 24,
                    ColumnDefinitions = new ColumnDefinitionCollection { new(GridLength.Star), new(GridLength.Star) },
                    RowDefinitions = new RowDefinitionCollection { new(GridLength.Auto) },
                    Children =
                    {
                        CardContent("Save your launch plan", "Validation and application logic run in C#.", new VerticalStackLayout { Spacing = 14, Children = { new HorizontalStackLayout { Spacing = 12, Children = { save, reset, busy } }, status } }),
                        CardContent("Session activity", "Scroll, filter and interact. Nothing is shared with another visitor.", new VerticalStackLayout { Spacing = 12, Children = { search, feed } }),
                    },
                },
                new Label { Text = "Built with Ooui · .NET MAUI 11 · An experimental browser backend", FontSize = 13, TextColor = Muted, HorizontalTextAlignment = TextAlignment.Center, Padding = new Thickness(0, 12) },
            },
        };
    }

    static Border CardContent(string heading, string description, View content) => new()
    {
        BackgroundColor = Colors.White, Stroke = new SolidColorBrush(Line), StrokeThickness = 1,
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 18 }, Padding = new Thickness(24),
        Content = new VerticalStackLayout { Spacing = 14, Children = { new Label { Text = heading, FontSize = 24, FontAttributes = FontAttributes.Bold, TextColor = Ink }, Small(description), content } },
    };

    static Border Hero() => new()
    {
        AutomationId = "hero", StrokeThickness = 0, Padding = new Thickness(32),
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 24 },
        Content = new VerticalStackLayout
        {
            Spacing = 16,
            Children =
            {
                new Label { Text = "OOUI  /  MAUI 11", TextColor = Color.FromArgb("#bdb5ff"), FontSize = 13, CharacterSpacing = 2 },
                new Label { AutomationId = "hero-title", Text = "Your next web app. Written in C#.", TextColor = Colors.White, FontSize = 48, FontAttributes = FontAttributes.Bold },
                new Label { Text = "Familiar .NET controls. Native browser elements. One interactive experience, session by session.", TextColor = Color.FromArgb("#d3d0ef"), FontSize = 19, LineHeight = 1.35 },
            },
        },
    };

    static Label Caption(string text) => new() { Text = text, TextColor = Muted, FontSize = 12, FontAttributes = FontAttributes.Bold, CharacterSpacing = 1 };
    static Label Body(string text) => new() { Text = text, TextColor = Ink, FontSize = 16 };
    static Label Small(string text) => new() { Text = text, TextColor = Muted, FontSize = 14, LineHeight = 1.3 };
    static Button Primary(string text) => new() { Text = text, TextColor = Colors.White, BackgroundColor = Accent, CornerRadius = 10, FontAttributes = FontAttributes.Bold, AutomationId = "save" };
    static Button Secondary(string text) => new() { Text = text, TextColor = Ink, BackgroundColor = Color.FromArgb("#eef0f7"), CornerRadius = 10, AutomationId = "reset" };

    async void Save()
    {
        if (saving) return;
        if (string.IsNullOrWhiteSpace(model.ProjectName))
        {
            status.Text = "Please enter a project name before saving.";
            status.TextColor = Color.FromArgb("#bb3d57");
            project.Focus();
            return;
        }
        saving = true;
        save.IsEnabled = false;
        busy.IsRunning = true;
        status.TextColor = Muted;
        status.Text = $"Saving {model.Summary}…";
        await Task.Delay(500);
        status.TextColor = Color.FromArgb("#31846a");
        status.Text = $"Saved locally · {model.Summary} · {DateTime.Now:T}";
        busy.IsRunning = false;
        save.IsEnabled = true;
        saving = false;
        events.Add($"✓  {model.ProjectName} saved · {DateTime.Now:T}");
        AddActivity("Form saved");
        Filter();
    }

    void Reset()
    {
        project.Text = "Northstar";
        role.SelectedIndex = 0;
        launchDate.Date = DateTime.Today.AddDays(14);
        launchTime.Time = new TimeSpan(10, 0, 0);
        preview.IsChecked = true;
        progress.Value = 68;
        notifications.IsToggled = true;
        notes.Text = string.Empty;
        status.Text = "Sample values restored. This only changed your session.";
        status.TextColor = Muted;
        events.Add("↺  Form reset");
        AddActivity("Form reset");
        Filter();
    }

    void Filter()
    {
        activity.Children.Clear();
        var query = search.Text?.Trim() ?? string.Empty;
        var matches = events.Where(text => string.IsNullOrEmpty(query) || text.Contains(query, StringComparison.OrdinalIgnoreCase)).ToArray();
        foreach (var text in matches)
            activity.Children.Add(new Label { Text = text, TextColor = Ink, FontSize = 14, Padding = new Thickness(0, 10) });
        if (matches.Length == 0) activity.Children.Add(Small("No matching activity. Try another search."));
    }

    void AddActivity(string text) => eventCount.Text = $"{++interactions} event{(interactions == 1 ? "" : "s")} this session";

    sealed class PercentageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) => value is double number ? number / 100 : 0;
        public object ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture) => throw new NotSupportedException();
    }
}

