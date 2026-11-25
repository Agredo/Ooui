using System.Collections.Concurrent;
using Microsoft.Maui.Dispatching;

namespace Ooui.Maui;

/// <summary>Queues MAUI work on a dedicated server thread.</summary>
public sealed class OouiDispatcher : IDispatcher, IDispatcherProvider
{
    readonly BlockingCollection<Action> work = new();

    public OouiDispatcher()
    {
        var thread = new Thread(Run) { IsBackground = true, Name = "Ooui MAUI dispatcher" };
        thread.Start();
    }

    public bool IsDispatchRequired => true;

    public IDispatcher GetForCurrentThread() => this;

    public bool Dispatch(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        work.Add(action);
        return true;
    }

    public bool DispatchDelayed(TimeSpan delay, Action action)
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(delay).ConfigureAwait(false);
            Dispatch(action);
        });
        return true;
    }

    public IDispatcherTimer CreateTimer() => new OouiDispatcherTimer(this);

    void Run()
    {
        SynchronizationContext.SetSynchronizationContext(new OouiSynchronizationContext(this));
        foreach (var action in work.GetConsumingEnumerable())
        {
            try { action(); }
            catch (Exception ex) { Console.Error.WriteLine($"Ooui dispatcher error: {ex}"); }
        }
    }

    sealed class OouiSynchronizationContext(OouiDispatcher dispatcher) : SynchronizationContext
    {
        public override void Post(SendOrPostCallback d, object? state) => dispatcher.Dispatch(() => d(state));
        public override void Send(SendOrPostCallback d, object? state)
        {
            if (SynchronizationContext.Current == this) d(state);
            else dispatcher.Dispatch(() => d(state));
        }
    }

    sealed class OouiDispatcherTimer(OouiDispatcher dispatcher) : IDispatcherTimer
    {
        Timer? timer;
        public TimeSpan Interval { get; set; }
        public bool IsRepeating { get; set; } = true;
        public bool IsRunning => timer is not null;
        public event EventHandler? Tick;
        public void Start() => timer = new Timer(_ => dispatcher.Dispatch(() => Tick?.Invoke(this, EventArgs.Empty)), null, Interval, IsRepeating ? Interval : Timeout.InfiniteTimeSpan);
        public void Stop() { timer?.Dispose(); timer = null; }
    }
}
