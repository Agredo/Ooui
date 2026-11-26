using System.Collections;
using System.Collections.Specialized;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using MauiLabel = Microsoft.Maui.Controls.Label;
using MauiListView = Microsoft.Maui.Controls.ListView;

// ListView, ViewCell and TextCell are deprecated in MAUI but still used by existing MAUI apps.
#pragma warning disable CS0618

namespace Ooui.Maui.Handlers;

/// <summary>
/// Renders a <see cref="MauiListView"/> as a list of clickable rows. Row virtualization, context actions and
/// swipe gestures are not supported.
/// </summary>
public class ListViewHandler : OouiViewHandler<MauiListView, Div>
{
    public const string ItemsHostClassName = "ooui-list-items";
    public const string RowClassName = "ooui-list-row";
    public const string HeaderClassName = "ooui-list-header";
    public const string FooterClassName = "ooui-list-footer";

    public static readonly IPropertyMapper<MauiListView, ListViewHandler> Mapper =
        new PropertyMapper<MauiListView, ListViewHandler>(OouiViewMapper.Mapper)
        {
            [nameof(MauiListView.ItemsSource)] = MapRows,
            [nameof(MauiListView.ItemTemplate)] = MapRows,
            [nameof(MauiListView.Header)] = MapChrome,
            [nameof(MauiListView.HeaderTemplate)] = MapChrome,
            [nameof(MauiListView.Footer)] = MapChrome,
            [nameof(MauiListView.FooterTemplate)] = MapChrome,
            [nameof(MauiListView.SelectionMode)] = MapSelection,
            [nameof(MauiListView.SelectedItem)] = MapSelection,
            [nameof(MauiListView.SeparatorVisibility)] = MapSeparators,
            [nameof(MauiListView.SeparatorColor)] = MapSeparators,
        };

    sealed record Row(object? Item, Cell? Cell, Div Element);

    readonly Div itemsHost = new();
    readonly List<Row> rows = new();
    Ooui.Element? header;
    Ooui.Element? footer;

    public ListViewHandler() : base(Mapper) { }

    protected override Div CreatePlatformView()
    {
        var root = new Div();
        root.Style["display"] = "flex";
        root.Style["flex-direction"] = "column";
        root.Style["min-height"] = "0";

        itemsHost.ClassName = ItemsHostClassName;
        itemsHost.Style["display"] = "flex";
        itemsHost.Style["flex-direction"] = "column";
        itemsHost.Style["overflow"] = "auto";
        itemsHost.Style["flex"] = "1 1 auto";
        itemsHost.Style["min-height"] = "0";
        itemsHost.SetAttribute("role", "list");

        root.AppendChild(itemsHost);
        return root;
    }

    protected override void ConnectHandler(Div platformView)
    {
        base.ConnectHandler(platformView);
        ObserveSource();
        Rebuild();
        UpdateChrome();
    }

    protected override void DisconnectHandler(Div platformView)
    {
        UnobserveSource();
        rows.Clear();
        header = null;
        footer = null;
        base.DisconnectHandler(platformView);
    }

    void ObserveSource()
    {
        UnobserveSource();
        if (VirtualView.ItemsSource is INotifyCollectionChanged notifying)
            notifying.CollectionChanged += OnSourceChanged;
    }

    void UnobserveSource()
    {
        if (VirtualView.ItemsSource is INotifyCollectionChanged notifying)
            notifying.CollectionChanged -= OnSourceChanged;
    }

    // A simple rebuild per change keeps the handler small; ListView usage in the samples is small.
    void OnSourceChanged(object? sender, NotifyCollectionChangedEventArgs args) => Rebuild();

    void Rebuild()
    {
        rows.Clear();
        while (itemsHost.Children.Count > 0)
            itemsHost.RemoveChild(itemsHost.Children[0]);

        if (VirtualView.ItemsSource is IEnumerable source)
        {
            foreach (var item in source)
                AddRow(item);
        }

        UpdateSelection();
        UpdateSeparators();
    }

    void AddRow(object? item)
    {
        var cell = CreateCell(item);
        var row = new Div();
        row.ClassName = RowClassName;
        row.Style["display"] = "flex";
        row.Style["flex-direction"] = "column";
        row.Style["padding"] = "8px 4px";
        row.Style["cursor"] = "pointer";
        row.SetAttribute("role", "listitem");

        var index = rows.Count;
        row.AddEventListener("click", (_, _) => OnRowClicked(index));

        if (cell is not null && MauiContext is not null && RenderCell(cell) is { } content)
            row.AppendChild(content);

        itemsHost.AppendChild(row);
        rows.Add(new Row(item, cell, row));
    }

