using System.Collections.Specialized;
using System.ComponentModel;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

/// <summary>Renders a <see cref="TabbedPage"/> as a tab bar plus the current page.</summary>
public class TabbedPageHandler : OouiViewHandler<ITabbedView, Div>, ITabbedViewHandler
{
    public static readonly IPropertyMapper<ITabbedView, TabbedPageHandler> Mapper =
        new PropertyMapper<ITabbedView, TabbedPageHandler>(OouiViewMapper.Mapper)
        {
            [nameof(TabbedPage.BarBackgroundColor)] = MapBarBackgroundColor,
            [nameof(TabbedPage.BarTextColor)] = MapTabColors,
            [nameof(TabbedPage.SelectedTabColor)] = MapTabColors,
            [nameof(TabbedPage.UnselectedTabColor)] = MapTabColors,
        };

    const string DefaultAccent = "#2563eb";

    readonly Dictionary<IView, Element> pageElements = new();
    readonly List<(Button Button, Page Page)> tabs = new();

    Div tabBar = null!;
    Div content = null!;
    TabbedPage? observedPage;

    public TabbedPageHandler() : base(Mapper) { }

    ITabbedView ITabbedViewHandler.VirtualView => VirtualView;

    protected override Div CreatePlatformView()
    {
        tabBar = new Div();
        tabBar.Style.Display = "flex";
        tabBar.Style["gap"] = "0.25rem";
        tabBar.Style["padding"] = "0 0.75rem";
        tabBar.Style["border-bottom"] = "1px solid rgba(15, 23, 42, 0.12)";
        tabBar.Style["min-height"] = "2.75rem";
        tabBar.SetAttribute("role", "tablist");

        content = new Div();
        content.Style["flex-grow"] = "1";

        var root = new Div(tabBar, content);
        root.Style.Display = "flex";
        root.Style["flex-direction"] = "column";
        root.Style["min-height"] = "100%";
        return root;
    }

    protected override void ConnectHandler(Div platformView)
    {
        base.ConnectHandler(platformView);
        ObservePage(VirtualView as TabbedPage);
        MapBarBackgroundColor(this, VirtualView);
        Render();
    }

    protected override void DisconnectHandler(Div platformView)
    {
        ObservePage(null);
        ClearTabs();
        foreach (var page in pageElements.Keys.ToArray())
            page.DisconnectHandlers();
        pageElements.Clear();
        base.DisconnectHandler(platformView);
    }

    public static void MapBarBackgroundColor(TabbedPageHandler handler, ITabbedView view) =>
        handler.tabBar.Style.BackgroundColor = OouiStyle.ToCss((view as TabbedPage)?.BarBackgroundColor);

    public static void MapTabColors(TabbedPageHandler handler, ITabbedView view) => handler.RenderTabs();

    void ObservePage(TabbedPage? page)
    {
        if (ReferenceEquals(observedPage, page))
            return;
        if (observedPage is not null)
        {
            observedPage.PropertyChanged -= OnPagePropertyChanged;
            if (observedPage.Children is INotifyCollectionChanged notify)
                notify.CollectionChanged -= OnChildrenChanged;
        }
        observedPage = page;
        if (observedPage is not null)
        {
            observedPage.PropertyChanged += OnPagePropertyChanged;
            if (observedPage.Children is INotifyCollectionChanged notify)
                notify.CollectionChanged += OnChildrenChanged;
        }
    }

    void OnPagePropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == nameof(TabbedPage.CurrentPage))
            Render();
    }

    void OnChildrenChanged(object? sender, NotifyCollectionChangedEventArgs args) => Render();

    void Render()
    {
        RenderTabs();
        RenderContent();
    }

    void RenderContent()
    {
        if (VirtualView is not TabbedPage page)
            return;
        var current = page.CurrentPage;
        var element = current is null ? null : GetPageElement(current);
        foreach (var child in content.Children.ToArray())
        {
            if (!ReferenceEquals(child, element))
                content.RemoveChild(child);
        }
        if (element is not null && !content.Children.Contains(element))
            content.AppendChild(element);
        RemoveUnusedPages(page);
    }

    void RenderTabs()
    {
        ClearTabs();
        if (VirtualView is not TabbedPage page)
            return;
        var selected = page.SelectedTabColor is not null ? OouiStyle.ToCss(page.SelectedTabColor) : DefaultAccent;
        var unselected = OouiStyle.ToCss(page.UnselectedTabColor) ?? OouiStyle.ToCss(page.BarTextColor);
        foreach (var child in page.Children.ToArray())
        {
            var isCurrent = ReferenceEquals(child, page.CurrentPage);
            var button = new Button(child.Title ?? string.Empty) { Type = ButtonType.Button };
            button.Style["background-color"] = "transparent";
            button.Style["border"] = "none";
            button.Style["border-bottom"] = isCurrent ? $"2px solid {selected}" : "2px solid transparent";
            button.Style["border-radius"] = "0";
            button.Style["padding"] = "0.5rem 0.75rem";
            button.Style["color"] = isCurrent ? selected : unselected;
            button.Style.FontWeight = isCurrent ? "600" : null;
            button.SetAttribute("role", "tab");
            button.SetAttribute("aria-selected", isCurrent ? "true" : "false");
            button.Click += OnTabClicked;
            tabBar.AppendChild(button);
            tabs.Add((button, child));
        }
    }

    void ClearTabs()
    {
        foreach (var (button, _) in tabs)
        {
            button.Click -= OnTabClicked;
            tabBar.RemoveChild(button);
        }
        tabs.Clear();
    }

    void OnTabClicked(object? sender, TargetEventArgs args)
    {
        if (VirtualView is not TabbedPage page || sender is not Button button)
            return;
        foreach (var (tab, tabPage) in tabs)
        {
            if (ReferenceEquals(tab, button))
            {
                page.CurrentPage = tabPage;
                return;
            }
        }
    }

    void RemoveUnusedPages(TabbedPage page)
    {
        foreach (var cached in pageElements.Keys.ToArray())
        {
            if (page.Children.Contains(cached))
                continue;
            pageElements.Remove(cached);
            cached.DisconnectHandlers();
        }
    }

    Element GetPageElement(IView page)
    {
        if (!pageElements.TryGetValue(page, out var element))
        {
            var context = MauiContext
                ?? throw new InvalidOperationException("The tabbed page is not connected to an Ooui context.");
            element = page.ToOouiElement(context);
            pageElements[page] = element;
        }
        return element;
    }
}
