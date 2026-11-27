using Aurora.Models;

namespace Aurora.Services;

public interface IDestinationService
{
    IReadOnlyList<Destination> GetAll();
    Destination? Find(string? id);
}

public interface IFavoritesService
{
    IReadOnlyCollection<string> Ids { get; }
    int Count { get; }
    event EventHandler? Changed;
    bool IsFavorite(string destinationId);
    bool Toggle(string destinationId);
}

public interface ITripService
{
    TripPlan Current { get; }
    event EventHandler? Changed;
    void Select(string destinationId);
    void Update(int travelers, int month);
    bool ToggleSaved();
}

public interface IAuroraNavigator
{
    Task ExploreAsync();
    Task OpenDestinationAsync(string destinationId);
    Task OpenFavoritesAsync();
    Task BackAsync();
}