    Ooui.Element? RenderCell(Cell cell)
    {
        switch (cell)
        {
            case ViewCell viewCell:
                return viewCell.View is IView view ? view.ToOouiElement(MauiContext!) : null;

            case TextCell textCell:
                var text = new Div();
                text.Style["display"] = "flex";
                text.Style["flex-direction"] = "column";
                text.AppendChild(new Span(textCell.Text ?? ""));
                if (!string.IsNullOrEmpty(textCell.Detail))
                {
                    var detail = new Span(textCell.Detail);
                    detail.Style.Opacity = 0.7;
                    detail.Style.FontSize = "0.85em";
                    text.AppendChild(detail);
                }
                return text;

            default:
                return null;
        }
    }

    Cell? CreateCell(object? item)
    {
        if (item is Cell directCell)
            return directCell;

        if (VirtualView.ItemTemplate is { } template && template.CreateContent() is { } created)
        {
            switch (created)
            {
                case Cell cell:
                    cell.BindingContext = item;
                    return cell;
                case View view:
                    return new ViewCell { View = view, BindingContext = item };
            }
        }

        if (item is View itemView)
            return new ViewCell { View = itemView, BindingContext = item };

        var fallback = VirtualView.CreateDefaultCell(item);
        if (fallback is not null)
            fallback.BindingContext = item;
        return fallback;
    }

    void OnRowClicked(int index)
    {
        if (index < 0 || index >= rows.Count)
            return;

        var row = rows[index];
        if (VirtualView.SelectionMode != ListViewSelectionMode.None)
            VirtualView.SelectedItem = row.Item;

        VirtualView.NotifyRowTapped(index, row.Cell);
        UpdateSelection();
    }

    void UpdateSelection()
    {
        var selected = VirtualView.SelectedItem;
        foreach (var row in rows)
        {
            var isSelected = selected is not null && Equals(selected, row.Item);
            row.Element.Style.BackgroundColor = isSelected ? "rgba(0, 0, 0, 0.12)" : null;
            if (isSelected)
                row.Element.SetAttribute("aria-selected", "true");
            else
                row.Element.RemoveAttribute("aria-selected");
        }
    }

    void UpdateSeparators()
    {
        var visible = VirtualView.SeparatorVisibility == SeparatorVisibility.Default;
        var color = VirtualView.SeparatorColor is { } separatorColor ? OouiStyle.ToCss(separatorColor) : "rgba(0, 0, 0, 0.12)";
        for (var i = 0; i < rows.Count; i++)
        {
            var isLast = i == rows.Count - 1;
            rows[i].Element.Style["border-bottom"] = visible && !isLast ? $"1px solid {color}" : null;
        }
    }

    void UpdateChrome()
    {
        if (header is not null && PlatformView.Children.Contains(header))
            PlatformView.RemoveChild(header);
        header = null;
        if (footer is not null && PlatformView.Children.Contains(footer))
            PlatformView.RemoveChild(footer);
        footer = null;

        if (CreateChrome(VirtualView.Header, VirtualView.HeaderTemplate, HeaderClassName) is { } headerElement)
        {
            header = headerElement;
            PlatformView.InsertBefore(headerElement, itemsHost);
        }

        if (CreateChrome(VirtualView.Footer, VirtualView.FooterTemplate, FooterClassName) is { } footerElement)
        {
            footer = footerElement;
            PlatformView.AppendChild(footerElement);
        }
    }

    Ooui.Element? CreateChrome(object? data, DataTemplate? template, string className)
    {
        if (MauiContext is null)
            return null;

        Ooui.Element? element = null;
        if (template is not null)
        {
            if (template.CreateContent() is not View templated)
                return null;
            templated.BindingContext = data;
            element = templated.ToOouiElement(MauiContext);
        }
        else
        {
            element = data switch
            {
                View view => view.ToOouiElement(MauiContext),
                string text when text.Length > 0 => new MauiLabel { Text = text }.ToOouiElement(MauiContext),
                _ => null,
            };
        }

        element?.ClassName = className;
        return element;
    }

    public static void MapRows(ListViewHandler handler, MauiListView list)
    {
        handler.ObserveSource();
        handler.Rebuild();
    }

    public static void MapChrome(ListViewHandler handler, MauiListView list) => handler.UpdateChrome();

    public static void MapSelection(ListViewHandler handler, MauiListView list) => handler.UpdateSelection();

    public static void MapSeparators(ListViewHandler handler, MauiListView list) => handler.UpdateSeparators();
}

#pragma warning restore CS0618
