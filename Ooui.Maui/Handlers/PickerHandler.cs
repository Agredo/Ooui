using System.Collections;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class PickerHandler : OouiViewHandler<IPicker, Select>, IPickerHandler
{
    public static readonly IPropertyMapper<IPicker, PickerHandler> Mapper =
        new PropertyMapper<IPicker, PickerHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IPicker.Items)] = MapItems,
            [nameof(IPicker.SelectedIndex)] = MapSelectedIndex,
            [nameof(IPicker.Title)] = MapTitle,
            [nameof(IPicker.TitleColor)] = MapTitle,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
        };

    public PickerHandler() : base(Mapper) { }

    IPicker IPickerHandler.VirtualView => VirtualView;
    object IPickerHandler.PlatformView => PlatformView;

    protected override Select CreatePlatformView() => new();

    protected override void ConnectHandler(Select platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Change += OnChange;
        if (VirtualView is Microsoft.Maui.Controls.Picker picker)
        {
            if (picker.Items is System.Collections.Specialized.INotifyCollectionChanged items)
                items.CollectionChanged += OnItemsChanged;
            UpdateItems(this, VirtualView);
        }
    }

    protected override void DisconnectHandler(Select platformView)
    {
        if (VirtualView is Microsoft.Maui.Controls.Picker picker && picker.Items is System.Collections.Specialized.INotifyCollectionChanged items)
            items.CollectionChanged -= OnItemsChanged;
        platformView.Change -= OnChange;
        base.DisconnectHandler(platformView);
    }

    void OnItemsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs args) =>
        UpdateItems(this, VirtualView);

    void OnChange(object? sender, TargetEventArgs args)
    {
        var index = PlatformView.Children.OfType<Option>().ToList().FindIndex(option => option.Value == PlatformView.Value);
        if (!string.IsNullOrEmpty(VirtualView.Title) && index >= 0) index--;
        if (VirtualView is BindableObject bindable)
            bindable.SetValue(Microsoft.Maui.Controls.Picker.SelectedIndexProperty, index);
    }

    public static void MapItems(PickerHandler handler, IPicker picker) => UpdateItems(handler, picker);

    static void UpdateItems(PickerHandler handler, IPicker picker)
    {
        foreach (var child in handler.PlatformView.Children.ToArray())
            handler.PlatformView.RemoveChild(child);
        if (!string.IsNullOrEmpty(picker.Title))
        {
            var title = new Option { Text = picker.Title, Value = string.Empty, DefaultSelected = picker.SelectedIndex < 0 };
            title.SetAttribute("disabled", string.Empty);
            handler.PlatformView.AppendChild(title);
        }
        for (var index = 0; index < picker.Items.Count; index++)
        {
            var option = new Option
            {
                Text = picker.Items[index],
                Value = index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                DefaultSelected = index == picker.SelectedIndex,
            };
            handler.PlatformView.AppendChild(option);
        }
    }

    public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
    {
        handler.PlatformView.Value = picker.SelectedIndex < 0 ? string.Empty : picker.SelectedIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
    }

    public static void MapTitle(PickerHandler handler, IPicker picker) => UpdateItems(handler, picker);

    public static void MapTextColor(PickerHandler handler, IPicker picker) =>
        handler.PlatformView.Style.Color = OouiStyle.ToCss((picker as ITextStyle)?.TextColor);

    public static void MapFont(PickerHandler handler, IPicker picker)
    {
        if (picker is ITextStyle textStyle) OouiStyle.SetFont(handler.PlatformView, textStyle);
    }

    public static void MapCharacterSpacing(PickerHandler handler, IPicker picker) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "letter-spacing", picker.CharacterSpacing);

    public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker picker) =>
        handler.PlatformView.Style.TextAlign = picker.HorizontalTextAlignment.ToString().ToLowerInvariant();
}
