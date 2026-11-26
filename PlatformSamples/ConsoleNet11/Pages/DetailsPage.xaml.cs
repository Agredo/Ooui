using Microsoft.Maui.Controls;

namespace ConsoleNet11.Pages;

public partial class DetailsPage : ContentPage
{
    public DetailsPage (string projectName)
    {
        InitializeComponent ();
        ProjectLabel.Text = string.IsNullOrWhiteSpace (projectName)
            ? "No project name yet"
            : projectName;
    }

    async void OnBackClicked (object? sender, EventArgs e) => await Navigation.PopAsync ();
}
