using Microsoft.Maui.Graphics;

namespace Ooui.Maui;

internal static class OouiStyle
{
    public static string? ToCss(Microsoft.Maui.Graphics.Color? color) => color?.ToRgbaHex(includeAlpha: true);

    public static void SetDimension(Style style, string property, double value)
    {
        style[property] = double.IsNaN(value) || double.IsInfinity(value)
            ? null
            : value.ToString(System.Globalization.CultureInfo.InvariantCulture) + "px";
    }

    public static void SetThickness(Style style, Thickness thickness, bool padding = true)
    {
        var prefix = padding ? "padding" : "margin";
        SetDimension(style, $"{prefix}-top", thickness.Top);
        SetDimension(style, $"{prefix}-right", thickness.Right);
        SetDimension(style, $"{prefix}-bottom", thickness.Bottom);
        SetDimension(style, $"{prefix}-left", thickness.Left);
    }

    public static void SetFont(Element element, ITextStyle text)
    {
        element.Style.FontFamily = text.Font.Family;
        element.Style.FontSize = text.Font.Size > 0 ? text.Font.Size : null;
        element.Style.FontWeight = text.Font.Weight switch
        {
            FontWeight.Regular => null,
            var weight => ((int)weight).ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        element.Style.FontStyle = text.Font.Slant != FontSlant.Default ? "italic" : null;
    }
}
