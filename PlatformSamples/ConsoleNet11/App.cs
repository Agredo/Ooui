using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Application = Microsoft.Maui.Controls.Application;

namespace ConsoleNet11;

public class App : Application
{
    protected override Microsoft.Maui.Controls.Window CreateWindow(IActivationState? activationState) =>
        new(new MainPage());
}
