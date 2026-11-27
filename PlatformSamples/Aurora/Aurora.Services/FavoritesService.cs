namespace Aurora.Services;

public sealed class FavoritesService : IFavoritesService
{
    readonly HashSet<string> ids = new(StringComparer.Ordinal);

    public IReadOnlyCollection<string> Ids => ids;
    public int Count => ids.Count;
    public event EventHandler? Changed;

    public bool IsFavorite(string destinationId) => ids.Contains(destinationId);

    public bool Toggle(string destinationId)
    {
        var added = ids.Add(destinationId);
        if (!added) ids.Remove(destinationId);
        Changed?.Invoke(this, EventArgs.Empty);
        return added;
    }
}
