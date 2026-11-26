using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

public class SearchBarHandler : OouiViewHandler<ISearchBar, TextInput>, ISearchBarHandler
{
    public static readonly IPropertyMapper<ISearchBar, SearchBarHandler> Mapper =
        new PropertyMapper<ISearchBar, SearchBarHandler>(OouiViewMapper.Mapper)
        {
            [nameof(IText.Text)] = MapText,
            [nameof(ITextStyle.TextColor)] = MapTextColor,
            [nameof(ITextStyle.Font)] = MapFont,
            [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
            [nameof(IPlaceholder.Placeholder)] = MapPlaceholder,
            [nameof(ITextInput.IsReadOnly)] = MapIsReadOnly,
            [nameof(ITextInput.MaxLength)] = MapMaxLength,
        };

    public SearchBarHandler() : base(Mapper) { }

    ISearchBar ISearchBarHandler.VirtualView => VirtualView;
    object ISearchBarHandler.PlatformView => PlatformView;
    object ISearchBarHandler.QueryEditor => PlatformView;

    protected override TextInput CreatePlatformView() => new() { Type = InputType.Search };

    protected override void ConnectHandler(TextInput platformView)
    {
        base.ConnectHandler(platformView);
        platformView.Input += OnInput;
        platformView.Change += OnChange;
        platformView.KeyDown += OnKeyDown;
    }

    protected override void DisconnectHandler(TextInput platformView)
    {
        platformView.Input -= OnInput;
        platformView.Change -= OnChange;
        platformView.KeyDown -= OnKeyDown;
        base.DisconnectHandler(platformView);
    }

    void OnInput(object? sender, TargetEventArgs args) => UpdateText();
    void OnChange(object? sender, TargetEventArgs args) => UpdateText();
    void OnKeyDown(object? sender, TargetEventArgs args)
    {
        if (args.Value == "Enter")
        {
            UpdateText();
            VirtualView.SearchButtonPressed();
        }
    }

    void UpdateText()
    {
        if (VirtualView is BindableObject bindable)
            bindable.SetValue(Microsoft.Maui.Controls.SearchBar.TextProperty, PlatformView.Value);
    }

    public static void MapText(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (handler.PlatformView.Value != searchBar.Text)
            handler.PlatformView.Value = searchBar.Text ?? string.Empty;
    }

    public static void MapTextColor(SearchBarHandler handler, ISearchBar searchBar) =>
        handler.PlatformView.Style.Color = OouiStyle.ToCss(searchBar.TextColor);

    public static void MapFont(SearchBarHandler handler, ISearchBar searchBar) => OouiStyle.SetFont(handler.PlatformView, searchBar);

    public static void MapCharacterSpacing(SearchBarHandler handler, ISearchBar searchBar) =>
        OouiStyle.SetDimension(handler.PlatformView.Style, "letter-spacing", searchBar.CharacterSpacing);

    public static void MapPlaceholder(SearchBarHandler handler, ISearchBar searchBar) =>
        handler.PlatformView.Placeholder = searchBar.Placeholder ?? string.Empty;

    public static void MapIsReadOnly(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (searchBar.IsReadOnly)
            handler.PlatformView.SetAttribute("readonly", string.Empty);
        else
            handler.PlatformView.RemoveAttribute("readonly");
    }

    public static void MapMaxLength(SearchBarHandler handler, ISearchBar searchBar)
    {
        if (searchBar.MaxLength >= 0)
            handler.PlatformView.SetAttribute("maxlength", searchBar.MaxLength);
        else
            handler.PlatformView.RemoveAttribute("maxlength");
    }
}
