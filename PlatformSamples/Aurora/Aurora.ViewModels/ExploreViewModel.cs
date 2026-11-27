using System.Collections.ObjectModel;
using Aurora.Models;
using Aurora.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aurora.ViewModels;

public partial class ExploreViewModel : ObservableObject
{
    readonly IDestinationService destinations;
    readonly IFavoritesService favorites;
    readonly IAuroraNavigator navigator;
    readonly Dictionary<string, DestinationCardViewModel> cards;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasResults), nameof(EmptyTitle), nameof(EmptyMessage), nameof(FilterButtonText), nameof(ResultsTitle))]
    private string selectedCategory = "All";

    [ObservableProperty]
    private string query = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilterButtonText), nameof(ResultsTitle))]
    private bool favoritesOnly;

    public ExploreViewModel(IDestinationService destinations, IFavoritesService favorites, IAuroraNavigator navigator)
    {
        this.destinations = destinations;
        this.favorites = favorites;
        this.navigator = navigator;
        cards = destinations.GetAll().ToDictionary(
            destination => destination.Id,
            destination => new DestinationCardViewModel(destination, favorites, navigator));
        favorites.Changed += (_, _) => Refresh();
        Refresh();
    }

    public string HeroImage => "images/lofoten.webp";
    public IReadOnlyList<string> Categories { get; } = ["All", "Nature", "Culture", "Hiking", "Food", "Ocean", "Wellness"];
    public ObservableCollection<DestinationCardViewModel> Destinations { get; } = [];
    public string FavoriteCount => favorites.Count.ToString();
    public string FavoritesLabel => $"Saved · {FavoriteCount}";
    public string FilterButtonText => FavoritesOnly ? "Show all journeys" : "Saved journeys";
    public string ResultsTitle => FavoritesOnly ? "Your saved journeys" : "Find your kind of away";
    public bool HasResults => Destinations.Count > 0;
    public string EmptyTitle => FavoritesOnly ? "A place for your someday" : "No journeys found";
    public string EmptyMessage => FavoritesOnly
        ? "Tap the heart on a journey and it will be waiting here."
        : "Try another search or explore all of our journeys.";
    public string ResultSummary => $"{Destinations.Count} thoughtfully paced {(Destinations.Count == 1 ? "journey" : "journeys")}";

    partial void OnSelectedCategoryChanged(string value) => Refresh();
    partial void OnQueryChanged(string value) => Refresh();
    partial void OnFavoritesOnlyChanged(bool value) => Refresh();

    [RelayCommand]
    void SelectCategory(string category) => SelectedCategory = category;

    [RelayCommand]
    void ToggleFavorites() => FavoritesOnly = !FavoritesOnly;

    [RelayCommand]
    void ClearFilters()
    {
        Query = string.Empty;
        SelectedCategory = "All";
        FavoritesOnly = false;
    }

    [RelayCommand]
    Task OpenDestination(string id) => navigator.OpenDestinationAsync(id);

    [RelayCommand]
    Task Explore() => navigator.OpenDestinationAsync("lofoten");

    [RelayCommand]
    Task OpenFavorites() => navigator.OpenFavoritesAsync();

    void Refresh()
    {
        var filtered = destinations.GetAll().Where(destination =>
            (SelectedCategory == "All" || destination.Categories.Contains(SelectedCategory)) &&
            (!FavoritesOnly || favorites.IsFavorite(destination.Id)) &&
            (string.IsNullOrWhiteSpace(Query) ||
             $"{destination.Name} {destination.Country} {destination.Region} {destination.Description} {string.Join(' ', destination.Categories)}"
                .Contains(Query.Trim(), StringComparison.OrdinalIgnoreCase)));
        Destinations.Clear();
        foreach (var destination in filtered)
            Destinations.Add(cards[destination.Id]);
        OnPropertyChanged(nameof(FavoriteCount));
        OnPropertyChanged(nameof(FavoritesLabel));
        OnPropertyChanged(nameof(HasResults));
        OnPropertyChanged(nameof(EmptyTitle));
        OnPropertyChanged(nameof(EmptyMessage));
        OnPropertyChanged(nameof(ResultSummary));
        OnPropertyChanged(nameof(ResultsTitle));
    }
}
