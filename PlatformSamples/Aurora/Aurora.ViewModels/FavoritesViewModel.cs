using Aurora.Models;
using Aurora.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aurora.ViewModels;

public partial class FavoritesViewModel : ObservableObject
{
    readonly IDestinationService destinations;
    readonly IFavoritesService favorites;
    readonly IAuroraNavigator navigator;

    public FavoritesViewModel(IDestinationService destinations, IFavoritesService favorites, IAuroraNavigator navigator)
    {
        this.destinations = destinations;
        this.favorites = favorites;
        this.navigator = navigator;
    }

    public IReadOnlyList<DestinationCardViewModel> SavedDestinations => destinations.GetAll()
        .Where(destination => favorites.IsFavorite(destination.Id))
        .Select(destination => new DestinationCardViewModel(destination, favorites, navigator))
        .ToArray();

    public bool HasFavorites => favorites.Count > 0;
    public string Summary => favorites.Count == 0
        ? "Keep the places that make you pause."
        : $"{favorites.Count} {(favorites.Count == 1 ? "place" : "places")} on your someday list.";

    public void Refresh()
    {
        OnPropertyChanged(nameof(SavedDestinations));
        OnPropertyChanged(nameof(HasFavorites));
        OnPropertyChanged(nameof(Summary));
    }

    [RelayCommand]
    Task Explore() => navigator.ExploreAsync();
}
