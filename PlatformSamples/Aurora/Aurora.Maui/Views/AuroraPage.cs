using Aurora.Maui.Resources;
using Microsoft.Maui.Controls;

namespace Aurora.Maui.Views;

public static class AuroraPage
{
    public static void Configure(ContentPage page)
    {
        page.Resources.MergedDictionaries.Add(new AuroraStyles());
        page.Resources["DestinationCardTemplate"] = new DataTemplate(static () => new DestinationCard());
        NavigationPage.SetHasNavigationBar(page, false);
    }
}
