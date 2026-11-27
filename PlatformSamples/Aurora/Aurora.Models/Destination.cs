namespace Aurora.Models;

public sealed record Destination(
    string Id,
    string Name,
    string Country,
    string Region,
    string Eyebrow,
    string Description,
    string Story,
    string Image,
    string Season,
    string Duration,
    string Pace,
    decimal Price,
    double Rating,
    IReadOnlyList<string> Categories,
    IReadOnlyList<Experience> Experiences,
    IReadOnlyList<ItineraryDay> Itinerary);

public sealed record Experience(string Name, string Detail, string Icon);

public sealed record ItineraryDay(string Title, string Detail, string Image);

public sealed record TripPlan(string DestinationId, int Travelers, int Month, bool IsSaved);

public sealed record DestinationFilter(string Category, string Query, bool FavoritesOnly);
