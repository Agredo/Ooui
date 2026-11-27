using Aurora.ViewModels;
using Microsoft.Maui.Controls;

namespace Aurora.Maui.Views;

public partial class ExplorePage : ContentPage
{
    public ExplorePage(ExploreViewModel viewModel)
    {
        AuroraPage.Configure(this);
        BindingContext = viewModel;
        InitializeComponent();
    }
}
