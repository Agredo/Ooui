using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiDemoUi.Mvvm;

/// <summary>
/// The whole state of the task list. Nothing here knows about Ooui, HTML or the browser: the page
/// only binds to it, and the toolkit turns the attributes into properties and commands.
/// </summary>
public partial class TasksViewModel : ObservableObject
{
    static readonly string[] Suggestions = [
        "Write the release notes",
        "Review the Ooui handlers",
        "Publish the WebAssembly bundle",
        "Close the open backlog",
    ];

    int suggestion;

    public TasksViewModel ()
    {
        Tasks.Add (new TaskItemViewModel (this, "Try the MVVM page"));
        Tasks.Add (new TaskItemViewModel (this, "Add a task with the entry above"));
        Tasks[0].IsDone = true;
        RefreshSummary ();
    }

    public ObservableCollection<TaskItemViewModel> Tasks { get; } = new ();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor (nameof (AddTaskCommand))]
    private string newTaskTitle = "";

    [ObservableProperty]
    private string status = "Nothing changed yet.";

    /// <summary>Computed from the collection, refreshed by <see cref="RefreshSummary"/>.</summary>
    public string Summary => $"{Tasks.Count (task => !task.IsDone)} open · {Tasks.Count} total";

    bool CanAddTask () => !string.IsNullOrWhiteSpace (NewTaskTitle);

    [RelayCommand (CanExecute = nameof (CanAddTask))]
    void AddTask ()
    {
        var title = NewTaskTitle.Trim ();
        NewTaskTitle = "";
        Tasks.Add (new TaskItemViewModel (this, title));
        Status = $"Added “{title}” · {Tasks.Count} tasks in the list.";
        RefreshSummary ();
    }

    [RelayCommand]
    void AddSuggestion ()
    {
        var title = Suggestions[suggestion++ % Suggestions.Length];
        Tasks.Add (new TaskItemViewModel (this, title));
        Status = $"Suggested “{title}”.";
        RefreshSummary ();
    }

    [RelayCommand]
    void ClearDone ()
    {
        var done = Tasks.Where (task => task.IsDone).ToList ();
        if (done.Count == 0) {
            Status = "Nothing is done yet.";
            return;
        }
        foreach (var task in done)
            Tasks.Remove (task);
        Status = $"Removed {done.Count} finished task(s).";
        RefreshSummary ();
    }

    internal void Remove (TaskItemViewModel item)
    {
        Tasks.Remove (item);
        Status = $"Removed “{item.Title}”.";
        RefreshSummary ();
    }

    internal void RefreshSummary () => OnPropertyChanged (nameof (Summary));
}
