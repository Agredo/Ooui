using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Ooui.Maui.Handlers;

public class EntryHandler : OouiViewHandler<IEntry, TextInput>, IEntryHandler
{
    public static readonly IPropertyMapper<IEntry, EntryHandler> Mapper =
        new PropertyMapper<IEntry, EntryHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IText.Text)] = MapText,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(IPlaceholder.Placeholder)] = MapPlaceholder,
            [nameof(IPlaceholder.PlaceholderColor)] = MapPlaceholderColor,
            [nameof(IEntry.IsPassword)] = MapIsPassword,
            [nameof(ITextInput.IsReadOnly)] = MapIsReadOnly,
            [nameof(ITextInput.MaxLength)] = MapMaxLength,
            [nameof(ITextInput.Keyboard)] = MapKeyboard,
        };

    public EntryHandler() : base(Mapper) { }

    IEntry IEntryHandler.VirtualView => VirtualView;
    object IEntryHandler.PlatformView => PlatformView;

    protected override TextInput CreatePlatformView() => new();

    protected override void ConnectHandler(TextInput platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Input += OnInput;
        platformView.Change += OnChange;
        platformView.KeyDown += OnKeyDown;
        UpdateReturnType();
    }

    protected override void DisconnectHandler(TextInput platformView)
    {
        platformView.Input -= OnInput;
        platformView.Change -= OnChange;
        platformView.KeyDown -= OnKeyDown;
        base.DisconnectHandler(platformView);
    }

    void OnInput(object? sender, TargetEventArgs args) => UpdateText();

    void OnChange(object? sender, TargetEventArgs args)
    {
        UpdateText();
        VirtualView.Completed();
    }

    void OnKeyDown(object? sender, TargetEventArgs args)
    {
        if (args.Value == "Enter")
        {
            UpdateText();
            VirtualView.Completed();
        }
    }

    void UpdateText()
    {
        if (VirtualView is BindableObject bindable)
            bindable.SetValue(InputView.TextProperty, PlatformView.Value);
    }

    void UpdateReturnType() => PlatformView.SetAttribute("enterkeyhint", VirtualView.ReturnType switch
    {
        ReturnType.Go => "go",
        ReturnType.Next => "next",
        ReturnType.Search => "search",
        ReturnType.Send => "send",
        ReturnType.Done => "done",
        _ => "enter",
    });

    public static void MapText(EntryHandler handler, IEntry entry)
    {
        if (handler.PlatformView.Value != entry.Text)
            handler.PlatformView.Value = entry.Text ?? string.Empty;
    }

    public static void MapTextColor(EntryHandler handler, IEntry entry) =>
        handler.PlatformView.Style.Color = OouiStyle.ToCss(entry.TextColor);

    public static void MapFont(EntryHandler handler, IEntry entry) => OouiStyle.SetFont(handler.PlatformView, entry);

    public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "letter-spacing", entry.CharacterSpacing);

    public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry) =>
        handler.PlatformView.Style.TextAlign = entry.HorizontalTextAlignment.ToString().ToLowerInvariant();

    public static void MapPlaceholder(EntryHandler handler, IEntry entry) =>
        handler.PlatformView.Placeholder = entry.Placeholder ?? string.Empty;

    public static void MapPlaceholderColor(EntryHandler handler, IEntry entry)
    {
        var color = OouiStyle.ToCss(entry.PlaceholderColor);
        if (color is null)
            handler.PlatformView.Style["--ooui-placeholder-color"] = null;
        else
            handler.PlatformView.Style["--ooui-placeholder-color"] = color;
    }

    public static void MapIsPassword(EntryHandler handler, IEntry entry) => MapKeyboard(handler, entry);

    public static void MapIsReadOnly(EntryHandler handler, IEntry entry)
    {
        if (entry.IsReadOnly)
            handler.PlatformView.SetAttribute("readonly", string.Empty);
        else
            handler.PlatformView.RemoveAttribute("readonly");
    }

    public static void MapMaxLength(EntryHandler handler, IEntry entry) =>
        handler.PlatformView.SetAttribute("maxlength", entry.MaxLength < 0 ? null : entry.MaxLength);

    public static void MapKeyboard(EntryHandler handler, IEntry entry)
    {
        handler.PlatformView.Type = entry.IsPassword ? InputType.Password : entry.Keyboard switch
        {
            _ when entry.Keyboard == Keyboard.Email => InputType.Email,
            _ when entry.Keyboard == Keyboard.Numeric => InputType.Number,
            _ when entry.Keyboard == Keyboard.Telephone => InputType.Tel,
            _ when entry.Keyboard == Keyboard.Url => InputType.Url,
            _ => InputType.Text,
        };
    }
}
