using Aurora.Services;

namespace Aurora.ViewModels.Tests;

[TestClass]
public sealed class DestinationDetailViewModelTests
{
    [TestMethod]
    public async Task FavoriteTravelersAndPlanStayInTheirServices()
    {
        var destinations = new DestinationService();
        var favorites = new FavoritesService();
        var trips = new TripService();
        var navigator = new RecordingNavigator();
        var viewModel = new DestinationDetailViewModel(favorites, trips, navigator);
        viewModel.Load(destinations.Find("kyoto")!);

        Assert.AreEqual("From €3.780", viewModel.TotalPrice);
        Assert.AreEqual("2 travellers · 6 days", viewModel.TravelerSummary);
        Assert.HasCount(3, viewModel.Journey);

        viewModel.Travelers = 4;
        Assert.AreEqual("From €7.560", viewModel.TotalPrice);
        Assert.AreEqual("4 travellers · 6 days", viewModel.TravelerSummary);

        viewModel.ToggleFavoriteCommand.Execute(null);
        Assert.IsTrue(favorites.IsFavorite("kyoto"));
        Assert.AreEqual("♥ Saved", viewModel.FavoriteButtonText);

        viewModel.ToggleSavedCommand.Execute(null);
        Assert.AreEqual(new("kyoto", 4, 6, true), trips.Current);
        Assert.AreEqual("✓ Added to your plans", viewModel.PlannerButtonText);

        viewModel.ToggleSavedCommand.Execute(null);
        Assert.IsFalse(trips.Current.IsSaved);
        await viewModel.BackCommand.ExecuteAsync(null);
        Assert.AreEqual(1, navigator.ExploreCount);
    }

    [TestMethod]
    public void UnloadedViewModelIsSafeForInitialBindings()
    {
        var viewModel = new DestinationDetailViewModel(new FavoritesService(), new TripService(), new RecordingNavigator());
        Assert.AreEqual(string.Empty, viewModel.TotalPrice);
        Assert.AreEqual(string.Empty, viewModel.TravelerSummary);
        Assert.AreEqual(string.Empty, viewModel.ImageUrl);
        Assert.IsEmpty(viewModel.Journey);
    }
}
