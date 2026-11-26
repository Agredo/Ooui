using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ConsoleNet11;

sealed class LaunchModel : INotifyPropertyChanged
{
    string projectName = "Northstar";
    string role = "Marketing site";
    double progress = 68;
    bool notifications;
    string notes = string.Empty;

    public string ProjectName { get => projectName; set => Set(ref projectName, value); }
    public string Role { get => role; set => Set(ref role, value); }
    public double Progress { get => progress; set => Set(ref progress, value); }
    public bool Notifications { get => notifications; set => Set(ref notifications, value); }
    public string Notes { get => notes; set => Set(ref notes, value); }
    public string ProgressText => $"Launch readiness · {Progress:0}%";
    public string Summary => string.IsNullOrWhiteSpace(ProjectName) ? "Give your project a name." : $"{ProjectName} · {Role}";

    public event PropertyChangedEventHandler? PropertyChanged;

    void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressText)));
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Summary)));
    }
}
