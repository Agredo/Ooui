using Aurora.Services;

namespace Aurora.ViewModels.Tests;

sealed class RecordingNavigator : IAuroraNavigator
{
    public List<string> Destinations { get; } = [];
    public int ExploreCount { get; private set; }
    public int FavoritesCount { get; private set; }
    public int BackCount { get; private set; }

    public Task ExploreAsync()
    {
        ExploreCount++;
        return Task.CompletedTask;
    }

    public Task OpenDestinationAsync(string destinationId)
    {
        Destinations.Add(destinationId);
        return Task.CompletedTask;
    }

    public Task OpenFavoritesAsync()
    {
        FavoritesCount++;
        return Task.CompletedTask;
    }

    public Task BackAsync()
    {
        BackCount++;
        return Task.CompletedTask;
    }
}
