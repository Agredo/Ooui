using Aurora.Maui.Views;
using Aurora.Services;
using Microsoft.Maui.Controls;

namespace Aurora.Maui.Navigation;

public sealed class AuroraNavigator(IServiceProvider services) : IAuroraNavigator
{
    INavigation? Navigation => Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation;

    public async Task ExploreAsync()
    {
        if (Navigation is { } navigation) await navigation.PopToRootAsync();
    }

    public async Task OpenDestinationAsync(string destinationId)
    {
        if (Navigation is not { } navigation) return;
        var destination = services.GetRequiredService<IDestinationService>().Find(destinationId);
        if (destination is null) return;
        var viewModel = services.GetRequiredService<ViewModels.DestinationDetailViewModel>();
        viewModel.Load(destination);
        await navigation.PushAsync(new DestinationPage(viewModel));
    }

    public async Task OpenFavoritesAsync()
    {
        if (Navigation is { } navigation)
            await navigation.PushAsync(services.GetRequiredService<FavoritesPage>());
    }

    public async Task BackAsync()
    {
        if (Navigation is { NavigationStack.Count: > 1 } navigation) await navigation.PopAsync();
    }
}
