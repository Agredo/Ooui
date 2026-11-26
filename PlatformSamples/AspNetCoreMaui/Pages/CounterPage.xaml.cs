using System.Globalization;
using Microsoft.Maui.Controls;

namespace AspNetCoreMaui.Pages;

public partial class CounterPage : ContentPage
{
    readonly SiteState state;

    public CounterPage (SiteState state)
    {
        this.state = state;
        InitializeComponent ();
        ValueLabel.Text = state.Count.ToString (CultureInfo.InvariantCulture);
        StatusLabel.Text = state.CounterNote;
    }

    void OnDecrementClicked (object? sender, EventArgs e)
    {
        state.Count--;
        Update ($"Decremented to {state.Count}.");
    }

    void OnIncrementClicked (object? sender, EventArgs e)
    {
        state.Count++;
        Update ($"Incremented to {state.Count}.");
    }

    void OnResetClicked (object? sender, EventArgs e)
    {
        state.Count = 0;
        Update ("Counter reset.");
    }

    // The note is part of the session too, so the next request starts with the same text.
    void Update (string message)
    {
        state.CounterNote = message;
        ValueLabel.Text = state.Count.ToString (CultureInfo.InvariantCulture);
        StatusLabel.Text = message;
    }
}
