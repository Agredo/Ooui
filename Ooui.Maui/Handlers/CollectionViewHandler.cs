using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using MauiLabel = Microsoft.Maui.Controls.Label;

namespace Ooui.Maui.Handlers;

/// <summary>
/// Renders a <see cref="CollectionView"/> as one container per item. Nothing is virtualised:
/// every item becomes part of the page, which is what a server rendered site wants anyway,
/// because the browser only receives the items that changed afterwards.
/// </summary>
/// <remarks>
/// Supported: <c>ItemsSource</c> (including <see cref="INotifyCollectionChanged"/> with
/// incremental add, remove and replace), <c>ItemTemplate</c>, <c>EmptyView</c> and
/// <c>EmptyViewTemplate</c>, <c>Header</c> and <c>Footer</c> with their templates, grouping via
/// <c>IsGrouped</c>, <c>SelectionMode</c> with <c>SelectedItem</c> and <c>SelectedItems</c>,
/// <c>ItemsLayout</c> for spacing and orientation, and <c>RemainingItemsThreshold</c> driven by
/// the browser scroll position. <c>ScrollTo</c> is not implemented, because the browser owns the
/// scroll position.
/// </remarks>
public class CollectionViewHandler : OouiViewHandler<CollectionView, Div>
{
    /// <summary>Class name of the element that holds the items and scrolls.</summary>
    public const string ItemsHostClassName = "ooui-items";

    /// <summary>Class name of the element that wraps one group.</summary>
    public const string GroupClassName = "ooui-group";

    public static readonly IPropertyMapper<CollectionView, CollectionViewHandler> Mapper =
        new PropertyMapper<CollectionView, CollectionViewHandler> (OouiViewMapper.Mapper)
        {
            [nameof (ItemsView.ItemsSource)] = MapItemsSource,
            [nameof (ItemsView.ItemTemplate)] = MapItems,
            [nameof (ItemsView.EmptyView)] = MapItems,
            [nameof (ItemsView.EmptyViewTemplate)] = MapItems,
            [nameof (ItemsView.RemainingItemsThreshold)] = MapRemainingItemsThreshold,
            [nameof (StructuredItemsView.ItemsLayout)] = MapItemsLayout,
            [nameof (StructuredItemsView.Header)] = MapChrome,
            [nameof (StructuredItemsView.HeaderTemplate)] = MapChrome,
            [nameof (StructuredItemsView.Footer)] = MapChrome,
            [nameof (StructuredItemsView.FooterTemplate)] = MapChrome,
            [nameof (GroupableItemsView.IsGrouped)] = MapItems,
            [nameof (GroupableItemsView.GroupHeaderTemplate)] = MapItems,
            [nameof (GroupableItemsView.GroupFooterTemplate)] = MapItems,
            [nameof (SelectableItemsView.SelectionMode)] = MapItems,
            [nameof (SelectableItemsView.SelectedItem)] = MapSelectedItem,
        };

    readonly List<ItemContainer> containers = new ();
    readonly List<INotifyCollectionChanged> observedGroups = new ();
    Div itemsHost = null!;
    INotifyCollectionChanged? observedSource;
    bool syncingSelection;
    bool thresholdReached;
    double lastScrollX;
    double lastScrollY;

    public CollectionViewHandler () : base (Mapper) { }

    protected override Div CreatePlatformView ()
    {
        itemsHost = new Div { ClassName = ItemsHostClassName };
        var platformView = new Div ();
        platformView.Style.Display = "flex";
        platformView.Style["flex-direction"] = "column";
        platformView.Style["min-height"] = "0";
        return platformView;
    }

    protected override void ConnectHandler (Div platformView)
    {
        base.ConnectHandler (platformView);
        itemsHost.AddEventListener ("scroll", OnScrolled);
        Observe ();
        Rebuild ();
    }

