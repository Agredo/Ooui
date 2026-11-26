using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class StepperHandler : OouiViewHandler<IStepper, Input>, IStepperHandler
{
    public static readonly IPropertyMapper<IStepper, StepperHandler> Mapper =
        new PropertyMapper<IStepper, StepperHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IRange.Minimum)] = MapRange,
            [nameof(IRange.Maximum)] = MapRange,
            [nameof(IStepper.Interval)] = MapRange,
            [nameof(IRange.Value)] = MapValue,
        };

    public StepperHandler() : base(Mapper) { }

    IStepper IStepperHandler.VirtualView => VirtualView;
    object IStepperHandler.PlatformView => PlatformView;

    protected override Input CreatePlatformView() => new(InputType.Number);

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

    void OnInput(object? sender, TargetEventArgs args) => UpdateValue();
    void OnChange(object? sender, TargetEventArgs args) => UpdateValue();

    void UpdateValue()
    {
        if (VirtualView is BindableObject bindable &&
            double.TryParse(PlatformView.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            bindable.SetValue(Microsoft.Maui.Controls.Stepper.ValueProperty, value);
    }

    public static void MapRange(StepperHandler handler, IStepper stepper)
    {
        handler.PlatformView.Minimum = stepper.Minimum;
        handler.PlatformView.Maximum = stepper.Maximum;
        handler.PlatformView.Step = stepper.Interval;
    }

    public static void MapValue(StepperHandler handler, IStepper stepper) =>
        handler.PlatformView.Value = stepper.Value.ToString(CultureInfo.InvariantCulture);
}
