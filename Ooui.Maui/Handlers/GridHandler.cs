using System.Globalization;
using Microsoft.Maui.Handlers;
using ILayout = Microsoft.Maui.ILayout;

namespace Ooui.Maui.Handlers;

public class GridHandler : LayoutHandler
{
    public new static readonly IPropertyMapper<ILayout, GridHandler> Mapper =
        new PropertyMapper<ILayout, GridHandler>(LayoutHandler.Mapper)
        {
            [nameof(IGridLayout.RowDefinitions)] = MapGrid,
            [nameof(IGridLayout.ColumnDefinitions)] = MapGrid,
            [nameof(IGridLayout.RowSpacing)] = MapGrid,
            [nameof(IGridLayout.ColumnSpacing)] = MapGrid,
            [nameof(ILayout.Count)] = MapGrid,
            ["Column"] = MapChildPlacement,
            ["Row"] = MapChildPlacement,
            ["ColumnSpan"] = MapChildPlacement,
            ["RowSpan"] = MapChildPlacement,
        };

    public GridHandler() : base(Mapper) { }

    protected override void ConnectHandler(Div platformView)
    {
        base.ConnectHandler(platformView);
        if (VirtualView is Microsoft.Maui.Controls.Grid grid)
        {
            grid.ColumnDefinitions.ItemSizeChanged += OnDefinitionsChanged;
            grid.RowDefinitions.ItemSizeChanged += OnDefinitionsChanged;
        }
    }

    protected override void DisconnectHandler(Div platformView)
    {
        if (VirtualView is Microsoft.Maui.Controls.Grid grid)
        {
            grid.ColumnDefinitions.ItemSizeChanged -= OnDefinitionsChanged;
            grid.RowDefinitions.ItemSizeChanged -= OnDefinitionsChanged;
        }
        base.DisconnectHandler(platformView);
    }

    void OnDefinitionsChanged(object? sender, EventArgs args) => UpdateLayout();


    bool updatingLayout;
    bool layoutInvalidated;
    bool updatingAutomaticPlacement;
    readonly Dictionary<IView, (int Row, int Column)> automaticPlacements = new();

    protected override void UpdateLayout()
    {
        if (updatingLayout)
        {
            layoutInvalidated = true;
            return;
        }
        updatingLayout = true;
        try
        {
            do
            {
                layoutInvalidated = false;
                base.UpdateLayout();
                if (VirtualView is not IGridLayout grid) return;
                PlatformView.Style.Display = "grid";
                PlatformView.Style["grid-template-columns"] = string.Join(" ", grid.ColumnDefinitions.Select(column => Length(column.Width)));
                PlatformView.Style["grid-template-rows"] = string.Join(" ", grid.RowDefinitions.Select(row => Length(row.Height)));
                OouiStyle.SetDimension(PlatformView.Style, "row-gap", grid.RowSpacing);
                OouiStyle.SetDimension(PlatformView.Style, "column-gap", grid.ColumnSpacing);
                foreach (var child in grid) UpdateChild(child);
                UpdateAutomaticPlacement(grid);
            } while (layoutInvalidated);
        }
        finally
        {
            updatingLayout = false;
        }
    }

    void UpdateAutomaticPlacement(IGridLayout grid)
    {
        var occupied = new HashSet<(int Row, int Column)>();
        var columns = Math.Max(1, grid.ColumnDefinitions.Count);
        var next = 0;
        foreach (var child in automaticPlacements.Keys.ToArray())
            if (!grid.Contains(child)) automaticPlacements.Remove(child);
        updatingAutomaticPlacement = true;
        try
        {
            foreach (var child in grid)
            {
                if (child.Handler is not IElementHandler { PlatformView: Element element }) continue;
                if (!IsAutomaticPlacement(child))
                {
                    automaticPlacements.Remove(child);
                    PlaceSpan(element, grid, child);
                    UpdateOccupied(occupied, grid, child, grid.GetRow(child), grid.GetColumn(child));
                    continue;
                }
                var row = grid.GetRow(child);
                var column = grid.GetColumn(child);
                while (occupied.Contains((next / columns, next % columns))) next++;
                row = next / columns;
                column = next % columns;
                automaticPlacements[child] = (row, column);
                if (child is BindableObject bindable)
                {
                    bindable.SetValue(Microsoft.Maui.Controls.Grid.RowProperty, row);
                    bindable.SetValue(Microsoft.Maui.Controls.Grid.ColumnProperty, column);
                }
                element.Style["grid-row-start"] = row + 1;
                element.Style["grid-column-start"] = column + 1;
                PlaceSpan(element, grid, child);
                UpdateOccupied(occupied, grid, child, row, column);
                next++;
            }
        }
        finally
        {
            updatingAutomaticPlacement = false;
        }
    }

