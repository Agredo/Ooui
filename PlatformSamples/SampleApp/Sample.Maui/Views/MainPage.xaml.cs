using Microsoft.Maui.Controls;
using Sample.ViewModels;

namespace Sample.Maui.Views;

/// <summary>The home page: the template's counter, filled by <see cref="MainViewModel"/>.</summary>
public partial class MainPage : ContentPage
{
    public MainPage (MainViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent ();
    }
}
