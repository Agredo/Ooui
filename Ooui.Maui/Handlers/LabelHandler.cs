using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class LabelHandler : OouiViewHandler<ILabel, Span>, ILabelHandler
{
    public static readonly IPropertyMapper<ILabel, LabelHandler> Mapper =
        new PropertyMapper<ILabel, LabelHandler>(OouiViewMapper.Mapper)
        {
            [nameof(ILabel.Text)] = MapText,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(ILabel.LineHeight)] = MapLineHeight,
            [nameof(ILabel.Padding)] = MapPadding,
            [nameof(ILabel.TextDecorations)] = MapTextDecorations,
        };

    public LabelHandler() : base(Mapper) { }

    ILabel ILabelHandler.VirtualView => VirtualView;
    object ILabelHandler.PlatformView => PlatformView;

    protected override Span CreatePlatformView() => new() { Style = { Display = "block" } };

    public static void MapText(LabelHandler handler, ILabel label) => handler.PlatformView.Text = label.Text;

    public static void MapTextColor(LabelHandler handler, ILabel label) =>
        handler.PlatformView.Style.Color = OouiStyle.ToCss(label.TextColor);

    public static void MapFont(LabelHandler handler, ILabel label) => OouiStyle.SetFont(handler.PlatformView, label);

    public static void MapCharacterSpacing(LabelHandler handler, ILabel label) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "letter-spacing", label.CharacterSpacing);

    public static void MapHorizontalTextAlignment(LabelHandler handler, ILabel label) =>
        handler.PlatformView.Style.TextAlign = label.HorizontalTextAlignment.ToString().ToLowerInvariant();

    public static void MapLineHeight(LabelHandler handler, ILabel label) =>
        handler.PlatformView.Style.LineHeight = label.LineHeight >= 0 ? label.LineHeight : null;

    public static void MapPadding(LabelHandler handler, ILabel label) => OouiStyle.SetThickness(handler.PlatformView.Style, label.Padding);

    public static void MapTextDecorations(LabelHandler handler, ILabel label) =>
        handler.PlatformView.Style.TextDecoration = label.TextDecorations switch
        {
            Microsoft.Maui.TextDecorations.Underline => "underline",
            Microsoft.Maui.TextDecorations.Strikethrough => "line-through",
            _ => null,
        };
}
