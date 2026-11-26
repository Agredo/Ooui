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

    public static void SetTextColor(Element element, ITextStyle? text)
    {
        if (text is not null) element.Style.Color = ToCss(text.TextColor);
    }

    public static void SetFont(Element element, ITextStyle? text)
    {
        if (text is null) return;
        element.Style.FontFamily = text.Font.Family;
        element.Style.FontSize = text.Font.Size > 0 ? text.Font.Size : null;
        element.Style.FontWeight = text.Font.Weight switch
        {
            FontWeight.Regular => null,
            var weight => ((int)weight).ToString(System.Globalization.CultureInfo.InvariantCulture),
        };
        element.Style.FontStyle = text.Font.Slant != FontSlant.Default ? "italic" : null;
    }

    public static bool HasCustomFormat(string? format, string defaultFormat) =>
        !string.IsNullOrEmpty(format) && format != defaultFormat;

    // Native date and time inputs always use the browser locale. A custom Format therefore needs a
    // visible text of its own with the still working native input laid transparently on top of it.
    public static void SetOverlay(Span display, Input input, bool custom)
    {
        if (custom)
        {
            display.Style["display"] = "inline-block";
            display.Style["padding"] = "6px 10px";
            display.Style["border"] = "1px solid #b0b0b0";
            display.Style["border-radius"] = "4px";
            display.Style["background-color"] = "#ffffff";
            input.Style["position"] = "absolute";
            input.Style["inset"] = "0";
            input.Style["width"] = "100%";
            input.Style["height"] = "100%";
            input.Style["opacity"] = "0";
            input.Style["cursor"] = "pointer";
        }
        else
        {
            display.Style["display"] = "none";
            display.Style["padding"] = null;
            display.Style["border"] = null;
            display.Style["border-radius"] = null;
            display.Style["background-color"] = null;
            input.Style["position"] = null;
            input.Style["inset"] = null;
            input.Style["width"] = null;
            input.Style["height"] = null;
            input.Style["opacity"] = null;
            input.Style["cursor"] = null;
        }
    }
}
