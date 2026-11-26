namespace Sample.Models;

/// <summary>
/// The counters kept by the counter service. The service owns this model, so both pages and all
/// backends see the same numbers.
/// </summary>
public sealed class CounterState
{
    public int Count { get; private set; }

    public DateTimeOffset? LastClick { get; private set; }

    public void Increase ()
    {
        Count++;
        LastClick = DateTimeOffset.Now;
    }

    public void Reset ()
    {
        Count = 0;
        LastClick = null;
    }
}
