using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Ooui.Maui.Handlers;

/// <summary>
/// Renders a <see cref="CollectionView"/> as one container per item. Nothing is virtualised:
/// every item becomes part of the page, which is what a server rendered site wants anyway,
/// because the browser only receives the items that changed afterwards.
/// </summary>
/// <remarks>
/// Supported: <c>ItemsSource</c> (including <see cref="INotifyCollectionChanged"/>),
/// <c>ItemTemplate</c>, <c>EmptyView</c>, <c>SelectionMode</c> with <c>SelectedItem</c> and
/// <c>SelectedItems</c>, and <c>ItemsLayout</c> for spacing and orientation. Grouping,
/// headers, footers and scrolling events are not implemented yet.
/// </remarks>
public class CollectionViewHandler : OouiViewHandler<CollectionView, Div>
{
    public static readonly IPropertyMapper<CollectionView, CollectionViewHandler> Mapper =
        new PropertyMapper<CollectionView, CollectionViewHandler>(OouiViewMapper.Mapper)
        {
            [nameof(ItemsView.ItemsSource)] = MapItemsSource,
            [nameof(ItemsView.ItemTemplate)] = MapItems,
            [nameof(ItemsView.EmptyView)] = MapItems,
            [nameof(StructuredItemsView.ItemsLayout)] = MapItemsLayout,
            [nameof(SelectableItemsView.SelectionMode)] = MapSelectionMode,
            [nameof(SelectableItemsView.SelectedItem)] = MapSelectedItem,
        };

    readonly List<ItemContainer> containers = new ();
    INotifyCollectionChanged? observedSource;
    bool syncingSelection;

    public CollectionViewHandler () : base (Mapper) { }

    protected override Div CreatePlatformView () => new ();

    protected override void ConnectHandler (Div platformView)
    {
        base.ConnectHandler (platformView);
        Observe ();
        Rebuild ();
    }

    protected override void DisconnectHandler (Div platformView)
    {
        Unobserve ();
        containers.Clear ();
        base.DisconnectHandler (platformView);
    }

    // The list is laid out by CSS: an explicit frame would freeze it at its measured height.
    public override void PlatformArrange (Microsoft.Maui.Graphics.Rect frame) { }

    public static void MapItemsSource (CollectionViewHandler handler, CollectionView view)
    {
        handler.Observe ();
        handler.Rebuild ();
    }

    public static void MapItems (CollectionViewHandler handler, CollectionView view) => handler.Rebuild ();

    public static void MapItemsLayout (CollectionViewHandler handler, CollectionView view) => handler.UpdateLayout ();

    public static void MapSelectionMode (CollectionViewHandler handler, CollectionView view) => handler.Rebuild ();

    public static void MapSelectedItem (CollectionViewHandler handler, CollectionView view)
    {
        if (!handler.syncingSelection)
            handler.UpdateSelection ();
    }

    void Observe ()
    {
        Unobserve ();
        if (VirtualView.ItemsSource is INotifyCollectionChanged notifying) {
            observedSource = notifying;
            notifying.CollectionChanged += OnItemsChanged;
        }
    }

    void Unobserve ()
    {
        if (observedSource is not null)
            observedSource.CollectionChanged -= OnItemsChanged;
        observedSource = null;
    }

    void OnItemsChanged (object? sender, NotifyCollectionChangedEventArgs args) => Rebuild ();

    void UpdateLayout ()
    {
        var style = PlatformView.Style;
        style.Display = "flex";
        style["flex-direction"] = "column";
        style["grid-template-columns"] = null;
        style["row-gap"] = null;
        style["column-gap"] = null;
        style.Overflow = "auto";

        switch (VirtualView.ItemsLayout) {
            case LinearItemsLayout linear:
                style["flex-direction"] = linear.Orientation == ItemsLayoutOrientation.Horizontal ? "row" : "column";
                OouiStyle.SetDimension (style, "row-gap", linear.ItemSpacing);
                OouiStyle.SetDimension (style, "column-gap", linear.ItemSpacing);
                break;
            case GridItemsLayout grid:
                style.Display = "grid";
                style["grid-template-columns"] = $"repeat({Math.Max (1, grid.Span)}, minmax(0, 1fr))";
                OouiStyle.SetDimension (style, "row-gap", grid.VerticalItemSpacing);
                OouiStyle.SetDimension (style, "column-gap", grid.HorizontalItemSpacing);
                break;
        }
    }

