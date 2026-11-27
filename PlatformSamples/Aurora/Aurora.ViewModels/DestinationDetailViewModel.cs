using Aurora.Models;
using Aurora.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Aurora.ViewModels;

public partial class DestinationDetailViewModel : ObservableObject
{
    readonly IFavoritesService favorites;
    readonly ITripService trips;
    readonly IAuroraNavigator navigator;

    [ObservableProperty]
    private Destination destination = null!;

    [ObservableProperty]
    private bool isFavorite;

    [ObservableProperty]
    private bool isSaved;

    [ObservableProperty]
    private int travelers = 2;

    public DestinationDetailViewModel(IFavoritesService favorites, ITripService trips, IAuroraNavigator navigator)
    {
        this.favorites = favorites;
        this.trips = trips;
        this.navigator = navigator;
    }

    public void Load(Destination destination)
    {
        Destination = destination;
        Travelers = trips.Current.Travelers;
        IsFavorite = favorites.IsFavorite(destination.Id);
        IsSaved = trips.Current.DestinationId == destination.Id && trips.Current.IsSaved;
        RefreshDetails();
    }

    public string FavoriteButtonText => IsFavorite ? "♥ Saved" : "♡ Save journey";
    public string PlannerButtonText => IsSaved ? "✓ Added to your plans" : "Plan this journey";
    public string TotalPrice => Destination is null ? string.Empty : $"From €{Destination.Price * Travelers:N0}";
    public string TravelerSummary => Destination is null ? string.Empty : $"{Travelers} {(Travelers == 1 ? "traveller" : "travellers")} · {Destination.Duration}";
    public string ImageUrl => Destination?.Image ?? string.Empty;
    public string Rating => Destination is null ? string.Empty : $"★ {Destination.Rating:0.0}";
    public string TripLabel => Destination is null ? string.Empty : $"{Destination.Duration} / {Destination.Pace}";
    public IReadOnlyList<JourneyStepViewModel> Journey => Destination is null
        ? Array.Empty<JourneyStepViewModel>()
        : Destination.Itinerary.Select((day, index) => new JourneyStepViewModel(index + 1, day)).ToArray();

    partial void OnDestinationChanged(Destination value) => RefreshDetails();
    partial void OnTravelersChanged(int value) => RefreshPrice();

    [RelayCommand]
    void ToggleFavorite()
    {
        IsFavorite = favorites.Toggle(Destination.Id);
        OnPropertyChanged(nameof(FavoriteButtonText));
    }

    [RelayCommand]
    void ToggleSaved()
    {
        if (trips.Current.DestinationId != Destination.Id)
            trips.Select(Destination.Id);
        IsSaved = trips.ToggleSaved();
        trips.Update(Travelers, 6);
        OnPropertyChanged(nameof(PlannerButtonText));
    }

    [RelayCommand]
    Task Back() => navigator.ExploreAsync();

    [RelayCommand]
    Task Explore() => navigator.ExploreAsync();

    void RefreshPrice() => OnPropertyChanged(nameof(TotalPrice));

    void RefreshDetails()
    {
        if (Destination is null) return;
        OnPropertyChanged(nameof(ImageUrl));
        OnPropertyChanged(nameof(Rating));
        OnPropertyChanged(nameof(TripLabel));
        OnPropertyChanged(nameof(Journey));
        OnPropertyChanged(nameof(FavoriteButtonText));
        OnPropertyChanged(nameof(PlannerButtonText));
        OnPropertyChanged(nameof(TravelerSummary));
        RefreshPrice();
    }
}
