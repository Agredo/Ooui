using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class CheckBoxHandler : OouiViewHandler<ICheckBox, Input>, ICheckBoxHandler
{
    public static readonly IPropertyMapper<ICheckBox, CheckBoxHandler> Mapper =
        new PropertyMapper<ICheckBox, CheckBoxHandler>(OouiViewMapper.Mapper)
        {
            [nameof(ICheckBox.IsChecked)] = MapIsChecked,
            [nameof(ICheckBox.Foreground)] = MapForeground,
        };

    public CheckBoxHandler() : base(Mapper) { }

    ICheckBox ICheckBoxHandler.VirtualView => VirtualView;
    object ICheckBoxHandler.PlatformView => PlatformView;

    protected override Input CreatePlatformView() => new(InputType.Checkbox);

    protected override void ConnectHandler(Input platformView)
    {
        base.ConnectHandler(platformView);
        MapIsChecked(this, VirtualView);
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
            bindable.SetValue(Microsoft.Maui.Controls.CheckBox.IsCheckedProperty, PlatformView.IsChecked);
    }

    public static void MapIsChecked(CheckBoxHandler handler, ICheckBox checkBox)
    {
        if (handler.PlatformView.IsChecked != checkBox.IsChecked)
            handler.PlatformView.IsChecked = checkBox.IsChecked;
    }

    public static void MapForeground(CheckBoxHandler handler, ICheckBox checkBox) =>
        handler.PlatformView.Style["accent-color"] = checkBox.Foreground is SolidPaint paint ? OouiStyle.ToCss(paint.Color) : null;
}