    void Rebuild ()
    {
        foreach (var child in PlatformView.Children.ToArray ())
            PlatformView.RemoveChild (child);
        containers.Clear ();
        UpdateLayout ();

        if (MauiContext is null)
            return;

        var items = ReadItems ();
        if (items.Count == 0)
            AddEmptyView ();
        else
            foreach (var item in items)
                AddItem (item);

        UpdateSelection ();
    }

    List<object?> ReadItems ()
    {
        var items = new List<object?> ();
        if (VirtualView.ItemsSource is IEnumerable source)
            foreach (var item in source)
                items.Add (item);
        return items;
    }

    void AddItem (object? item)
    {
        var selectable = VirtualView.SelectionMode != SelectionMode.None;
        var container = new Div { Style = { Display = "flex" } };
        container.Style["flex-direction"] = "column";
        if (selectable) {
            container.Style["cursor"] = "pointer";
            container.SetAttribute ("tabindex", "0");
            container.AddEventListener ("click", (_, _) => OnItemClicked (item));
        }

        if (CreateItemContent (item) is { } content)
            container.AppendChild (content);
        PlatformView.AppendChild (container);
        containers.Add (new ItemContainer (item, container));
    }

    void AddEmptyView ()
    {
        var view = VirtualView.EmptyView switch {
            View empty => empty,
            string text when text.Length > 0 => (View)new Microsoft.Maui.Controls.Label { Text = text },
            _ => null,
        };
        if (view is not null)
            PlatformView.AppendChild (view.ToOouiElement (MauiContext!));
    }

    Element? CreateItemContent (object? item)
    {
        var view = CreateItemView (item);
        return view?.ToOouiElement (MauiContext!);
    }

    View CreateItemView (object? item)
    {
        if (VirtualView.ItemTemplate is DataTemplate template && template.CreateContent () is View templated) {
            templated.BindingContext = item;
            return templated;
        }
        if (item is View itemView)
            return itemView;
        return new Microsoft.Maui.Controls.Label { Text = item?.ToString () ?? "" };
    }

    void OnItemClicked (object? item)
    {
        syncingSelection = true;
        try {
            if (VirtualView.SelectionMode == SelectionMode.Multiple) {
                var selectedItems = VirtualView.SelectedItems;
                var existing = selectedItems.FirstOrDefault (x => ReferenceEquals (x, item));
                if (existing is not null)
                    selectedItems.Remove (existing);
                else if (item is not null)
                    selectedItems.Add (item);
            }
            else {
                VirtualView.SelectedItem = ReferenceEquals (VirtualView.SelectedItem, item) ? null : item;
            }
        }
        finally {
            syncingSelection = false;
        }
        UpdateSelection ();
    }

    bool IsSelected (object? item)
    {
        if (item is null)
            return false;
        return VirtualView.SelectionMode switch {
            SelectionMode.None => false,
            SelectionMode.Multiple => VirtualView.SelectedItems.Any (x => ReferenceEquals (x, item)),
            _ => ReferenceEquals (VirtualView.SelectedItem, item),
        };
    }

    void UpdateSelection ()
    {
        var selectable = VirtualView.SelectionMode != SelectionMode.None;
        PlatformView.SetAttribute ("role", selectable ? "listbox" : "list");
        if (VirtualView.SelectionMode == SelectionMode.Multiple)
            PlatformView.SetAttribute ("aria-multiselectable", "true");
        else
            PlatformView.RemoveAttribute ("aria-multiselectable");

        foreach (var container in containers) {
            container.Element.SetAttribute ("role", selectable ? "option" : "listitem");
            if (selectable)
                container.Element.SetAttribute ("aria-selected", IsSelected (container.Item) ? "true" : "false");
            else
                container.Element.RemoveAttribute ("aria-selected");
        }
    }

    sealed record ItemContainer (object? Item, Div Element);
}
