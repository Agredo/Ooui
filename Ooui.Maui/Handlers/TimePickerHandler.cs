using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class TimePickerHandler : OouiViewHandler<ITimePicker, Input>, ITimePickerHandler
{
    public static readonly IPropertyMapper<ITimePicker, TimePickerHandler> Mapper =
        new PropertyMapper<ITimePicker, TimePickerHandler>(OouiViewMapper.Mapper)
        {
            [nameof(ITimePicker.Time)] = MapTime,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
        };

    public TimePickerHandler() : base(Mapper) { }

    ITimePicker ITimePickerHandler.VirtualView => VirtualView;
    object ITimePickerHandler.PlatformView => PlatformView;

    protected override Input CreatePlatformView() => new(InputType.Time);

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
        var time = TimeSpan.TryParseExact(PlatformView.Value, @"hh\:mm", CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : (TimeSpan?)null;
        bindable.SetValue(Microsoft.Maui.Controls.TimePicker.TimeProperty, time);
    }

    public static void MapTime(TimePickerHandler handler, ITimePicker picker) =>
        handler.PlatformView.Value = picker.Time?.ToString(@"hh\:mm", CultureInfo.InvariantCulture) ?? string.Empty;

    public static void MapFont(TimePickerHandler handler, ITimePicker picker) => OouiStyle.SetFont(handler.PlatformView, picker);

    public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker picker) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "letter-spacing", picker.CharacterSpacing);
}
