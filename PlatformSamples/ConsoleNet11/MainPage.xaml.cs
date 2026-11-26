using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace ConsoleNet11;

public partial class MainPage : ContentPage
{
    readonly LaunchModel model = new ();
    readonly List<string> events = new ()
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

    public MainPage ()
    {
        InitializeComponent ();
        BindingContext = model;

        RolePicker.SelectedIndex = 0;
        NotificationsSwitch.IsToggled = true;

        AddActivity ("Your session is ready. Try editing the project name.");
        Filter ();
    }

    Color ThemeColor (string key) => Resources.TryGetValue (key, out var value) && value is Color color
        ? color
        : Colors.Gray;

    void OnSaveClicked (object? sender, EventArgs e) => Save ();

    void OnResetClicked (object? sender, EventArgs e) => Reset ();

    void OnProjectCompleted (object? sender, EventArgs e) => Save ();

    void OnSearchTextChanged (object? sender, TextChangedEventArgs e) => Filter ();

    void OnSearchButtonPressed (object? sender, EventArgs e) => Filter ();

    async void Save ()
    {
        if (saving) return;
        if (string.IsNullOrWhiteSpace (model.ProjectName)) {
            StatusLabel.Text = "Please enter a project name before saving.";
            StatusLabel.TextColor = ThemeColor ("Danger");
            ProjectEntry.Focus ();
            return;
        }
        saving = true;
        SaveButton.IsEnabled = false;
        Busy.IsRunning = true;
        StatusLabel.TextColor = ThemeColor ("Muted");
        StatusLabel.Text = $"Saving {model.Summary}…";
        await Task.Delay (500);
        StatusLabel.TextColor = ThemeColor ("Success");
        StatusLabel.Text = $"Saved locally · {model.Summary} · {DateTime.Now:T}";
        Busy.IsRunning = false;
        SaveButton.IsEnabled = true;
        saving = false;
        events.Add ($"✓  {model.ProjectName} saved · {DateTime.Now:T}");
        AddActivity ("Form saved");
        Filter ();
    }

    void Reset ()
    {
        ProjectEntry.Text = "Northstar";
        RolePicker.SelectedIndex = 0;
        LaunchDatePicker.Date = DateTime.Today.AddDays (14);
        LaunchTimePicker.Time = new TimeSpan (10, 0, 0);
        PreviewCheckBox.IsChecked = true;
        ProgressSlider.Value = 68;
        NotificationsSwitch.IsToggled = true;
        NotesEditor.Text = string.Empty;
        StatusLabel.Text = "Sample values restored. This only changed your session.";
        StatusLabel.TextColor = ThemeColor ("Muted");
        events.Add ("↺  Form reset");
        AddActivity ("Form reset");
        Filter ();
    }

    void Filter ()
    {
        ActivityFeed.Children.Clear ();
        var query = Search.Text?.Trim () ?? string.Empty;
        var matches = events
            .Where (text => string.IsNullOrEmpty (query) || text.Contains (query, StringComparison.OrdinalIgnoreCase))
            .ToArray ();
        foreach (var text in matches)
            ActivityFeed.Children.Add (new Label {
                Text = text,
                TextColor = ThemeColor ("Ink"),
                FontSize = 14,
                Padding = new Thickness (0, 10),
            });
        if (matches.Length == 0)
            ActivityFeed.Children.Add (new Label {
                Text = "No matching activity. Try another search.",
                TextColor = ThemeColor ("Muted"),
                FontSize = 14,
                LineHeight = 1.3,
            });
    }

    void AddActivity (string text) =>
        EventCountLabel.Text = $"{++interactions} event{(interactions == 1 ? "" : "s")} this session";
}
