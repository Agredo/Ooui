using Microsoft.Maui.Controls;

namespace AspNetCoreMaui;

/// <summary>
/// Colours and text styles shared by every page of the sample. Pages merge it in their own
/// resources, so the XAML below only says <c>{StaticResource Card}</c> instead of repeating five
/// setters per card.
/// </summary>
public partial class Theme : ResourceDictionary
{
    public Theme () => InitializeComponent ();
}
