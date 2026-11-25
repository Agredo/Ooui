using Microsoft.Maui.Handlers;
using ILayout = Microsoft.Maui.ILayout;

namespace Ooui.Maui.Handlers;

public class LayoutHandler : OouiViewHandler<ILayout, Div>, ILayoutHandler
{
    public static readonly IPropertyMapper<ILayout, LayoutHandler> Mapper =
        new PropertyMapper<ILayout, LayoutHandler>(ViewMapper)
        {
            [nameof(IView.AutomationId)] = MapAutomationId,
            [nameof(IView.Background)] = MapBackground,
            [nameof(IView.Width)] = MapWidth,
            [nameof(IView.Height)] = MapHeight,
            [nameof(IView.MinimumWidth)] = MapMinimumWidth,
            [nameof(IView.MinimumHeight)] = MapMinimumHeight,
            [nameof(IView.MaximumWidth)] = MapMaximumWidth,
            [nameof(IView.MaximumHeight)] = MapMaximumHeight,
            [nameof(IView.IsEnabled)] = MapIsEnabled,
            [nameof(IView.Opacity)] = MapOpacity,
            [nameof(IView.Visibility)] = MapVisibility,
            [nameof(IView.InputTransparent)] = MapInputTransparent,
            [nameof(IToolTipElement.ToolTip)] = MapToolTip,
            [nameof(ILayout.ClipsToBounds)] = MapClipsToBounds,
        };

    public static readonly CommandMapper<ILayout, LayoutHandler> CommandMapper = new(ViewCommandMapper)
    {
        [nameof(ILayoutHandler.Add)] = (handler, _, args) => Add(handler, args),
        [nameof(ILayoutHandler.Remove)] = (handler, _, args) => Remove(handler, args),
        [nameof(ILayoutHandler.Clear)] = (handler, _, _) => handler.Clear(),
        [nameof(ILayoutHandler.Insert)] = (handler, _, args) => Insert(handler, args),
        [nameof(ILayoutHandler.Update)] = (handler, _, args) => Update(handler, args),
        [nameof(ILayoutHandler.UpdateZIndex)] = (handler, _, args) => UpdateZIndex(handler, args),
    };

    public LayoutHandler() : base(Mapper, CommandMapper) { }

    ILayout ILayoutHandler.VirtualView => VirtualView;
    object ILayoutHandler.PlatformView => PlatformView;

    protected override Div CreatePlatformView() => new();

    protected override void ConnectHandler(Div platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Style.Display = "flex";
        UpdateLayout();
        foreach (var child in VirtualView)
            Add(child);
    }

    public void Add(IView view) => Insert(PlatformView.Children.Count, view);

    public void Insert(int index, IView view)
    {
        if (MauiContext is null)
            throw new InvalidOperationException("The MAUI context is required to add a child.");
        var element = view.ToOouiElement(MauiContext);
        var reference = index < PlatformView.Children.Count ? PlatformView.Children[index] : null;
        PlatformView.InsertBefore(element, reference);
        UpdateZIndex(view);
    }

    public void Remove(IView view)
    {
        if (view.Handler is IElementHandler { PlatformView: Element element } && PlatformView.Children.Contains(element))
            PlatformView.RemoveChild(element);
    }

    public void Update(int index, IView view)
    {
        if (index >= 0 && index < PlatformView.Children.Count)
            PlatformView.RemoveChild(PlatformView.Children[index]);
        Insert(index, view);
    }

    public void Clear()
    {
        foreach (var child in PlatformView.Children.ToArray())
            PlatformView.RemoveChild(child);
    }

    public void UpdateZIndex(IView view)
    {
        if (view.Handler is IElementHandler { PlatformView: Element element })
            element.Style.ZIndex = view.ZIndex;
    }

    void UpdateLayout()
    {
        var style = PlatformView.Style;
        style["flex-direction"] = VirtualView is HorizontalStackLayout ? "row" : "column";
        var spacing = VirtualView is IStackLayout stackLayout ? stackLayout.Spacing : (double?)null;
        OouiStyle.SetDimension(style, "row-gap", spacing ?? double.NaN);
        OouiStyle.SetDimension(style, "column-gap", spacing ?? double.NaN);
        OouiStyle.SetThickness(style, VirtualView.Padding);
    }

    public static void MapClipsToBounds(LayoutHandler handler, ILayout layout) =>
        handler.PlatformView.Style.Overflow = layout.ClipsToBounds ? "hidden" : null;

    static void Add(LayoutHandler handler, object? args)
    {
        if (args is LayoutHandlerUpdate update) handler.Add(update.View);
    }

    static void Remove(LayoutHandler handler, object? args)
    {
        if (args is LayoutHandlerUpdate update) handler.Remove(update.View);
    }

    static void Insert(LayoutHandler handler, object? args)
    {
        if (args is LayoutHandlerUpdate update) handler.Insert(update.Index, update.View);
    }

    static void Update(LayoutHandler handler, object? args)
    {
        if (args is LayoutHandlerUpdate update) handler.Update(update.Index, update.View);
    }

    static void UpdateZIndex(LayoutHandler handler, object? args)
    {
        if (args is IView view) handler.UpdateZIndex(view);
    }
}
