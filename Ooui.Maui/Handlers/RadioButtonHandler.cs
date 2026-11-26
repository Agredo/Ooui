using System.ComponentModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using MauiRadioButton = Microsoft.Maui.Controls.RadioButton;

namespace Ooui.Maui.Handlers;

/// <summary>Renders a <see cref="MauiRadioButton"/> as a native radio input with a text label.</summary>
public class RadioButtonHandler : OouiViewHandler<MauiRadioButton, Label>
{
    public static readonly IPropertyMapper<MauiRadioButton, RadioButtonHandler> Mapper =
        new PropertyMapper<MauiRadioButton, RadioButtonHandler>(OouiViewMapper.Mapper)
        {
            [nameof(MauiRadioButton.IsChecked)] = MapIsChecked,
            [nameof(MauiRadioButton.GroupName)] = MapGroupName,
            [nameof(MauiRadioButton.Content)] = MapContent,
            [nameof(MauiRadioButton.TextColor)] = MapTextStyle,
            [nameof(MauiRadioButton.FontSize)] = MapTextStyle,
            [nameof(MauiRadioButton.FontFamily)] = MapTextStyle,
            [nameof(MauiRadioButton.FontAttributes)] = MapTextStyle,
        };

    readonly Input input = new(InputType.Radio);
    readonly Span text = new();

    public RadioButtonHandler() : base(Mapper) { }

    protected override Label CreatePlatformView()
    {
        var label = new Label();
        label.Style["display"] = "inline-flex";
        label.Style["align-items"] = "center";
        label.Style["gap"] = "8px";
        label.AppendChild(input);
        label.AppendChild(text);
        return label;
    }

    protected override void ConnectHandler(Label platformView)
    {
        base.ConnectHandler(platformView);
        input.Change += OnChange;
        VirtualView.PropertyChanged += OnVirtualViewPropertyChanged;
        MapIsChecked(this, VirtualView);
        MapGroupName(this, VirtualView);
        MapContent(this, VirtualView);
        MapTextStyle(this, VirtualView);
    }

    protected override void DisconnectHandler(Label platformView)
    {
        input.Change -= OnChange;
        VirtualView.PropertyChanged -= OnVirtualViewPropertyChanged;
        base.DisconnectHandler(platformView);
    }

    // RadioButton raises plain property notifications, so the mapper has to be driven from them.
    void OnVirtualViewPropertyChanged(object? sender, PropertyChangedEventArgs args)
    {
        switch (args.PropertyName)
        {
            case nameof(MauiRadioButton.IsChecked): MapIsChecked(this, VirtualView); break;
            case nameof(MauiRadioButton.GroupName): MapGroupName(this, VirtualView); break;
            case nameof(MauiRadioButton.Content): MapContent(this, VirtualView); break;
            case nameof(MauiRadioButton.TextColor):
            case nameof(MauiRadioButton.FontSize):
            case nameof(MauiRadioButton.FontFamily):
            case nameof(MauiRadioButton.FontAttributes):
                MapTextStyle(this, VirtualView); break;
        }
    }

    void OnChange(object? sender, TargetEventArgs args)
    {
        if (VirtualView.IsChecked != input.IsChecked)
            VirtualView.IsChecked = input.IsChecked;
    }

    public static void MapIsChecked(RadioButtonHandler handler, MauiRadioButton radio)
    {
        if (handler.input.IsChecked != radio.IsChecked)
            handler.input.IsChecked = radio.IsChecked;
    }

    public static void MapGroupName(RadioButtonHandler handler, MauiRadioButton radio) =>
        handler.input.Name = radio.GroupName ?? "";

    public static void MapContent(RadioButtonHandler handler, MauiRadioButton radio) =>
        handler.text.Text = radio.Content?.ToString() ?? "";

    public static void MapTextStyle(RadioButtonHandler handler, MauiRadioButton radio)
    {
        OouiStyle.SetTextColor(handler.PlatformView, radio);
        OouiStyle.SetFont(handler.PlatformView, radio);
    }
}
