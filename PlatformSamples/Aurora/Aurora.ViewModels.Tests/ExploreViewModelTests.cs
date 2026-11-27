using Aurora.Services;

namespace Aurora.ViewModels.Tests;

[TestClass]
public sealed class ExploreViewModelTests
{
    DestinationService destinations = null!;
    FavoritesService favorites = null!;
    RecordingNavigator navigator = null!;
    ExploreViewModel viewModel = null!;

    [TestInitialize]
    public void Setup()
    {
        destinations = new DestinationService();
        favorites = new FavoritesService();
        navigator = new RecordingNavigator();
        viewModel = new ExploreViewModel(destinations, favorites, navigator);
    }

    [TestMethod]
    public void SearchFiltersAcrossDestinationContent()
    {
        viewModel.Query = "  Kyoto  ";
        Assert.HasCount(1, viewModel.Destinations);
        Assert.AreEqual("kyoto", viewModel.Destinations[0].Destination.Id);
        Assert.AreEqual("1 thoughtfully paced journey", viewModel.ResultSummary);

        viewModel.Query = "lantern";
        Assert.HasCount(1, viewModel.Destinations);

        viewModel.Query = "not a destination";
        Assert.IsEmpty(viewModel.Destinations);
        Assert.IsFalse(viewModel.HasResults);
        Assert.AreEqual("No journeys found", viewModel.EmptyTitle);
    }

    [TestMethod]
    public async Task CategoryAndClearFiltersRestoreAllJourneys()
    {
        viewModel.SelectCategoryCommand.Execute("Hiking");
        Assert.HasCount(2, viewModel.Destinations);
        Assert.IsTrue(viewModel.Destinations.All(card => card.Destination.Categories.Contains("Hiking")));

        viewModel.Query = "Kyoto";
        viewModel.ToggleFavoritesCommand.Execute(null);
        viewModel.ClearFiltersCommand.Execute(null);
        Assert.HasCount(6, viewModel.Destinations);
        Assert.AreEqual("All", viewModel.SelectedCategory);
        Assert.AreEqual(string.Empty, viewModel.Query);
        Assert.IsFalse(viewModel.FavoritesOnly);
    }

    [TestMethod]
    public void FavoriteChangesRefreshFilterAndCard()
    {
        var card = viewModel.Destinations.Single(card => card.Destination.Id == "kyoto");
        card.ToggleFavoriteCommand.Execute(null);
        Assert.IsTrue(card.IsFavorite);
        Assert.AreEqual("♥", card.FavoriteIcon);
        Assert.AreEqual("1", viewModel.FavoriteCount);

        viewModel.ToggleFavoritesCommand.Execute(null);
        Assert.HasCount(1, viewModel.Destinations);
        Assert.AreEqual("Your saved journeys", viewModel.ResultsTitle);

        card.ToggleFavoriteCommand.Execute(null);
        Assert.IsEmpty(viewModel.Destinations);
        Assert.AreEqual("A place for your someday", viewModel.EmptyTitle);
    }

    [TestMethod]
    public async Task CommandsNavigateWithoutReferencingMaui()
    {
        await viewModel.Destinations[0].OpenCommand.ExecuteAsync(null);
        await viewModel.OpenFavoritesCommand.ExecuteAsync(null);
        await viewModel.ExploreCommand.ExecuteAsync(null);
        Assert.AreEqual("iceland", navigator.Destinations[0]);
        Assert.AreEqual("lofoten", navigator.Destinations[1]);
        Assert.AreEqual(1, navigator.FavoritesCount);
    }
}
