using System.ComponentModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

/// <summary>Renders a <see cref="NavigationPage"/> as a navigation bar plus the current page.</summary>
public class NavigationPageHandler : OouiViewHandler<IStackNavigationView, Div>, INavigationViewHandler
{
    public static readonly IPropertyMapper<IStackNavigationView, NavigationPageHandler> Mapper =
        new PropertyMapper<IStackNavigationView, NavigationPageHandler>(OouiViewMapper.Mapper)
        {
            [nameof(NavigationPage.BarBackgroundColor)] = MapBarBackgroundColor,
            [nameof(NavigationPage.BarTextColor)] = MapBarTextColor,
        };

    public static readonly CommandMapper<IStackNavigationView, NavigationPageHandler> CommandMapper =
        new(OouiViewMapper.CommandMapper)
        {
            [nameof(IStackNavigationView.RequestNavigation)] = MapRequestNavigation,
        };

    readonly Dictionary<IView, Element> pageElements = new();

    Div bar = null!;
    Div host = null!;
    Button backButton = null!;
    Span title = null!;
    Ooui.Window? browserWindow;
    Page? observedPage;
    IReadOnlyList<IView> stack = Array.Empty<IView>();
    int stackCount = 1;
    bool urlSynced;

    public NavigationPageHandler() : base(Mapper, CommandMapper) { }

    IStackNavigationView INavigationViewHandler.VirtualView => VirtualView;
    object INavigationViewHandler.PlatformView => PlatformView;

    protected override Div CreatePlatformView()
    {
        backButton = new Button("←") { Type = ButtonType.Button };
        backButton.Style["display"] = "none";
        backButton.Style["border"] = "1px solid rgba(15, 23, 42, 0.2)";
        backButton.Style["background-color"] = "transparent";
        backButton.Style["border-radius"] = "4px";
        backButton.Style["padding"] = "0 0.5rem";
        backButton.Style["min-width"] = "2rem";
        backButton.SetAttribute("aria-label", "Back");

        title = new Span();
        title.Style.FontWeight = "600";

        bar = new Div(backButton, title);
        bar.Style.Display = "flex";
        bar.Style["align-items"] = "center";
        bar.Style["gap"] = "0.5rem";
        bar.Style["padding"] = "0.5rem 0.75rem";
        bar.Style["border-bottom"] = "1px solid rgba(15, 23, 42, 0.12)";
        bar.Style["min-height"] = "2.75rem";
        bar.SetAttribute("role", "toolbar");

        host = new Div();
        host.Style["flex-grow"] = "1";

        var root = new Div(bar, host);
        root.Style.Display = "flex";
        root.Style["flex-direction"] = "column";
        root.Style["min-height"] = "100%";
        return root;
    }

    protected override void ConnectHandler(Div platformView)
    {
        base.ConnectHandler(platformView);
        backButton.Click += OnBackClicked;
        browserWindow = platformView.Document.Window;
        browserWindow.AddEventListener("hashchange", OnHashChanged);
        MapBarBackgroundColor(this, VirtualView);
        MapBarTextColor(this, VirtualView);
        if (VirtualView is NavigationPage navigationPage)
            Render(new IView[] { navigationPage.RootPage });
    }

    protected override void DisconnectHandler(Div platformView)
    {
        backButton.Click -= OnBackClicked;
        if (browserWindow is not null)
        {
            browserWindow.RemoveEventListener("hashchange", OnHashChanged);
            browserWindow = null;
        }
        if (observedPage is not null)
        {
            observedPage.PropertyChanged -= OnPagePropertyChanged;
            observedPage = null;
        }
        foreach (var page in pageElements.Keys.ToArray())
            page.DisconnectHandlers();
        pageElements.Clear();
        base.DisconnectHandler(platformView);
    }

    public static void MapRequestNavigation(NavigationPageHandler handler, IStackNavigationView view, object? args) =>
        handler.UpdateNavigation(args as NavigationRequest);

    public static void MapBarBackgroundColor(NavigationPageHandler handler, IStackNavigationView view) =>
        handler.bar.Style.BackgroundColor = OouiStyle.ToCss((view as NavigationPage)?.BarBackgroundColor);

