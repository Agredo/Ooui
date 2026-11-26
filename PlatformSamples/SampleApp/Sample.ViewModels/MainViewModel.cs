using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sample.Models;
using Sample.Services;

namespace Sample.ViewModels;

/// <summary>
/// The home page of the template: the counter. Everything the page shows comes from here, and the
/// numbers come from <see cref="ICounterService"/>, so the page itself has no state.
/// </summary>
public partial class MainViewModel : ObservableObject
{
    readonly ICounterService counter;
    readonly IAppNavigator navigator;

    public MainViewModel (
        BackendInfo backend,
        IReadOnlyList<BackendInfo> backends,
        ICounterService counter,
        IAppNavigator navigator)
    {
        Backend = backend;
        Backends = backends;
        this.counter = counter;
        this.navigator = navigator;
        count = counter.Count;
        status = Describe (counter.LastClick);
    }

    /// <summary>The backend this instance of the app is currently rendered by.</summary>
    public BackendInfo Backend { get; }

    /// <summary>All backends of the sample, listed on the details page.</summary>
    public IReadOnlyList<BackendInfo> Backends { get; }

    public string BackendName => Backend.Name;

    public string BackendSummary => Backend.Summary;

    public string BackendProject => Backend.Project;

    [ObservableProperty]
    [NotifyPropertyChangedFor (nameof (CountText))]
    int count;

    [ObservableProperty]
    string status;

    public string CountText => $"Current count: {Count}";

    [RelayCommand]
    void Increment ()
    {
        Count = counter.Increment ();
        Status = Describe (counter.LastClick);
    }

    [RelayCommand]
    Task OpenDetailsAsync () => navigator.NavigateAsync (AppRoute.Details);

    static string Describe (DateTimeOffset? lastClick) =>
        lastClick is null
            ? "The counter is stored in the counter service, not in the page."
            : $"Last click at {lastClick.Value:t}.";
}
