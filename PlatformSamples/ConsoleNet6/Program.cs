using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;
using Ooui.Maui;

namespace ConsoleNet6;

public static class Program
{
    public static void Main()
    {
        var app = MauiApp.CreateBuilder()
            .UseMauiApp<App>()
            .UseOoui()
            .Build();

        app.Publish();
        Console.WriteLine($"MAUI 11 on Ooui: {Ooui.UI.GetUrl("/")}");
        Thread.Sleep(Timeout.Infinite);
    }
}