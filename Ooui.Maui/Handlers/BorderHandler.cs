using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Layouts;

namespace Ooui.Maui.Handlers;

public class BorderHandler : ContentViewHandler, IBorderHandler
{
    public new static readonly IPropertyMapper<IContentView, BorderHandler> Mapper =
        new PropertyMapper<IContentView, BorderHandler>(ContentViewHandler.Mapper)
        {
            [nameof(IBorderView.Padding)] = MapPadding,
            [nameof(IStroke.Stroke)] = MapStroke,
            [nameof(IStroke.StrokeThickness)] = MapStroke,
            [nameof(IBorderStroke.Shape)] = MapStroke,
        };

    public BorderHandler() : base(Mapper) { }

    IBorderView IBorderHandler.VirtualView => (IBorderView)VirtualView;
    object IBorderHandler.PlatformView => PlatformView;

    public static void MapPadding(BorderHandler handler, IContentView content) =>
        OouiStyle.SetThickness(handler.PlatformView.Style, content.Padding);

    public static void MapStroke(BorderHandler handler, IContentView content)
    {
        var border = (IBorderView)content;
        handler.PlatformView.Style.BorderWidth = border.StrokeThickness;
        handler.PlatformView.Style.BorderStyle = border.StrokeThickness > 0 ? "solid" : null;
        handler.PlatformView.Style.BorderColor = border.Stroke is SolidPaint paint ? OouiStyle.ToCss(paint.Color) : null;
        handler.PlatformView.Style.BorderRadius = border.Shape switch
        {
            Microsoft.Maui.Controls.Shapes.RoundRectangle rounded => FormattableString.Invariant($"{rounded.CornerRadius.TopLeft}px {rounded.CornerRadius.TopRight}px {rounded.CornerRadius.BottomRight}px {rounded.CornerRadius.BottomLeft}px"),
            Microsoft.Maui.Controls.Shapes.Rectangle rectangle => FormattableString.Invariant($"{rectangle.RadiusX}px {rectangle.RadiusY}px"),
            _ => null,
        };
    }
}
