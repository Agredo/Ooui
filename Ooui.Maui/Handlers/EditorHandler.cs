using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class EditorHandler : OouiViewHandler<IEditor, TextArea>, IEditorHandler
{
    public static readonly IPropertyMapper<IEditor, EditorHandler> Mapper =
        new PropertyMapper<IEditor, EditorHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IText.Text)] = MapText,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
            [nameof(IPlaceholder.Placeholder)] = MapPlaceholder,
            [nameof(ITextInput.IsReadOnly)] = MapIsReadOnly,
            [nameof(ITextInput.MaxLength)] = MapMaxLength,
        };

    public EditorHandler() : base(Mapper) { }

    IEditor IEditorHandler.VirtualView => VirtualView;
    object IEditorHandler.PlatformView => PlatformView;

    protected override TextArea CreatePlatformView() => new() { Rows = 4 };

    protected override void ConnectHandler(TextArea platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Input += OnInput;
        platformView.Change += OnChange;
    }

    protected override void DisconnectHandler(TextArea platformView)
    {
        platformView.Input -= OnInput;
        platformView.Change -= OnChange;
        base.DisconnectHandler(platformView);
    }

    void OnInput(object? sender, TargetEventArgs args)
    {
        if (VirtualView is BindableObject bindable)
            bindable.SetValue(InputView.TextProperty, PlatformView.Value);
    }

    void OnChange(object? sender, TargetEventArgs args)
    {
        OnInput(sender, args);
        VirtualView.Completed();
    }

    public static void MapText(EditorHandler handler, IEditor editor)
    {
        if (handler.PlatformView.Value != editor.Text)
            handler.PlatformView.Value = editor.Text ?? string.Empty;
    }

    public static void MapTextColor(EditorHandler handler, IEditor editor) =>
        handler.PlatformView.Style.Color = OouiStyle.ToCss(editor.TextColor);

    public static void MapFont(EditorHandler handler, IEditor editor) => OouiStyle.SetFont(handler.PlatformView, editor);

    public static void MapCharacterSpacing(EditorHandler handler, IEditor editor) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "letter-spacing", editor.CharacterSpacing);

    public static void MapHorizontalTextAlignment(EditorHandler handler, IEditor editor) =>
        handler.PlatformView.Style.TextAlign = editor.HorizontalTextAlignment.ToString().ToLowerInvariant();

    public static void MapPlaceholder(EditorHandler handler, IEditor editor) =>
        handler.PlatformView.SetAttribute("placeholder", editor.Placeholder ?? string.Empty);

    public static void MapIsReadOnly(EditorHandler handler, IEditor editor)
    {
        if (editor.IsReadOnly)
            handler.PlatformView.SetAttribute("readonly", string.Empty);
        else
            handler.PlatformView.RemoveAttribute("readonly");
    }

    public static void MapMaxLength(EditorHandler handler, IEditor editor)
    {
        if (editor.MaxLength >= 0)
            handler.PlatformView.SetAttribute("maxlength", editor.MaxLength);
        else
            handler.PlatformView.RemoveAttribute("maxlength");
    }
}
