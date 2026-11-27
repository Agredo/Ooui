using Aurora.Models;

namespace Aurora.Services;

public sealed class TripService : ITripService
{
    public TripPlan Current { get; private set; } = new("iceland", 2, 6, false);
    public event EventHandler? Changed;

    public void Select(string destinationId)
    {
        Current = Current with { DestinationId = destinationId, IsSaved = false };
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Update(int travelers, int month)
    {
        Current = Current with { Travelers = Math.Clamp(travelers, 1, 8), Month = Math.Clamp(month, 1, 12) };
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public bool ToggleSaved()
    {
        Current = Current with { IsSaved = !Current.IsSaved };
        Changed?.Invoke(this, EventArgs.Empty);
        return Current.IsSaved;
    }
}