    protected override void DisconnectHandler (Div platformView)
    {
        itemsHost.RemoveEventListener ("scroll", OnScrolled);
        Unobserve ();
        UnobserveGroups ();
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

    public static void MapChrome (CollectionViewHandler handler, CollectionView view) => handler.Rebuild ();

    public static void MapItemsLayout (CollectionViewHandler handler, CollectionView view) => handler.UpdateLayout ();

    public static void MapRemainingItemsThreshold (CollectionViewHandler handler, CollectionView view) => handler.thresholdReached = false;

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

    void UnobserveGroups ()
    {
        foreach (var group in observedGroups)
            group.CollectionChanged -= OnGroupChanged;
        observedGroups.Clear ();
    }

    void OnGroupChanged (object? sender, NotifyCollectionChangedEventArgs args) => Rebuild ();

    void OnItemsChanged (object? sender, NotifyCollectionChangedEventArgs args)
    {
        if (UsesGroups () || !ApplyIncrementalChange (args)) {
            Rebuild ();
            return;
        }
        thresholdReached = false;
        UpdateSelection ();
    }

    bool ApplyIncrementalChange (NotifyCollectionChangedEventArgs args)
    {
        switch (args.Action) {
            case NotifyCollectionChangedAction.Add when args.NewItems is { } added && args.NewStartingIndex >= 0:
                // The empty view has to go away before the first item appears, so start over.
                if (containers.Count == 0 || args.NewStartingIndex > containers.Count)
                    return false;
                InsertItems (args.NewStartingIndex, added);
                return true;
            case NotifyCollectionChangedAction.Remove when args.OldItems is { } removed && args.OldStartingIndex >= 0:
                if (args.OldStartingIndex + removed.Count > containers.Count)
                    return false;
                RemoveItems (args.OldStartingIndex, removed.Count);
                return true;
            case NotifyCollectionChangedAction.Replace when args.NewItems is { } replacement && args.OldItems is { } replaced && args.NewStartingIndex >= 0:
                if (args.NewStartingIndex + replaced.Count > containers.Count)
                    return false;
                ReplaceItems (args.NewStartingIndex, replacement);
                return true;
            default:
                return false;
        }
    }

    void InsertItems (int index, IList items)
    {
        for (var i = 0; i < items.Count; i++) {
            var container = CreateContainer (items[i]);
            if (index + i < containers.Count)
                itemsHost.InsertBefore (container.Element, containers[index + i].Element);
            else
                itemsHost.AppendChild (container.Element);
            containers.Insert (index + i, container);
        }
    }

    void RemoveItems (int index, int count)
    {
        for (var i = 0; i < count; i++) {
            var container = containers[index];
            containers.RemoveAt (index);
            itemsHost.RemoveChild (container.Element);
        }
        // The empty view comes back when the last item leaves.
        if (containers.Count == 0)
            Rebuild ();
    }

    void ReplaceItems (int index, IList items)
    {
        for (var i = 0; i < items.Count; i++) {
            var previous = containers[index + i];
            var container = CreateContainer (items[i]);
            itemsHost.InsertBefore (container.Element, previous.Element);
            itemsHost.RemoveChild (previous.Element);
            containers[index + i] = container;
        }
    }

    void UpdateLayout ()
    {
        var style = itemsHost.Style;
        style.Display = "flex";
        style["flex"] = "1 1 auto";
        style["min-height"] = "0";
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
        containers.Clear ();
        UnobserveGroups ();
        // The items host has to be emptied while it is still in the tree: Ooui addresses nodes by id, and
        // removeChild messages for a detached subtree never reach the browser.
        foreach (var child in itemsHost.Children.ToArray ())
            itemsHost.RemoveChild (child);
        foreach (var child in PlatformView.Children.ToArray ())
            PlatformView.RemoveChild (child);
        thresholdReached = false;

        if (MauiContext is null)
            return;

        if (CreateChrome (VirtualView.Header, VirtualView.HeaderTemplate) is { } header)
            PlatformView.AppendChild (header);
        PlatformView.AppendChild (itemsHost);
        if (CreateChrome (VirtualView.Footer, VirtualView.FooterTemplate) is { } footer)
            PlatformView.AppendChild (footer);

        UpdateLayout ();
        if (UsesGroups ())
            AddGroups ();
        else
            AddItems ();

        UpdateSelection ();
    }

    bool UsesGroups ()
    {
        if (!VirtualView.IsGrouped || VirtualView.ItemsSource is not IEnumerable source)
            return false;
        foreach (var item in source)
            return item is IEnumerable and not string;
        return false;
    }

    void AddItems ()
    {
        var items = ReadItems (VirtualView.ItemsSource);
        if (items.Count == 0) {
            AddEmptyView (itemsHost);
            return;
        }
        foreach (var item in items)
            AddItem (item, itemsHost);
    }

    void AddGroups ()
    {
        var groups = ReadItems (VirtualView.ItemsSource);
        if (groups.Count == 0) {
            AddEmptyView (itemsHost);
            return;
        }

        foreach (var group in groups) {
            var groupElement = new Div { ClassName = GroupClassName };
            groupElement.SetAttribute ("role", "group");
            groupElement.Style.Display = "flex";
            groupElement.Style["flex-direction"] = "column";

            if (CreateGroupChrome (group, VirtualView.GroupHeaderTemplate, useDefaultText: true) is { } groupHeader)
                groupElement.AppendChild (groupHeader);
            foreach (var item in ReadItems (group as IEnumerable))
                AddItem (item, groupElement);
            if (CreateGroupChrome (group, VirtualView.GroupFooterTemplate, useDefaultText: false) is { } groupFooter)
                groupElement.AppendChild (groupFooter);

            if (group is INotifyCollectionChanged notifying) {
                notifying.CollectionChanged += OnGroupChanged;
                observedGroups.Add (notifying);
            }

            itemsHost.AppendChild (groupElement);
        }
    }

    void AddItem (object? item, Div parent)
    {
        var container = CreateContainer (item);
        parent.AppendChild (container.Element);
        containers.Add (container);
    }

    ItemContainer CreateContainer (object? item)
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

        return new ItemContainer (item, container);
    }

