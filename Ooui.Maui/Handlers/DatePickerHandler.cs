using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class DatePickerHandler : OouiViewHandler<IDatePicker, Div>, IDatePickerHandler
{
    public static readonly IPropertyMapper<IDatePicker, DatePickerHandler> Mapper =
        new PropertyMapper<IDatePicker, DatePickerHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IDatePicker.Date)] = MapDate,
            [nameof(IDatePicker.Format)] = MapFormat,
            [nameof(IDatePicker.MinimumDate)] = MapRange,
            [nameof(IDatePicker.MaximumDate)] = MapRange,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
        };

    readonly Span display = new();

    Input input = null!;

    public DatePickerHandler() : base(Mapper) { }

    IDatePicker IDatePickerHandler.VirtualView => VirtualView;
    object IDatePickerHandler.PlatformView => PlatformView;

    protected override Div CreatePlatformView()
    {
        var wrapper = new Div();
        wrapper.Style["position"] = "relative";
        wrapper.Style["display"] = "inline-block";
        input = new Input(InputType.Date);
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
        var date = DateTime.TryParseExact(input.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : (DateTime?)null;
        bindable.SetValue(Microsoft.Maui.Controls.DatePicker.DateProperty, date);
    }

    public static void MapDate(DatePickerHandler handler, IDatePicker picker)
    {
        handler.input.Value = picker.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;
        handler.UpdateFormat();
    }

    public static void MapFormat(DatePickerHandler handler, IDatePicker picker) => handler.UpdateFormat();

    public static void MapRange(DatePickerHandler handler, IDatePicker picker)
    {
        handler.input.SetAttribute("min", picker.MinimumDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        handler.input.SetAttribute("max", picker.MaximumDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    public static void MapTextColor(DatePickerHandler handler, IDatePicker picker) =>
        handler.display.Style.Color = OouiStyle.ToCss(picker.TextColor);

    public static void MapFont(DatePickerHandler handler, IDatePicker picker) => OouiStyle.SetFont(handler.display, picker);

    public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker picker) =>
        OouiStyle.SetDimension(handler.display.Style, "letter-spacing", picker.CharacterSpacing);

    // The browser renders <input type="date"> in its own locale, so a custom Format is shown by the span.
    void UpdateFormat()
    {
        var format = VirtualView.Format;
        var custom = OouiStyle.HasCustomFormat(format, "d");
        OouiStyle.SetOverlay(display, input, custom);
        display.Text = custom && VirtualView.Date is { } date
            ? date.ToString(format, CultureInfo.CurrentCulture)
            : string.Empty;
    }
}
