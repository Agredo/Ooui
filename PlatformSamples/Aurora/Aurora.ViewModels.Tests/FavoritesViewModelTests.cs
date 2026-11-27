using Aurora.Services;

namespace Aurora.ViewModels.Tests;

[TestClass]
public sealed class FavoritesViewModelTests
{
    [TestMethod]
    public async Task RefreshProjectsOnlyCurrentFavoritesAndNavigatesBack()
    {
        var destinations = new DestinationService();
        var favorites = new FavoritesService();
        var navigator = new RecordingNavigator();
        var viewModel = new FavoritesViewModel(destinations, favorites, navigator);

        Assert.IsFalse(viewModel.HasFavorites);
        Assert.IsEmpty(viewModel.SavedDestinations);
        Assert.AreEqual("Keep the places that make you pause.", viewModel.Summary);

        favorites.Toggle("kyoto");
        favorites.Toggle("forest");
        viewModel.Refresh();
        Assert.IsTrue(viewModel.HasFavorites);
        Assert.HasCount(2, viewModel.SavedDestinations);
        Assert.AreEqual("2 places on your someday list.", viewModel.Summary);

        favorites.Toggle("kyoto");
        viewModel.Refresh();
        Assert.AreEqual("forest", viewModel.SavedDestinations.Single().Destination.Id);
        await viewModel.ExploreCommand.ExecuteAsync(null);
        Assert.AreEqual(1, navigator.ExploreCount);
    }
}
