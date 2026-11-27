using Aurora.Models;
using Aurora.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aurora.ViewModels;

public partial class DestinationCardViewModel : ObservableObject
{
    readonly IFavoritesService favorites;
    readonly IAuroraNavigator navigator;

    public DestinationCardViewModel(Destination destination, IFavoritesService favorites, IAuroraNavigator navigator)
    {
        Destination = destination;
        this.favorites = favorites;
        this.navigator = navigator;
        this.favorites.Changed += (_, _) =>
        {
            OnPropertyChanged(nameof(IsFavorite));
            OnPropertyChanged(nameof(FavoriteIcon));
            OnPropertyChanged(nameof(FavoriteLabel));
        };
    }

    public Destination Destination { get; }
    public bool IsFavorite => favorites.IsFavorite(Destination.Id);
    public string FavoriteIcon => IsFavorite ? "♥" : "♡";
    public string FavoriteLabel => IsFavorite ? "Saved" : "Save";
    public string ImageUrl => Destination.Image;
    public string Category => Destination.Categories.FirstOrDefault() ?? "Escape";
    public string Location => $"{Destination.Country} · {Destination.Region}";
    public string Price => $"From €{Destination.Price:N0}";
    public string Rating => $"★ {Destination.Rating:0.0}";
    public string Details => $"{Destination.Duration} · {Destination.Season}";

    [RelayCommand]
    void ToggleFavorite() => favorites.Toggle(Destination.Id);

    [RelayCommand]
    Task Open() => navigator.OpenDestinationAsync(Destination.Id);
}
