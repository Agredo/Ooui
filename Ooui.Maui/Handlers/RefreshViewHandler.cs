using System.ComponentModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using MauiRefreshView = Microsoft.Maui.Controls.RefreshView;

namespace Ooui.Maui.Handlers;

/// <summary>
/// Renders a <see cref="MauiRefreshView"/> with an explicit refresh bar, because browsers have no pull-to-refresh.
/// </summary>
public class RefreshViewHandler : OouiViewHandler<MauiRefreshView, Div>
{
    public const string BarClassName = "ooui-refresh-bar";
    public const string ButtonClassName = "ooui-refresh-button";
    public const string StatusClassName = "ooui-refresh-status";
    public const string ContentClassName = "ooui-refresh-content";

    public static readonly IPropertyMapper<MauiRefreshView, RefreshViewHandler> Mapper =
        new PropertyMapper<MauiRefreshView, RefreshViewHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IContentView.Content)] = MapContent,
            [nameof(MauiRefreshView.IsRefreshing)] = MapRefreshState,
            [nameof(MauiRefreshView.IsRefreshEnabled)] = MapRefreshState,
            [nameof(MauiRefreshView.Command)] = MapRefreshState,
            [nameof(MauiRefreshView.CommandParameter)] = MapRefreshState,
            [nameof(MauiRefreshView.RefreshColor)] = MapRefreshColor,
        };

    readonly Div bar = new();
    readonly Button button = new("Refresh");
    readonly Span status = new();
    readonly Div contentHost = new();
    Element? content;

    public RefreshViewHandler() : base(Mapper) { }

    protected override Div CreatePlatformView()
    {
        var root = new Div();
        root.Style["display"] = "flex";
        root.Style["flex-direction"] = "column";
        root.Style["gap"] = "8px";

        bar.ClassName = BarClassName;
        bar.Style["display"] = "flex";
        bar.Style["flex-direction"] = "row";
        bar.Style["align-items"] = "center";
        bar.Style["gap"] = "8px";

        button.ClassName = ButtonClassName;
        status.ClassName = StatusClassName;

        bar.AppendChild(button);
        bar.AppendChild(status);

        contentHost.ClassName = ContentClassName;
        contentHost.Style["flex"] = "1 1 auto";
        contentHost.Style["min-height"] = "0";

        root.AppendChild(bar);
        root.AppendChild(contentHost);
        return root;
    }

    protected override void ConnectHandler(Div platformView)
    {
        base.ConnectHandler(platformView);
        button.Click += OnRefreshClicked;
        VirtualView.PropertyChanged += OnVirtualViewPropertyChanged;
        UpdateContent();
        UpdateRefreshState();
    }

    protected override void DisconnectHandler(Div platformView)
    {
        button.Click -= OnRefreshClicked;
        VirtualView.PropertyChanged -= OnVirtualViewPropertyChanged;
        content = null;
        base.DisconnectHandler(platformView);
    }

    // The mapper is not invoked for every change, so the state is refreshed from the notifications as well.
    void OnVirtualViewPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(MauiRefreshView.IsRefreshing):
            case nameof(MauiRefreshView.IsRefreshEnabled):
            case nameof(MauiRefreshView.Command):
                UpdateRefreshState();
                break;
            case nameof(MauiRefreshView.Content):
                UpdateContent();
                break;
        }
    }

    void OnRefreshClicked(object? sender, TargetEventArgs args)
    {
        // A browser has no pull gesture, so the button starts the same cycle. RefreshView runs the command itself.
        if (!VirtualView.IsRefreshing && VirtualView.IsRefreshEnabled)
            VirtualView.IsRefreshing = true;
    }

    void UpdateContent()
    {
        if (content is not null && contentHost.Children.Contains(content))
            contentHost.RemoveChild(content);
        content = null;
        if (VirtualView.Content is IView view && MauiContext is not null)
        {
            content = view.ToOouiElement(MauiContext);
            contentHost.AppendChild(content);
        }
    }

    void UpdateRefreshState()
    {
        var refreshing = VirtualView.IsRefreshing;
        var command = VirtualView.Command;
        var canExecute = command is null || command.CanExecute(VirtualView.CommandParameter);

        button.IsDisabled = refreshing || !VirtualView.IsRefreshEnabled || !canExecute;
        status.Text = refreshing ? "Refreshing…" : "";
        contentHost.Style["opacity"] = refreshing ? "0.5" : null;
        if (refreshing)
            PlatformView.SetAttribute("aria-busy", "true");
        else
            PlatformView.RemoveAttribute("aria-busy");
    }

    public static void MapContent(RefreshViewHandler handler, MauiRefreshView view) => handler.UpdateContent();

    public static void MapRefreshState(RefreshViewHandler handler, MauiRefreshView view) => handler.UpdateRefreshState();

    public static void MapRefreshColor(RefreshViewHandler handler, MauiRefreshView view) =>
        handler.status.Style.Color = view.RefreshColor is { } color ? OouiStyle.ToCss(color) : null;
}
