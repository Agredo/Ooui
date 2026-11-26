using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public static class OouiViewMapper
{
    static Element Element(IViewHandler handler) => (Element)handler.PlatformView!;

    public static readonly CommandMapper<IView, IViewHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
    {
        [nameof(IView.Focus)] = (handler, _, args) =>
        {
            Element(handler).Focus();
            if (args is RetrievePlatformValueRequest<bool> request) request.SetResult(true);
        },
    };

    public static readonly IPropertyMapper<IView, IViewHandler> Mapper =
        new PropertyMapper<IView, IViewHandler>(ViewHandler.ViewMapper)
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
            [nameof(IView.TranslationX)] = MapTransform,
            [nameof(IView.TranslationY)] = MapTransform,
            [nameof(IView.Scale)] = MapTransform,
            [nameof(IView.ScaleX)] = MapTransform,
            [nameof(IView.ScaleY)] = MapTransform,
            [nameof(IView.Rotation)] = MapTransform,
            [nameof(IView.RotationX)] = MapTransform,
            [nameof(IView.RotationY)] = MapTransform,
        };

    public static void MapAutomationId(IViewHandler handler, IView view)
    {
        if (string.IsNullOrEmpty(view.AutomationId)) Element(handler).RemoveAttribute("data-automation-id");
        else Element(handler).SetAttribute("data-automation-id", view.AutomationId);
    }

    public static void MapBackground(IViewHandler handler, IView view) =>
        Element(handler).Style.BackgroundColor = view.Background is SolidPaint solidPaint ? OouiStyle.ToCss(solidPaint.Color) : null;

    public static void MapWidth(IViewHandler handler, IView view) => OouiStyle.SetDimension(Element(handler).Style, "width", view.Width);
    public static void MapHeight(IViewHandler handler, IView view) => OouiStyle.SetDimension(Element(handler).Style, "height", view.Height);
    public static void MapMinimumWidth(IViewHandler handler, IView view) => OouiStyle.SetDimension(Element(handler).Style, "min-width", view.MinimumWidth);
    public static void MapMinimumHeight(IViewHandler handler, IView view) => OouiStyle.SetDimension(Element(handler).Style, "min-height", view.MinimumHeight);
    public static void MapMaximumWidth(IViewHandler handler, IView view) => OouiStyle.SetDimension(Element(handler).Style, "max-width", view.MaximumWidth);
    public static void MapMaximumHeight(IViewHandler handler, IView view) => OouiStyle.SetDimension(Element(handler).Style, "max-height", view.MaximumHeight);

    public static void MapIsEnabled(IViewHandler handler, IView view)
    {
        if (view.IsEnabled) Element(handler).RemoveAttribute("disabled");
        else Element(handler).SetAttribute("disabled", string.Empty);
    }

    public static void MapOpacity(IViewHandler handler, IView view) => Element(handler).Style.Opacity = view.Opacity;
    public static void MapVisibility(IViewHandler handler, IView view) => Element(handler).Style.Display = view.Visibility == Visibility.Visible ? null : "none";
    public static void MapInputTransparent(IViewHandler handler, IView view) => Element(handler).Style["pointer-events"] = view.InputTransparent ? "none" : null;
    public static void MapToolTip(IViewHandler handler, IView view) => Element(handler).Title = (view as IToolTipElement)?.ToolTip?.ToString() ?? string.Empty;

    public static void MapTransform(IViewHandler handler, IView view)
    {
        var transforms = new List<string>();
        if (view.TranslationX != 0 || view.TranslationY != 0)
            transforms.Add(FormattableString.Invariant($"translate({view.TranslationX}px, {view.TranslationY}px)"));
        if (view.Rotation != 0) transforms.Add(FormattableString.Invariant($"rotate({view.Rotation}deg)"));
        if (view.RotationX != 0) transforms.Add(FormattableString.Invariant($"rotateX({view.RotationX}deg)"));
        if (view.RotationY != 0) transforms.Add(FormattableString.Invariant($"rotateY({view.RotationY}deg)"));
        var scale = view.Scale * view.ScaleX;
        var scaleY = view.Scale * view.ScaleY;
        if (scale != 1 || scaleY != 1) transforms.Add(FormattableString.Invariant($"scale({scale}, {scaleY})"));
        Element(handler).Style.Transform = transforms.Count == 0 ? null : string.Join(" ", transforms);
    }
}