    void AddEmptyView (Div parent)
    {
        if (MauiContext is null)
            return;

        if (VirtualView.EmptyViewTemplate is DataTemplate template && template.CreateContent () is View templated) {
            templated.BindingContext = VirtualView.EmptyView;
            parent.AppendChild (templated.ToOouiElement (MauiContext));
            return;
        }

        var view = VirtualView.EmptyView switch {
            View empty => empty,
            string text when text.Length > 0 => (View)new MauiLabel { Text = text },
            _ => null,
        };
        if (view is not null)
            parent.AppendChild (view.ToOouiElement (MauiContext));
    }

    Ooui.Element? CreateChrome (object? data, DataTemplate? template)
    {
        if (MauiContext is null)
            return null;

        if (template is not null) {
            if (template.CreateContent () is not View templated)
                return null;
            templated.BindingContext = data;
            return templated.ToOouiElement (MauiContext);
        }

        return data switch {
            View view => view.ToOouiElement (MauiContext),
            string text when text.Length > 0 => new MauiLabel { Text = text }.ToOouiElement (MauiContext),
            _ => null,
        };
    }

    Ooui.Element? CreateGroupChrome (object? group, DataTemplate? template, bool useDefaultText)
    {
        if (template is not null)
            return CreateChrome (group, template);
        if (!useDefaultText)
            return null;

        // Without a template MAUI shows the text of the group itself, so mirror that.
        var text = group?.ToString () ?? "";
        if (text.Length == 0)
            return null;

        var label = new MauiLabel { Text = text, FontAttributes = FontAttributes.Bold };
        return MauiContext is null ? null : label.ToOouiElement (MauiContext);
    }

    Ooui.Element? CreateItemContent (object? item)
    {
        if (MauiContext is null)
            return null;
        return CreateItemView (item)?.ToOouiElement (MauiContext);
    }

    View CreateItemView (object? item)
    {
        if (VirtualView.ItemTemplate is DataTemplate template && template.CreateContent () is View templated) {
            templated.BindingContext = item;
            return templated;
        }
        if (item is View itemView)
            return itemView;
        return new MauiLabel { Text = item?.ToString () ?? "" };
    }

    static List<object?> ReadItems (IEnumerable? source)
    {
        var items = new List<object?> ();
        if (source is not null)
            foreach (var item in source)
                items.Add (item);
        return items;
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
        itemsHost.SetAttribute ("role", selectable ? "listbox" : "list");
        if (VirtualView.SelectionMode == SelectionMode.Multiple)
            itemsHost.SetAttribute ("aria-multiselectable", "true");
        else
            itemsHost.RemoveAttribute ("aria-multiselectable");

        foreach (var container in containers) {
            container.Element.SetAttribute ("role", selectable ? "option" : "listitem");
            if (selectable)
                container.Element.SetAttribute ("aria-selected", IsSelected (container.Item) ? "true" : "false");
            else
                container.Element.RemoveAttribute ("aria-selected");
        }
    }

    void OnScrolled (object? sender, TargetEventArgs args)
    {
        var rowHeight = containers.Count > 0 ? args.ContentHeight / containers.Count : 0;
        var first = 0;
        var center = 0;
        var last = Math.Max (0, containers.Count - 1);
        if (rowHeight > 0) {
            first = ClampRow ((int)Math.Floor (args.ScrollY / rowHeight));
            center = ClampRow ((int)Math.Floor ((args.ScrollY + args.ClientHeight / 2) / rowHeight));
            last = ClampRow ((int)Math.Ceiling ((args.ScrollY + args.ClientHeight) / rowHeight) - 1);
        }

        VirtualView.SendScrolled (new ItemsViewScrolledEventArgs {
            VerticalOffset = args.ScrollY,
            VerticalDelta = args.ScrollY - lastScrollY,
            HorizontalOffset = args.ScrollX,
            HorizontalDelta = args.ScrollX - lastScrollX,
            FirstVisibleItemIndex = first,
            CenterItemIndex = center,
            LastVisibleItemIndex = last,
        });
        lastScrollX = args.ScrollX;
        lastScrollY = args.ScrollY;

        var threshold = VirtualView.RemainingItemsThreshold;
        if (threshold < 0 || thresholdReached || rowHeight <= 0)
            return;

        var remaining = args.ContentHeight - args.ScrollY - args.ClientHeight;
        if (remaining > threshold * rowHeight)
            return;

        thresholdReached = true;
        VirtualView.SendRemainingItemsThresholdReached ();
    }

    int ClampRow (int index) => Math.Clamp (index, 0, Math.Max (0, containers.Count - 1));

    sealed record ItemContainer (object? Item, Div Element);
}
