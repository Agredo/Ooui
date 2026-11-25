using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public abstract class OouiViewHandler<TVirtualView, TPlatformView> : ViewHandler<TVirtualView, TPlatformView>
    where TVirtualView : class, IView
    where TPlatformView : Element
{
    protected OouiViewHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper ?? ViewCommandMapper)
    {
    }

    public override bool NeedsContainer => false;

    public new TPlatformView PlatformView => base.PlatformView;

    protected override void SetupContainer() => SetContainerView(null);

    protected override void RemoveContainer() => SetContainerView(null);

    public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        var width = VirtualView.Width;
        var height = VirtualView.Height;
        return new Size(
            double.IsNaN(width) ? Math.Min(widthConstraint, 100) : width,
            double.IsNaN(height) ? Math.Min(heightConstraint, 44) : height);
    }

    public override void PlatformArrange(Rect frame)
    {
        OouiStyle.SetDimension(PlatformView.Style, "width", frame.Width);
        OouiStyle.SetDimension(PlatformView.Style, "height", frame.Height);
    }

    protected override void ConnectHandler(TPlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Style["box-sizing"] = "border-box";
    }

    public static void MapAutomationId(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        handler.PlatformView.SetAttribute("data-automation-id", view.AutomationId);

    public static void MapBackground(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        handler.PlatformView.Style.BackgroundColor = view.Background is SolidPaint solidPaint
            ? OouiStyle.ToCss(solidPaint.Color)
            : null;

    public static void MapWidth(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "width", view.Width);

    public static void MapHeight(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "height", view.Height);

    public static void MapMinimumWidth(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "min-width", view.MinimumWidth);

    public static void MapMinimumHeight(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "min-height", view.MinimumHeight);

    public static void MapMaximumWidth(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "max-width", view.MaximumWidth);

    public static void MapMaximumHeight(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "max-height", view.MaximumHeight);

    public static void MapIsEnabled(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view)
    {
        if (view.IsEnabled)
            handler.PlatformView.RemoveAttribute("disabled");
        else
            handler.PlatformView.SetAttribute("disabled", string.Empty);
    }

    public static void MapOpacity(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        handler.PlatformView.Style.Opacity = view.Opacity;

    public static void MapVisibility(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        handler.PlatformView.Style.Display = view.Visibility == Visibility.Visible ? null : "none";

    public static void MapInputTransparent(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        handler.PlatformView.Style["pointer-events"] = view.InputTransparent ? "none" : null;

    public static void MapToolTip(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view) =>
        handler.PlatformView.Title = (view as IToolTipElement)?.ToolTip?.ToString() ?? string.Empty;

    public static void MapTransform(OouiViewHandler<TVirtualView, TPlatformView> handler, IView view)
    {
        var transforms = new List<string>();
        if (view.TranslationX != 0 || view.TranslationY != 0)
            transforms.Add(FormattableString.Invariant($"translate({view.TranslationX}px, {view.TranslationY}px)"));
        if (view.Rotation != 0)
            transforms.Add(FormattableString.Invariant($"rotate({view.Rotation}deg)"));
        if (view.RotationX != 0)
            transforms.Add(FormattableString.Invariant($"rotateX({view.RotationX}deg)"));
        if (view.RotationY != 0)
            transforms.Add(FormattableString.Invariant($"rotateY({view.RotationY}deg)"));
        var scale = view.Scale * view.ScaleX;
        var scaleY = view.Scale * view.ScaleY;
        if (scale != 1 || scaleY != 1)
            transforms.Add(FormattableString.Invariant($"scale({scale}, {scaleY})"));
        handler.PlatformView.Style.Transform = transforms.Count == 0 ? null : string.Join(" ", transforms);
    }
}
