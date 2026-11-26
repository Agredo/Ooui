using Microsoft.Maui.Controls;
using Sample.ViewModels;

namespace Sample.Maui.Views;

/// <summary>The second page, filled by <see cref="DetailsViewModel"/>.</summary>
public partial class DetailsPage : ContentPage
{
    public DetailsPage (DetailsViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent ();
    }
}
