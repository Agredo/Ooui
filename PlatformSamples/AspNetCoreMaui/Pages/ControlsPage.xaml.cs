using System.Globalization;
using Microsoft.Maui.Controls;

namespace AspNetCoreMaui.Pages;

public partial class ControlsPage : ContentPage
{
    public ControlsPage ()
    {
        InitializeComponent ();
        EchoLabel.Text = "The search text is echoed from C#.";
    }

    void OnSearchTextChanged (object? sender, TextChangedEventArgs e) =>
        EchoLabel.Text = string.IsNullOrEmpty (e.NewTextValue)
            ? "The search text is echoed from C#."
            : $"Server received: “{e.NewTextValue}”";

    void OnPlanChanged (object? sender, CheckedChangedEventArgs e)
    {
        if (e.Value && sender is RadioButton radio)
            PlanLabel.Text = $"Selected plan: {radio.Content}";
    }

    int logoClicks;

    void OnLogoClicked (object? sender, EventArgs e) =>
        LogoLabel.Text = $"Logo clicked {++logoClicks} time{(logoClicks == 1 ? "" : "s")}.";

    void OnStepperValueChanged (object? sender, ValueChangedEventArgs e) =>
        StepperLabel.Text = e.NewValue.ToString ("0", CultureInfo.InvariantCulture) + (e.NewValue == 1 ? " seat" : " seats");
}
