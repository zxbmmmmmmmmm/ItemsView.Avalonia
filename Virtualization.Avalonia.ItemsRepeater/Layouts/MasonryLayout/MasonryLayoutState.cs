namespace Virtualization.Avalonia.Layouts;
public class MasonryLayoutState
{
    private List<MasonryItem> _items = new List<MasonryItem>();
    private VirtualizingLayoutContext _context;
    private Dictionary<int, MasonryColumnLayout> _columnLayout = new Dictionary<int, MasonryColumnLayout>();
    private double _lastAverageHeight;

    public MasonryLayoutState(VirtualizingLayoutContext context)
    {
        _context = context;
    }

    public double ColumnWidth { get; internal set; }

    public int NumberOfColumns
    {
        get
        {
            return _columnLayout.Count;
        }
    }

    public double RowSpacing { get; internal set; }

    internal void AddItemToColumn(MasonryItem item, int columnIndex)
    {
        if (_columnLayout.TryGetValue(columnIndex, out MasonryColumnLayout? columnLayout) == false)
        {
            columnLayout = new MasonryColumnLayout();
            _columnLayout[columnIndex] = columnLayout;
        }

        if (columnLayout.Contains(item) == false)
        {
            columnLayout.Add(item);
        }
    }

    internal MasonryItem GetItemAt(int index)
    {
        if (index < 0)
        {
            throw new IndexOutOfRangeException();
        }

        if (index <= (_items.Count - 1))
        {
            return _items[index];
        }
        else
        {
            MasonryItem item = new MasonryItem(index);
            _items.Add(item);
            return item;
        }
    }

    internal MasonryColumnLayout GetColumnLayout(int columnIndex)
    {
        _columnLayout.TryGetValue(columnIndex, out MasonryColumnLayout? columnLayout);
        return columnLayout!;
    }

    /// <summary>
    /// Clear everything that has been calculated.
    /// </summary>
    internal void Clear()
    {
        _columnLayout.Clear();
        _items.Clear();
    }

    /// <summary>
    /// Clear the layout columns so they will be recalculated.
    /// </summary>
    internal void ClearColumns()
    {
        _columnLayout.Clear();
    }

    /// <summary>
    /// Gets the estimated height of the layout.
    /// </summary>
    /// <returns>The estimated height of the layout.</returns>
    /// <remarks>
    /// If all the items have been calculated then the actual height will be returned.
    /// If all the items have not been calculated then an estimated height will be calculated based on the average height of the items.
    /// </remarks>
    internal double GetHeight()
    {
        double desiredHeight = _columnLayout.Values.Max(c => c.Height);

        var itemCount = _columnLayout.Values.Sum(c => c.Count);
        if (itemCount == _context.ItemCount)
        {
            return desiredHeight;
        }

        double averageHeight = 0;
        foreach (var kvp in _columnLayout)
        {
            averageHeight += kvp.Value.Height / kvp.Value.Count;
        }

        averageHeight /= _columnLayout.Count;
        double estimatedHeight = (averageHeight * _context.ItemCount) / _columnLayout.Count;
        if (estimatedHeight > desiredHeight)
        {
            desiredHeight = estimatedHeight;
        }

        if (Math.Abs(desiredHeight - _lastAverageHeight) < 5)
        {
            return _lastAverageHeight;
        }

        _lastAverageHeight = desiredHeight;
        return desiredHeight;
    }

    internal void RecycleElementAt(int index)
    {
        var element = _context.GetOrCreateElementAt(index);
        _context.RecycleElement(element);
    }

    internal void RemoveFromIndex(int index)
    {
        if (index >= _items.Count)
        {
            // Item was added/removed but we haven't realized that far yet
            return;
        }

        int numToRemove = _items.Count - index;
        _items.RemoveRange(index, numToRemove);

        foreach (var kvp in _columnLayout)
        {
            MasonryColumnLayout layout = kvp.Value;
            for (int i = 0; i < layout.Count; i++)
            {
                if (layout[i].Index >= index)
                {
                    numToRemove = layout.Count - i;
                    layout.RemoveRange(i, numToRemove);
                    break;
                }
            }
        }
    }

    internal void RemoveRange(int startIndex, int endIndex)
    {
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (i > _items.Count)
            {
                break;
            }

            MasonryItem item = _items[i];
            item.Height = 0;
            item.Top = 0;

            // We must recycle all elements to ensure that it gets the correct context
            RecycleElementAt(i);
        }

        foreach (var kvp in _columnLayout)
        {
            MasonryColumnLayout layout = kvp.Value;
            for (int i = 0; i < layout.Count; i++)
            {
                if ((startIndex <= layout[i].Index) && (layout[i].Index <= endIndex))
                {
                    int numToRemove = layout.Count - i;
                    layout.RemoveRange(i, numToRemove);
                    break;
                }
            }
        }
    }
}