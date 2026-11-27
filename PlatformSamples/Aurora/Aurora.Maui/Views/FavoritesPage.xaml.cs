using Aurora.ViewModels;
using Microsoft.Maui.Controls;

namespace Aurora.Maui.Views;

public partial class FavoritesPage : ContentPage
{
    readonly FavoritesViewModel viewModel;

    public FavoritesPage(FavoritesViewModel viewModel)
    {
        AuroraPage.Configure(this);
        BindingContext = this.viewModel = viewModel;
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        viewModel.Refresh();
    }
}
