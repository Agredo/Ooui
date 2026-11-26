using Microsoft.Maui.Controls;

namespace MauiDemoUi.Mvvm;

/// <summary>
/// The whole code-behind of the MVVM sample: create the view model, hand it to the bindings.
/// </summary>
public partial class TasksView : ContentView
{
    public TasksView ()
    {
        InitializeComponent ();
        BindingContext = new TasksViewModel ();
    }
}
