namespace Sample.Services;

/// <summary>
/// The counter behind both pages. It is the only place that changes the number, so the value
/// survives navigating between pages and - on the ASP.NET Core host - between requests.
/// </summary>
public interface ICounterService
{
    int Count { get; }

    DateTimeOffset? LastClick { get; }

    /// <summary>Counts one click and returns the new value.</summary>
    int Increment ();

    void Reset ();
}
