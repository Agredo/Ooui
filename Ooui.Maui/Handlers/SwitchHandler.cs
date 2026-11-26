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
            [nameof(Microsoft.Maui.Controls.Switch.OnColor)] = MapColors,
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

    // accent-color tints the whole switch, so the colors are merged into one value.
    // OnColor lives on the control, not on ISwitch.
    public static void MapColors(SwitchHandler handler, ISwitch switchView)
    {
        var onColor = (switchView as Microsoft.Maui.Controls.Switch)?.OnColor;
        handler.PlatformView.Style["accent-color"] =
            OouiStyle.ToCss(onColor ?? switchView.TrackColor ?? switchView.ThumbColor);
    }
}
