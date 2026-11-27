using Microsoft.Maui.Graphics;

namespace Ooui.Maui;

internal static class OouiStyle
{
    public static string? ToCss(Microsoft.Maui.Graphics.Color? color) => color?.ToRgbaHex(includeAlpha: true);

    /// <summary>A stroke can only carry one color in CSS; a gradient contributes its start color.</summary>
    public static string? ToCss(Paint? paint) => paint switch
    {
        SolidPaint solid => ToCss(solid.Color),
        GradientPaint gradient => ToCss(gradient.StartColor ?? gradient.GradientStops?.FirstOrDefault()?.Color),
        _ => null,
    };

    /// <summary>
    /// Turns a MAUI paint into the CSS that paints an element's background. A solid paint becomes a
    /// background color; gradients become background images. Unknown paints clear both, so switching
    /// from a gradient to a solid color (or back) never leaves the previous value behind.
    /// </summary>
    public static void SetBackground(Style style, Paint? paint)
    {
        switch (paint)
        {
            case SolidPaint solid:
                style.BackgroundColor = ToCss(solid.Color);
                style["background-image"] = null;
                break;
            case GradientPaint gradient:
                style.BackgroundColor = null;
                style["background-image"] = ToCssGradient(gradient);
                break;
            default:
                style.BackgroundColor = null;
                style["background-image"] = null;
                break;
        }
    }

    static string? ToCssGradient(GradientPaint paint)
    {
        var stops = paint.GradientStops;
        if (stops is null || stops.Length == 0) return null;
        var cssStops = string.Join(", ", stops
            .Where(stop => stop is not null && stop.Color is not null)
            .OrderBy(stop => stop.Offset)
            .Select(stop => FormattableString.Invariant($"{ToCss(stop.Color)} {stop.Offset * 100:0.###}%")));
        if (cssStops.Length == 0) return null;
        return paint switch
        {
            // MAUI gradient points are fractions of the element box; CSS expects percentages.
            LinearGradientPaint linear => FormattableString.Invariant(
                $"linear-gradient({GradientAngle(linear.StartPoint, linear.EndPoint):0.###}deg, {cssStops})"),
            RadialGradientPaint radial => FormattableString.Invariant(
                $"radial-gradient(circle at {radial.Center.X * 100:0.###}% {radial.Center.Y * 100:0.###}%, {cssStops})"),
            _ => null,
        };
    }

    static double GradientAngle(Point start, Point end)
    {
        var dx = end.X - start.X;
        var dy = end.Y - start.Y;
        if (dx == 0 && dy == 0) return 180; // CSS default: top to bottom.
        return (Math.Atan2(dx, -dy) * 180 / Math.PI + 360) % 360;
    }

    /// <summary>Maps MAUI's radius/offset/opacity shadow onto a CSS box-shadow.</summary>
    public static void SetShadow(Style style, IShadow? shadow)
    {
        if (shadow is null || shadow.Paint is null)
        {
            style["box-shadow"] = null;
            return;
        }
        var color = ToCss(shadow.Paint);
        if (color is null)
        {
            style["box-shadow"] = null;
            return;
        }
        var opacity = Math.Clamp(shadow.Opacity, 0f, 1f);
        style["box-shadow"] = FormattableString.Invariant(
            $"{shadow.Offset.X:0.###}px {shadow.Offset.Y:0.###}px {Math.Max(0, shadow.Radius):0.###}px {WithOpacity(color, opacity)}");
    }

    static string WithOpacity(string color, float opacity)
    {
        var r = Convert.ToInt32(color.Substring(1, 2), 16);
        var g = Convert.ToInt32(color.Substring(3, 2), 16);
        var b = Convert.ToInt32(color.Substring(5, 2), 16);
        var a = color.Length >= 9 ? Convert.ToInt32(color.Substring(7, 2), 16) / 255f : 1f;
        return FormattableString.Invariant($"rgba({r}, {g}, {b}, {a * opacity:0.###})");
    }

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

    public static string ToCss(Microsoft.Maui.Primitives.LayoutAlignment alignment) => alignment switch
    {
        Microsoft.Maui.Primitives.LayoutAlignment.Start => "start",
        Microsoft.Maui.Primitives.LayoutAlignment.Center => "center",
        Microsoft.Maui.Primitives.LayoutAlignment.End => "end",
        _ => "stretch",
    };

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
