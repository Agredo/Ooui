using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class DatePickerHandler : OouiViewHandler<IDatePicker, Input>, IDatePickerHandler
{
    public static readonly IPropertyMapper<IDatePicker, DatePickerHandler> Mapper =
        new PropertyMapper<IDatePicker, DatePickerHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IDatePicker.Date)] = MapDate,
            [nameof(IDatePicker.MinimumDate)] = MapRange,
            [nameof(IDatePicker.MaximumDate)] = MapRange,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
        };

    public DatePickerHandler() : base(Mapper) { }

    IDatePicker IDatePickerHandler.VirtualView => VirtualView;
    object IDatePickerHandler.PlatformView => PlatformView;

    protected override Input CreatePlatformView() => new(InputType.Date);

    protected override void ConnectHandler(Input platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Change += OnChange;
    }

    protected override void DisconnectHandler(Input platformView)
    {
        platformView.Change -= OnChange;
        base.DisconnectHandler(platformView);
    }

    void OnChange(object? sender, TargetEventArgs args)
    {
        if (VirtualView is not BindableObject bindable)
            return;
        var date = DateTime.TryParseExact(PlatformView.Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : (DateTime?)null;
        bindable.SetValue(Microsoft.Maui.Controls.DatePicker.DateProperty, date);
    }

    public static void MapDate(DatePickerHandler handler, IDatePicker picker) =>
        handler.PlatformView.Value = picker.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? string.Empty;

    public static void MapRange(DatePickerHandler handler, IDatePicker picker)
    {
        handler.PlatformView.SetAttribute("min", picker.MinimumDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        handler.PlatformView.SetAttribute("max", picker.MaximumDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
    }

    public static void MapFont(DatePickerHandler handler, IDatePicker picker) => OouiStyle.SetFont(handler.PlatformView, picker);

    public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker picker) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "letter-spacing", picker.CharacterSpacing);
}