    public static void MapBarTextColor(NavigationPageHandler handler, IStackNavigationView view)
    {
        var color = OouiStyle.ToCss((view as NavigationPage)?.BarTextColor);
        handler.title.Style.Color = color;
        handler.backButton.Style.Color = color;
    }

    void UpdateNavigation(NavigationRequest? request)
    {
        if (request is null)
            return;
        Render(request.NavigationStack);
        VirtualView.NavigationFinished(request.NavigationStack);
    }

    void Render(IReadOnlyList<IView> newStack)
    {
        var previousCount = stackCount;
        stack = newStack;
        stackCount = newStack.Count;
        var top = newStack.Count > 0 ? newStack[newStack.Count - 1] : null;
        var topElement = top is null ? null : GetPageElement(top);
        foreach (var child in host.Children.ToArray())
        {
            if (!ReferenceEquals(child, topElement))
                host.RemoveChild(child);
        }
        if (topElement is not null && !host.Children.Contains(topElement))
            host.AppendChild(topElement);
        RemoveUnusedPages(newStack);
        if (browserWindow is not null)
            browserWindow.Call("scrollTo", 0, 0);
        UpdateBar(top as Page);
        SyncUrl(newStack, newStack.Count > previousCount);
    }

    void SyncUrl(IReadOnlyList<IView> newStack, bool isPush)
    {
        if (browserWindow is null)
            return;
        if (!urlSynced)
        {
            if (!isPush)
                return;
            urlSynced = true;
            browserWindow.ReplaceHash(NavigationUrl.GetHash(newStack, newStack.Count - 1));
        }
        var hash = NavigationUrl.GetHash(newStack);
        if (isPush)
            browserWindow.SetHash(hash);
        else
            browserWindow.ReplaceHash(hash);
    }

    void OnHashChanged(object? sender, TargetEventArgs args)
    {
        if (args.Value is not string value || stack.Count == 0)
            return;
        var hash = NavigationUrl.Normalize(value);
        if (!urlSynced)
        {
            urlSynced = true;
            browserWindow?.ReplaceHash(NavigationUrl.GetHash(stack));
            return;
        }
        if (hash == NavigationUrl.GetHash(stack))
            return;
        if (stack.Count <= 1 || hash != NavigationUrl.GetHash(stack, stack.Count - 1))
            return;
        _ = PopAsync();
    }

    void RemoveUnusedPages(IReadOnlyList<IView> stack)
    {
        foreach (var page in pageElements.Keys.ToArray())
        {
            if (stack.Contains(page))
                continue;
            pageElements.Remove(page);
            page.DisconnectHandlers();
        }
    }

    void UpdateBar(Page? top)
    {
        var hasTitle = !string.IsNullOrEmpty(top?.Title);
        var hasBar = top is not null
            && NavigationPage.GetHasNavigationBar(top)
            && (stackCount > 1 || hasTitle);
        bar.Style.Display = hasBar ? "flex" : "none";
        title.Text = top?.Title ?? string.Empty;
        backButton.Style["display"] = stackCount > 1 ? null : "none";
        ObservePage(top);
    }

    void ObservePage(Page? page)
    {
        if (ReferenceEquals(observedPage, page))
            return;
        if (observedPage is not null)
            observedPage.PropertyChanged -= OnPagePropertyChanged;
        observedPage = page;
        if (observedPage is not null)
            observedPage.PropertyChanged += OnPagePropertyChanged;
    }

    void OnPagePropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(Page.Title) && observedPage is not null)
            UpdateBar(observedPage);
    }

    Element GetPageElement(IView page)
    {
        if (!pageElements.TryGetValue(page, out var element))
        {
            var context = MauiContext
                ?? throw new InvalidOperationException("The navigation page is not connected to an Ooui context.");
            element = page.ToOouiElement(context);
            pageElements[page] = element;
        }
        return element;
    }

    void OnBackClicked(object? sender, TargetEventArgs args) => _ = PopAsync();

    async Task PopAsync()
    {
        if (VirtualView is NavigationPage navigationPage)
            await navigationPage.PopAsync().ConfigureAwait(false);
    }
}