    static void UpdateOccupied(HashSet<(int Row, int Column)> occupied, IGridLayout grid, IView child, int row, int column)
    {
        for (var r = row; r < row + grid.GetRowSpan(child); r++)
            for (var c = column; c < column + grid.GetColumnSpan(child); c++)
                occupied.Add((r, c));
    }

    void PlaceSpan(Element element, IGridLayout grid, IView child)
    {
        element.Style["grid-row-end"] = $"span {grid.GetRowSpan(child)}";
        element.Style["grid-column-end"] = $"span {grid.GetColumnSpan(child)}";
    }

    // Materialized placements must keep looking automatic, so a later explicit write can be detected by comparison.
    bool IsAutomaticPlacement(IView child)
    {
        if (child is not BindableObject bindable) return false;
        if (bindable.IsSet(Microsoft.Maui.Controls.Grid.RowSpanProperty) ||
            bindable.IsSet(Microsoft.Maui.Controls.Grid.ColumnSpanProperty)) return false;
        var rowSet = bindable.IsSet(Microsoft.Maui.Controls.Grid.RowProperty);
        var columnSet = bindable.IsSet(Microsoft.Maui.Controls.Grid.ColumnProperty);
        if (!automaticPlacements.TryGetValue(child, out var placement))
            return !rowSet && !columnSet;
        return (!rowSet || (int)bindable.GetValue(Microsoft.Maui.Controls.Grid.RowProperty) == placement.Row) &&
            (!columnSet || (int)bindable.GetValue(Microsoft.Maui.Controls.Grid.ColumnProperty) == placement.Column);
    }

    protected override void UpdateChild(IView view)
    {
        if (VirtualView is not IGridLayout grid || view.Handler is not IElementHandler { PlatformView: Element element }) return;
        element.Style["grid-row-start"] = grid.GetRow(view) + 1;
        element.Style["grid-row-end"] = $"span {grid.GetRowSpan(view)}";
        element.Style["grid-column-start"] = grid.GetColumn(view) + 1;
        element.Style["grid-column-end"] = $"span {grid.GetColumnSpan(view)}";
        element.Style["justify-self"] = OouiStyle.ToCss(view.HorizontalLayoutAlignment);
        element.Style["align-self"] = OouiStyle.ToCss(view.VerticalLayoutAlignment);
    }

    protected override void OnChildPropertyChanged(IView view, string propertyName)
    {
        if (updatingAutomaticPlacement) return;
        if (propertyName is "Row" or "Column" or "RowSpan" or "ColumnSpan" or
            nameof(Microsoft.Maui.Controls.Grid.RowProperty) or
            nameof(Microsoft.Maui.Controls.Grid.ColumnProperty) or
            nameof(Microsoft.Maui.Controls.Grid.RowSpanProperty) or
            nameof(Microsoft.Maui.Controls.Grid.ColumnSpanProperty))
        {
            UpdateChild(view);
            if (VirtualView is IGridLayout grid) UpdateAutomaticPlacement(grid);
            return;
        }
        base.OnChildPropertyChanged(view, propertyName);
    }

    public static void MapGrid(GridHandler handler, ILayout layout) => handler.UpdateLayout();

    public static void MapChildPlacement(GridHandler handler, IView view)
    {
        handler.UpdateChild(view);
        if (handler.VirtualView is IGridLayout grid) handler.UpdateAutomaticPlacement(grid);
    }

    static string Length(GridLength length)
    {
        if (length.IsAuto) return "auto";
        if (length.IsStar) return length.Value.ToString(CultureInfo.InvariantCulture) + "fr";
        return length.Value.ToString(CultureInfo.InvariantCulture) + "px";
    }
}
