using Sample.Models;

namespace Sample.Services;

/// <summary>The default counter: the numbers live in the <see cref="IStateStore"/> of the host.</summary>
public sealed class CounterService (IStateStore store) : ICounterService
{
    public const string StateKey = "sample.counter";

    CounterState State => store.GetOrAdd (StateKey, () => new CounterState ());

    public int Count => State.Count;

    public DateTimeOffset? LastClick => State.LastClick;

    public int Increment ()
    {
        var state = State;
        state.Increase ();
        return state.Count;
    }

    public void Reset () => State.Reset ();
}
