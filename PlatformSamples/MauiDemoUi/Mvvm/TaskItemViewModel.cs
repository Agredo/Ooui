using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiDemoUi.Mvvm;

/// <summary>
/// One row of the list. The item owns its own <see cref="RemoveCommand"/> so the XAML template can
/// bind to it directly instead of climbing the visual tree.
/// </summary>
public partial class TaskItemViewModel : ObservableObject
{
    readonly TasksViewModel owner;

    public TaskItemViewModel (TasksViewModel owner, string title)
    {
        this.owner = owner;
        Title = title;
    }

    public string Title { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor (nameof (State))]
    private bool isDone;

    /// <summary>Computed by the toolkit: changing <see cref="IsDone"/> also raises this one.</summary>
    public string State => IsDone ? "done" : "open";

    [RelayCommand]
    void Remove () => owner.Remove (this);

    // Generated hook: runs after IsDone changed, so the list header can refresh.
    partial void OnIsDoneChanged (bool value) => owner.RefreshSummary ();
}
