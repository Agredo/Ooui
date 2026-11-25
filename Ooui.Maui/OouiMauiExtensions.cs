using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.Hosting;
using Ooui.Maui.Handlers;
using MauiButton = Microsoft.Maui.Controls.Button;
using MauiContentView = Microsoft.Maui.Controls.ContentView;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiLayout = Microsoft.Maui.Controls.Layout;

namespace Ooui.Maui;

public static class OouiMauiExtensions
{
    /// <summary>Registers the experimental Ooui handlers for a server-hosted MAUI application.</summary>
    public static MauiAppBuilder UseOoui(this MauiAppBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        var dispatcher = new OouiDispatcher();
        DispatcherProvider.SetCurrent(dispatcher);
        builder.Services.TryAddSingleton<IDispatcher>(dispatcher);
        builder.Services.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<MauiLabel, LabelHandler>();
            handlers.AddHandler<MauiButton, ButtonHandler>();
            handlers.AddHandler<MauiLayout, LayoutHandler>();
            handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
            handlers.AddHandler<HorizontalStackLayout, LayoutHandler>();
            handlers.AddHandler<ContentPage, ContentViewHandler>();
            handlers.AddHandler<MauiContentView, ContentViewHandler>();
            handlers.AddHandler<IContentView, ContentViewHandler>();
        });
        return builder;
    }
}
