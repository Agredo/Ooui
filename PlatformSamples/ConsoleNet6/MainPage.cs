using Microsoft.Maui;
using Microsoft.Maui.Controls;

class MainPage : ContentPage
{
    readonly Label counterLabel = new() { Text = "Noch nicht geklickt", FontSize = 24 };
    int counter;

    public MainPage()
    {
        Title = "MAUI 11 im Browser";
        Content = new VerticalStackLayout
        {
            Spacing = 16,
            Padding = new Thickness(24),
            Children =
            {
                new Label { Text = "Hallo von .NET MAUI 11 und Ooui!", FontSize = 32 },
                counterLabel,
                new Button
                {
                    Text = "Zähler erhöhen",
                    Command = new Command(() => counterLabel.Text = $"Klicks: {++counter}"),
                },
            },
        };
    }
}

