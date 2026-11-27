using System.ComponentModel;
using Microsoft.Maui.Handlers;
using ILayout = Microsoft.Maui.ILayout;

namespace Ooui.Maui.Handlers;

public class LayoutHandler : OouiViewHandler<ILayout, Div>, ILayoutHandler
{
    public static readonly IPropertyMapper<ILayout, LayoutHandler> Mapper =
        new PropertyMapper<ILayout, LayoutHandler>(OouiViewMapper.Mapper)
        {
            [nameof(ILayout.ClipsToBounds)] = MapClipsToBounds,
            [nameof(IPadding.Padding)] = MapPadding,
        };

    public static readonly CommandMapper<ILayout, LayoutHandler> CommandMapper = new(OouiViewMapper.CommandMapper)
    {
        [nameof(IView.InvalidateMeasure)] = (handler, _, _) => handler.UpdateLayout(),
        [nameof(ILayoutHandler.Add)] = (handler, _, args) => Add(handler, args),
        [nameof(ILayoutHandler.Remove)] = (handler, _, args) => Remove(handler, args),
        [nameof(ILayoutHandler.Clear)] = (handler, _, _) => handler.Clear(),
        [nameof(ILayoutHandler.Insert)] = (handler, _, args) => Insert(handler, args),
        [nameof(ILayoutHandler.Update)] = (handler, _, args) => Update(handler, args),
        [nameof(ILayoutHandler.UpdateZIndex)] = (handler, _, args) => UpdateZIndex(handler, args),
    };

    public LayoutHandler() : base(Mapper, CommandMapper) { }

    protected LayoutHandler(IPropertyMapper<ILayout, LayoutHandler> mapper) : base(mapper, CommandMapper) { }

    protected virtual void OnChildPropertyChanged(IView view, string propertyName)
    {
        if (propertyName is "HorizontalOptions" or "VerticalOptions"
            or nameof(IView.HorizontalLayoutAlignment) or nameof(IView.VerticalLayoutAlignment))
            UpdateChild(view);
    }

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

    public void Add(IView view)
    {
        Insert(PlatformView.Children.Count, view);
        UpdateLayout();
    }

    public void Insert(int index, IView view)
    {
        if (MauiContext is null)
            throw new InvalidOperationException("The MAUI context is required to add a child.");
        var element = view.ToOouiElement(MauiContext);
        var reference = index < PlatformView.Children.Count ? PlatformView.Children[index] : null;
        PlatformView.InsertBefore(element, reference);
        UpdateZIndex(view);
        UpdateChild(view);
        if (view is INotifyPropertyChanged notifyingChanged)
        {
            void ChildChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs args) => OnChildPropertyChanged(view, args.PropertyName ?? "");
            notifyingChanged.PropertyChanged += ChildChanged;
            childPropertyHandlers[view] = ChildChanged;
        }
    }

    public void Remove(IView view)
    {
        if (childPropertyHandlers.Remove(view, out var handler) && view is INotifyPropertyChanged notifying)
            notifying.PropertyChanged -= handler;
        if (view.Handler is IElementHandler { PlatformView: Element element } && PlatformView.Children.Contains(element))
            PlatformView.RemoveChild(element);
        UpdateLayout();
    }

    public void Update(int index, IView view)
    {
        if (index >= 0 && index < PlatformView.Children.Count)
            PlatformView.RemoveChild(PlatformView.Children[index]);
        Insert(index, view);
        UpdateLayout();
    }

    public void Clear()
    {
        foreach (var view in childPropertyHandlers.Keys.ToArray()) Remove(view);
        foreach (var child in PlatformView.Children.ToArray())
            PlatformView.RemoveChild(child);
        UpdateLayout();
    }

    public void UpdateZIndex(IView view)
    {
        if (view.Handler is IElementHandler { PlatformView: Element element })
            element.Style.ZIndex = view.ZIndex;
    }

    protected virtual void UpdateChild(IView view)
    {
        if (VirtualView is not ILayout layout || view.Handler is not IElementHandler { PlatformView: Element element }) return;
        var crossAxis = layout is HorizontalStackLayout ? view.VerticalLayoutAlignment : view.HorizontalLayoutAlignment;
        element.Style["align-self"] = OouiStyle.ToCss(crossAxis);
    }

    readonly Dictionary<IView, System.ComponentModel.PropertyChangedEventHandler> childPropertyHandlers = new();

    protected override void DisconnectHandler(Div platformView)
    {
        foreach (var pair in childPropertyHandlers)
            if (pair.Key is INotifyPropertyChanged notifying) notifying.PropertyChanged -= pair.Value;
        childPropertyHandlers.Clear();
        base.DisconnectHandler(platformView);
    }

    protected virtual void UpdateLayout()
    {
        var style = PlatformView.Style;
        style["flex-direction"] = VirtualView is HorizontalStackLayout ? "row" : "column";
        var spacing = VirtualView is IStackLayout stackLayout ? stackLayout.Spacing : (double?)null;
        OouiStyle.SetDimension(style, "row-gap", spacing ?? double.NaN);
        OouiStyle.SetDimension(style, "column-gap", spacing ?? double.NaN);
        OouiStyle.SetThickness(style, VirtualView.Padding);
        foreach (var child in VirtualView) UpdateChild(child);
    }

    public static void MapPadding(LayoutHandler handler, ILayout layout) => handler.UpdateLayout();

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
