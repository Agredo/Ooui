using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class SwitchHandler : OouiViewHandler<ISwitch, Input>, ISwitchHandler
{
    public static readonly IPropertyMapper<ISwitch, SwitchHandler> Mapper =
        new PropertyMapper<ISwitch, SwitchHandler>(OouiViewMapper.Mapper)
        {
            [nameof(ISwitch.IsOn)] = MapIsOn,
            [nameof(ISwitch.TrackColor)] = MapColors,
            [nameof(ISwitch.ThumbColor)] = MapColors,
        };

    public SwitchHandler() : base(Mapper) { }

    ISwitch ISwitchHandler.VirtualView => VirtualView;
    object ISwitchHandler.PlatformView => PlatformView;

    protected override Input CreatePlatformView() => new(InputType.Checkbox) { ClassName = "ooui-switch" };

    protected override void ConnectHandler(Input platformView)
    {
        base.ConnectHandler(platformView);
        platformView.IsChecked = VirtualView.IsOn;
        platformView.Change += OnChange;
    }

    protected override void DisconnectHandler(Input platformView)
    {
        platformView.Change -= OnChange;
        base.DisconnectHandler(platformView);
    }

    void OnChange(object? sender, TargetEventArgs args)
    {
        if (VirtualView is BindableObject bindable)
            bindable.SetValue(Microsoft.Maui.Controls.Switch.IsToggledProperty, PlatformView.IsChecked);
    }

    public static void MapIsOn(SwitchHandler handler, ISwitch switchView)
    {
        if (handler.PlatformView.IsChecked != switchView.IsOn)
            handler.PlatformView.IsChecked = switchView.IsOn;
    }

    public static void MapColors(SwitchHandler handler, ISwitch switchView) =>
        handler.PlatformView.Style["accent-color"] = OouiStyle.ToCss(switchView.TrackColor);
}
