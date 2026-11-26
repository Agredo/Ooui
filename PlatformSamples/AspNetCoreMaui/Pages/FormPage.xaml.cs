using System.Globalization;
using Microsoft.Maui.Controls;

namespace AspNetCoreMaui.Pages;

public partial class FormPage : ContentPage
{
    readonly SiteState state;

    public FormPage (SiteState state)
    {
        this.state = state;
        InitializeComponent ();

        // Values that are easier to express in C# than in XAML.
        NameEntry.Text = state.Name;
        EmailEntry.Text = state.Email;
        NotesEditor.Text = state.Notes;
        DateControl.Date = DateTime.Today.AddDays (14);
        TimeControl.Time = new TimeSpan (10, 0, 0);
        StatusLabel.Text = state.FormNote;
    }

    void OnNameChanged (object? sender, TextChangedEventArgs e) => state.Name = e.NewTextValue ?? "";

    void OnNameCompleted (object? sender, EventArgs e)
    {
        state.FormNote = "Enter on the project name jumped to the contact field.";
        StatusLabel.TextColor = ThemeColor ("Muted");
        StatusLabel.Text = state.FormNote;
        EmailEntry.Focus ();
    }

    void OnEmailChanged (object? sender, TextChangedEventArgs e) => state.Email = e.NewTextValue ?? "";

    void OnNotesChanged (object? sender, TextChangedEventArgs e) => state.Notes = e.NewTextValue ?? "";

    void OnBudgetChanged (object? sender, ValueChangedEventArgs e)
    {
        BudgetLabel.Text = e.NewValue.ToString ("0", CultureInfo.InvariantCulture) + " %";
        BudgetBar.Progress = e.NewValue / 100;
    }

    // Colors live in Theme.xaml; fall back to a readable default if a key is missing.
    Color ThemeColor (string key) => Resources.TryGetValue (key, out var value) && value is Color color
        ? color
        : Colors.Gray;

    void OnSubmitClicked (object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace (NameEntry.Text)) {
            StatusLabel.TextColor = ThemeColor ("Danger");
            StatusLabel.Text = "Please enter a project name.";
            NameEntry.Focus ();
            return;
        }
        if (TermsCheckBox.IsChecked is not true) {
            StatusLabel.TextColor = ThemeColor ("Danger");
            StatusLabel.Text = "Please accept the terms first.";
            return;
        }

        StatusLabel.TextColor = ThemeColor ("Success");
        var at = TimeControl.Time ?? new TimeSpan (10, 0, 0);
        var when = $"{DateControl.Date:yyyy-MM-dd} {at.Hours:00}:{at.Minutes:00}";
        state.FormNote = $"Saved {NameEntry.Text.Trim ()} · {RolePicker.SelectedItem} · {when} · budget {BudgetSlider.Value:0} %";
        StatusLabel.Text = state.FormNote;
    }

    void OnResetClicked (object? sender, EventArgs e)
    {
        NameEntry.Text = string.Empty;
        EmailEntry.Text = string.Empty;
        NotesEditor.Text = string.Empty;
        state.Name = "";
        state.Email = "";
        state.Notes = "";
        RolePicker.SelectedIndex = 0;
        DateControl.Date = DateTime.Today.AddDays (14);
        TimeControl.Time = new TimeSpan (10, 0, 0);
        BudgetSlider.Value = 68;
        TermsCheckBox.IsChecked = false;
        state.FormNote = "Sample values restored. Only this session changed.";
        StatusLabel.TextColor = ThemeColor ("Muted");
        StatusLabel.Text = state.FormNote;
    }
}
