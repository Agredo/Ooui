using Aurora.ViewModels;
using Microsoft.Maui.Controls;

namespace Aurora.Maui.Views;

public partial class DestinationPage : ContentPage
{
    public DestinationPage(DestinationDetailViewModel viewModel)
    {
        AuroraPage.Configure(this);
        BindingContext = viewModel;
        InitializeComponent();
    }
}
