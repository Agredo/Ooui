using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sample.Models;
using Sample.Services;

namespace Sample.ViewModels;

/// <summary>
/// The second page. It shows the same counter as the home page and lists the backends the app can
/// run on, which proves that the app - not the host - owns the content.
/// </summary>
public partial class DetailsViewModel : ObservableObject
{
    readonly ICounterService counter;
    readonly IAppNavigator navigator;

    public DetailsViewModel (
        BackendInfo backend,
        IReadOnlyList<BackendInfo> backends,
        ICounterService counter,
        IAppNavigator navigator)
    {
        Backend = backend;
        Backends = backends;
        this.counter = counter;
        this.navigator = navigator;
        lastClickText = Describe (counter.LastClick);
    }

    public BackendInfo Backend { get; }

    public IReadOnlyList<BackendInfo> Backends { get; }

    public string BackendName => Backend.Name;

    public string BackendProject => Backend.Project;

    public string BackendSummary => Backend.Summary;

    int count => counter.Count;

    public string CountText => $"The counter is at {count} right now.";

    [ObservableProperty]
    string lastClickText;

    [RelayCommand]
    Task GoBackAsync () => navigator.GoBackAsync ();

    [RelayCommand]
    Task GoHomeAsync () => navigator.NavigateAsync (AppRoute.Home);

    static string Describe (DateTimeOffset? lastClick) =>
        lastClick is null
            ? "You have not clicked the counter yet."
            : $"The last click was at {lastClick.Value:t}.";
}
