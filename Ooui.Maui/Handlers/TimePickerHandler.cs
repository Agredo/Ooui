using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class TimePickerHandler : OouiViewHandler<ITimePicker, Div>, ITimePickerHandler
{
    public static readonly IPropertyMapper<ITimePicker, TimePickerHandler> Mapper =
        new PropertyMapper<ITimePicker, TimePickerHandler>(OouiViewMapper.Mapper)
        {
            [nameof(ITimePicker.Time)] = MapTime,
            [nameof(ITimePicker.Format)] = MapFormat,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
        };

    readonly Span display = new();

    Input input = null!;

    public TimePickerHandler() : base(Mapper) { }

    ITimePicker ITimePickerHandler.VirtualView => VirtualView;
    object ITimePickerHandler.PlatformView => PlatformView;

    protected override Div CreatePlatformView()
    {
        var wrapper = new Div();
        wrapper.Style["position"] = "relative";
        wrapper.Style["display"] = "inline-block";
        input = new Input(InputType.Time);
        wrapper.AppendChild(display);
        wrapper.AppendChild(input);
        return wrapper;
    }

    protected override void ConnectHandler(Div platformView)
    {
        base.ConnectHandler(platformView);
        input.Change += OnChange;
    }

    protected override void DisconnectHandler(Div platformView)
    {
        input.Change -= OnChange;
        base.DisconnectHandler(platformView);
    }

    void OnChange(object? sender, TargetEventArgs args)
    {
        if (VirtualView is not BindableObject bindable)
            return;
        var time = TimeSpan.TryParseExact(input.Value, @"hh\:mm", CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : (TimeSpan?)null;
        bindable.SetValue(Microsoft.Maui.Controls.TimePicker.TimeProperty, time);
    }

    public static void MapTime(TimePickerHandler handler, ITimePicker picker)
    {
        handler.input.Value = picker.Time?.ToString(@"hh\:mm", CultureInfo.InvariantCulture) ?? string.Empty;
        handler.UpdateFormat();
    }

    public static void MapFormat(TimePickerHandler handler, ITimePicker picker) => handler.UpdateFormat();

    public static void MapTextColor(TimePickerHandler handler, ITimePicker picker) =>
        handler.display.Style.Color = OouiStyle.ToCss(picker.TextColor);

    public static void MapFont(TimePickerHandler handler, ITimePicker picker) => OouiStyle.SetFont(handler.display, picker);

    public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker picker) =>
        OouiStyle.SetDimension(handler.display.Style, "letter-spacing", picker.CharacterSpacing);

    // The browser renders <input type="time"> in its own locale, so a custom Format is shown by the span.
    void UpdateFormat()
    {
        var format = VirtualView.Format;
        var custom = OouiStyle.HasCustomFormat(format, "t");
        OouiStyle.SetOverlay(display, input, custom);
        display.Text = custom && VirtualView.Time is { } time ? FormatTime(time, format) : string.Empty;
    }

    static string FormatTime(TimeSpan time, string format)
    {
        try
        {
            // MAUI documents TimeSpan formats, but "HH:mm" style DateTime formats are common and work as well.
            return DateTime.Today.Add(time).ToString(format, CultureInfo.CurrentCulture);
        }
        catch (FormatException)
        {
            return time.ToString(@"hh\:mm", CultureInfo.InvariantCulture);
        }
    }
}
