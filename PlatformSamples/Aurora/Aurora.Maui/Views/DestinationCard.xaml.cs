using Aurora.Maui.Resources;
using Aurora.ViewModels;
using Microsoft.Maui.Controls;

namespace Aurora.Maui.Views;

public partial class DestinationCard : ContentView
{
    public DestinationCard()
    {
        Resources.MergedDictionaries.Add(new AuroraStyles());
        InitializeComponent();
    }
}
