using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class BoxViewHandler : OouiViewHandler<IShapeView, Div>, IShapeViewHandler
{
    public static readonly IPropertyMapper<IShapeView, BoxViewHandler> Mapper =
        new PropertyMapper<IShapeView, BoxViewHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IShapeView.Shape)] = MapShape,
            [nameof(IShapeView.Aspect)] = MapShape,
            [nameof(IShapeView.Fill)] = MapFill,
        };

    public BoxViewHandler() : base(Mapper) { }

    IShapeView IShapeViewHandler.VirtualView => VirtualView;
    object IShapeViewHandler.PlatformView => PlatformView;

    protected override Div CreatePlatformView() => new();

    public static void MapShape(BoxViewHandler handler, IShapeView view)
    {
        if (view is Microsoft.Maui.Controls.BoxView box)
        {
            var radius = box.CornerRadius;
            handler.PlatformView.Style.BorderRadius = FormattableString.Invariant($"{radius.TopLeft}px {radius.TopRight}px {radius.BottomRight}px {radius.BottomLeft}px");
        }
        else
        {
            handler.PlatformView.Style.BorderRadius = null;
        }
    }

    public static void MapFill(BoxViewHandler handler, IShapeView view) =>
        handler.PlatformView.Style.BackgroundColor = view.Fill is Microsoft.Maui.Graphics.SolidPaint paint ? OouiStyle.ToCss(paint.Color) : null;
}
