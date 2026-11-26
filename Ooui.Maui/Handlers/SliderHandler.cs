using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class SliderHandler : OouiViewHandler<ISlider, Input>, ISliderHandler
{
    public static readonly IPropertyMapper<ISlider, SliderHandler> Mapper =
        new PropertyMapper<ISlider, SliderHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IRange.Minimum)] = MapRange,
            [nameof(IRange.Maximum)] = MapRange,
            [nameof(IRange.Value)] = MapValue,
            [nameof(ISlider.MinimumTrackColor)] = MapColors,
            [nameof(ISlider.MaximumTrackColor)] = MapColors,
            [nameof(ISlider.ThumbColor)] = MapColors,
        };

    public SliderHandler() : base(Mapper) { }

    ISlider ISliderHandler.VirtualView => VirtualView;
    object ISliderHandler.PlatformView => PlatformView;

    protected override Input CreatePlatformView() => new(InputType.Range);

    protected override void ConnectHandler(Input platformView)
    {
        base.ConnectHandler(platformView);
        platformView.AddEventListener("input", OnInput);
        platformView.Change += OnChange;
    }

    protected override void DisconnectHandler(Input platformView)
    {
        platformView.RemoveEventListener("input", OnInput);
        platformView.Change -= OnChange;
        base.DisconnectHandler(platformView);
    }

    void OnInput(object? sender, TargetEventArgs args)
    {
        UpdateValue();
        VirtualView.DragStarted();
    }

    void OnChange(object? sender, TargetEventArgs args)
    {
        UpdateValue();
        VirtualView.DragCompleted();
    }

    void UpdateValue()
    {
        if (VirtualView is BindableObject bindable &&
            double.TryParse(PlatformView.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            bindable.SetValue(Microsoft.Maui.Controls.Slider.ValueProperty, value);
    }

    public static void MapRange(SliderHandler handler, ISlider slider)
    {
        handler.PlatformView.Minimum = slider.Minimum;
        handler.PlatformView.Maximum = slider.Maximum;
        // A range input with the default step of 1 snaps every value to whole numbers.
        handler.PlatformView.Step = 0.01;
    }

    public static void MapValue(SliderHandler handler, ISlider slider) =>
        handler.PlatformView.Value = slider.Value.ToString(CultureInfo.InvariantCulture);

    public static void MapColors(SliderHandler handler, ISlider slider) =>
        handler.PlatformView.Style["accent-color"] = OouiStyle.ToCss(slider.ThumbColor ?? slider.MinimumTrackColor);
}
