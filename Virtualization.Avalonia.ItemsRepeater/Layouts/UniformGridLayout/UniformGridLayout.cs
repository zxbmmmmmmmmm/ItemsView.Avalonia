// This source file is adapted from the WinUI project.
// (https://github.com/microsoft/microsoft-ui-xaml)
//
// Licensed to The Avalonia Project under MIT License, courtesy of The .NET Foundation.

using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Logging;

namespace Virtualization.Avalonia.Layouts;

/// <summary>
/// Defines constants that specify how items are aligned on the non-scrolling or non-virtualizing axis.
/// </summary>
public enum UniformGridLayoutItemsJustification
{
    /// <summary>
    /// Items are aligned with the start of the row or column, with extra space at the end.
    /// Spacing between items does not change.
    /// </summary>
    Start = 0,

    /// <summary>
    /// Items are aligned in the center of the row or column, with extra space at the start and
    /// end. Spacing between items does not change.
    /// </summary>
    Center = 1,

    /// <summary>
    /// Items are aligned with the end of the row or column, with extra space at the start.
    /// Spacing between items does not change.
    /// </summary>
    End = 2,

    /// <summary>
    /// Items are aligned so that extra space is added evenly before and after each item.
    /// </summary>
    SpaceAround = 3,

    /// <summary>
    /// Items are aligned so that extra space is added evenly between adjacent items. No space
    /// is added at the start or end.
    /// </summary>
    SpaceBetween = 4,

    SpaceEvenly = 5,
};

/// <summary>
/// Defines constants that specify how items are sized to fill the available space.
/// </summary>
public enum UniformGridLayoutItemsStretch
{
    /// <summary>
    /// The item retains its natural size. Use of extra space is determined by the
    /// <see cref="UniformGridLayout.ItemsJustification"/> property.
    /// </summary>
    None = 0,

    /// <summary>
    /// The item is sized to fill the available space in the non-scrolling direction. Item size
    /// in the scrolling direction is not changed.
    /// </summary>
    Fill = 1,

    /// <summary>
    /// The item is sized to both fill the available space in the non-scrolling direction and
    /// maintain its aspect ratio.
    /// </summary>
    Uniform = 2,
};

/// <summary>
/// Positions elements sequentially from left to right or top to bottom in a wrapping layout.
/// </summary>
public class UniformGridLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates, IOrientationBasedMeasures
{
    /// <summary>
    /// Defines the <see cref="ItemsJustification"/> property.
    /// </summary>
    public static readonly StyledProperty<UniformGridLayoutItemsJustification> ItemsJustificationProperty =
        AvaloniaProperty.Register<UniformGridLayout, UniformGridLayoutItemsJustification>(nameof(ItemsJustification));

    /// <summary>
    /// Defines the <see cref="ItemsStretch"/> property.
    /// </summary>
    public static readonly StyledProperty<UniformGridLayoutItemsStretch> ItemsStretchProperty =
        AvaloniaProperty.Register<UniformGridLayout, UniformGridLayoutItemsStretch>(nameof(ItemsStretch));

    /// <summary>
    /// Defines the <see cref="MinColumnSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinColumnSpacingProperty =
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(MinColumnSpacing));

    /// <summary>
    /// Defines the <see cref="MinItemHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinItemHeightProperty =
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(MinItemHeight));

    /// <summary>
    /// Defines the <see cref="MinItemWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(MinItemWidth));

    /// <summary>
    /// Defines the <see cref="MinRowSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinRowSpacingProperty =
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(MinRowSpacing));

    /// <summary>
    /// Defines the <see cref="MaximumRowsOrColumns"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaximumRowsOrColumnsProperty =
        AvaloniaProperty.Register<UniformGridLayout, int>(nameof(MaximumRowsOrColumns));

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackLayout.OrientationProperty.AddOwner<UniformGridLayout>();

    private double _minItemWidth = double.NaN;
    private double _minItemHeight = double.NaN;
    private double _minRowSpacing;
    private double _minColumnSpacing;
    private UniformGridLayoutItemsJustification _itemsJustification;
    private UniformGridLayoutItemsStretch _itemsStretch;
    private int _maximumRowsOrColumns = int.MaxValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="UniformGridLayout"/> class.
    /// </summary>
    public UniformGridLayout()
    {
        //LayoutId = "UniformGridLayout";
    } 

    static UniformGridLayout()
    {
        OrientationProperty.OverrideDefaultValue<UniformGridLayout>(Orientation.Horizontal);
    }

    /// <summary>
    /// Gets or sets a value that indicates how items are aligned on the non-scrolling or non-
    /// virtualizing axis.
    /// </summary>
    /// <value>
    /// An enumeration value that indicates how items are aligned. The default is Start.
    /// </value>
    public UniformGridLayoutItemsJustification ItemsJustification
    {
        get => GetValue(ItemsJustificationProperty);
        set => SetValue(ItemsJustificationProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates how items are sized to fill the available space.
    /// </summary>
    /// <value>
    /// An enumeration value that indicates how items are sized to fill the available space.
    /// The default is None.
    /// </value>
    /// <remarks>
    /// This property enables adaptive layout behavior where the items are sized to fill the
    /// available space along the non-scrolling axis, and optionally maintain their aspect ratio.
    /// </remarks>
    public UniformGridLayoutItemsStretch ItemsStretch
    {
        get => GetValue(ItemsStretchProperty);
        set => SetValue(ItemsStretchProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum space between items on the horizontal axis.
    /// </summary>
    /// <remarks>
    /// The spacing may exceed this minimum value when <see cref="ItemsJustification"/> is set
    /// to SpaceEvenly, SpaceAround, or SpaceBetween.
    /// </remarks>
    public double MinColumnSpacing
    {
        get => GetValue(MinColumnSpacingProperty);
        set => SetValue(MinColumnSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum height of each item.
    /// </summary>
    /// <value>
    /// The minimum height (in pixels) of each item. The default is NaN, in which case the
    /// height of the first item is used as the minimum.
    /// </value>
    public double MinItemHeight
    {
        get => GetValue(MinItemHeightProperty);
        set => SetValue(MinItemHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum width of each item.
    /// </summary>
    /// <value>
    /// The minimum width (in pixels) of each item. The default is NaN, in which case the width
    /// of the first item is used as the minimum.
    /// </value>
    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum space between items on the vertical axis.
    /// </summary>
    /// <remarks>
    /// The spacing may exceed this minimum value when <see cref="ItemsJustification"/> is set
    /// to SpaceEvenly, SpaceAround, or SpaceBetween.
    /// </remarks>
    public double MinRowSpacing
    {
        get => GetValue(MinRowSpacingProperty);
        set => SetValue(MinRowSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum row or column count.
    /// </summary>
    public int MaximumRowsOrColumns
    {
        get => GetValue(MaximumRowsOrColumnsProperty);
        set => SetValue(MaximumRowsOrColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the axis along which items are laid out.
    /// </summary>
    /// <value>
    /// One of the enumeration values that specifies the axis along which items are laid out.
    /// The default is Vertical.
    /// </value>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    internal double LineSpacing => Orientation == Orientation.Horizontal ? _minRowSpacing : _minColumnSpacing;
    internal double MinItemSpacing => Orientation == Orientation.Horizontal ? _minColumnSpacing : _minRowSpacing;

    private ScrollOrientation _scrollOrientation = ScrollOrientation.Vertical;

    ScrollOrientation IOrientationBasedMeasures.ScrollOrientation
    {
        get => _scrollOrientation;
        set => _scrollOrientation = value;
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(
        int index,
        Size availableSize,
        VirtualizingLayoutContext context)
    {
        var gridState = (UniformGridLayoutState)context.LayoutState!;
        return new Size(gridState.EffectiveItemWidth, gridState.EffectiveItemHeight);
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(
        int index,
        Size measureSize,
        Size desiredSize,
        VirtualizingLayoutContext context)
    {
        var gridState = (UniformGridLayoutState)context.LayoutState!;
        return new Size(gridState.EffectiveItemWidth, gridState.EffectiveItemHeight);
    }

    bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace) => remainingSpace < 0;

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(
        Size availableSize,
        VirtualizingLayoutContext context)
    {
        Rect bounds = new Rect(double.NaN, double.NaN, double.NaN, double.NaN);
        int anchorIndex = -1;

        int itemsCount = context.ItemsCount;
        var realizationRect = context.RealizationRect;
        if (itemsCount > 0 && this.MajorSize(realizationRect) > 0)
        {
            var gridState = (UniformGridLayoutState)context.LayoutState!;
            var lastExtent = gridState.FlowAlgorithm.LastExtent;
            var itemsPerLine = GetItemsCountPerLine(context, availableSize);
            var overlaps = this.MajorEnd(realizationRect) >= this.MajorStart(lastExtent)
                           && this.MajorStart(realizationRect) <= this.MajorEnd(lastExtent);
            if (overlaps)
            {
                var realizationWindowStartWithinExtent = this.MajorStart(realizationRect) - this.MajorStart(lastExtent);
                var o = Math.Max(0.0, realizationWindowStartWithinExtent + LineSpacing);
                var anchorLineIndex = (int)(o / GetMajorItemSizeWithSpacing(context));
                anchorIndex = Math.Clamp(anchorLineIndex * itemsPerLine, 0, itemsCount - 1);
                bounds = GetLayoutRectForDataIndex(anchorIndex, itemsPerLine, lastExtent, context);
            }
        }

        return new FlowLayoutAnchorInfo
        {
            Index = anchorIndex,
            Offset = this.MajorStart(bounds)
        };
    }

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(
        int targetIndex,
        Size availableSize,
        VirtualizingLayoutContext context)
    {
        var index = -1;
        var offset = double.NaN;
        var count = context.ItemsCount;
        if (targetIndex >= 0 && targetIndex < count)
        {
            var itemsPerLine = GetItemsCountPerLine(context, availableSize);
            var indexOfFirstInLine = targetIndex / itemsPerLine * itemsPerLine;
            index = indexOfFirstInLine;
            var state = (UniformGridLayoutState)context.LayoutState!;
            offset = this.MajorStart(GetLayoutRectForDataIndex(indexOfFirstInLine, itemsPerLine, state.FlowAlgorithm.LastExtent, context));
        }

        return new FlowLayoutAnchorInfo
        {
            Index = index,
            Offset = offset
        };
    }

    Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(
        Size availableSize,
        VirtualizingLayoutContext context,
        Control? firstRealized,
        int firstRealizedItemIndex,
        Rect firstRealizedLayoutBounds,
        Control? lastRealized,
        int lastRealizedItemIndex,
        Rect lastRealizedLayoutBounds)
    {
        var extent = new Rect();

        // Constants
        int itemsCount = context.ItemsCount;
        double availableSizeMinor = this.Minor(availableSize);
        int itemsPerLine = GetItemsCountPerLine(context, availableSize);
        double lineSize = GetMajorItemSizeWithSpacing(context);

        if (itemsCount > 0)
        {
            this.SetMinorSize(
                ref extent,
                !double.IsInfinity(availableSizeMinor) && _itemsStretch == UniformGridLayoutItemsStretch.Fill
                    ? availableSizeMinor
                    : Math.Max(0d, (itemsPerLine * GetMinorItemSizeWithSpacing(context)) - MinItemSpacing));
            this.SetMajorSize(
                ref extent,
                GetMajorSize(context, (itemsCount / itemsPerLine) + 1));

            if (firstRealized != null)
            {
                this.SetMajorStart(
                    ref extent,
                    this.MajorStart(firstRealizedLayoutBounds) - GetMajorSize(context, firstRealizedItemIndex / itemsPerLine));
                int remainingLines = (itemsCount / itemsPerLine) - (lastRealizedItemIndex / itemsPerLine) - 1;
                this.SetMajorSize(
                    ref extent,
                    this.MajorEnd(lastRealizedLayoutBounds) - this.MajorStart(extent) + GetMajorSize(context, remainingLines));
            }
            else
            {
                Logger.TryGet(LogEventLevel.Verbose, "Repeater")?.Log(this, $"{nameof(UniformGridLayout)}: Estimating extent with no realized elements");
            }
        }

        Logger.TryGet(LogEventLevel.Verbose, "Repeater")?.Log(this, $"{nameof(UniformGridLayout)}: Extent is ({extent.Size}). Based on lineSize {lineSize} and items per line {itemsPerLine}");
        return extent;
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(Control element, int index, Size availableSize, Size measureSize, Size desiredSize, Size provisionalArrangeSize, VirtualizingLayoutContext context)
    {
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(int startIndex, int countInLine, double lineSize, VirtualizingLayoutContext context)
    {
    }

    protected internal override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        var state = context.LayoutState;

        if (state is not UniformGridLayoutState gridState)
        {
            if (state is not null)
                throw new InvalidOperationException($"{nameof(context.LayoutState)} must derive from {nameof(UniformGridLayoutState)}.");

            // Custom deriving layouts could potentially be stateful.
            // If that is the case, we will just create the base state required by UniformGridLayout ourselves.
            gridState = new UniformGridLayoutState();
        }

        gridState.InitializeForContext(context, this);
    }

    protected internal override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        var gridState = (UniformGridLayoutState)context.LayoutState!;
        gridState.UninitializeForContext(context);
    }

    protected internal override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        // Set the width and height on the grid state. If the user already set them then use the preset. 
        // If not, we have to measure the first element and get back a size which we're going to be using for the rest of the items.
        var gridState = (UniformGridLayoutState)context.LayoutState!;
        gridState.EnsureElementSize(availableSize, context, _minItemWidth, _minItemHeight, _itemsStretch, Orientation, MinRowSpacing, MinColumnSpacing, _maximumRowsOrColumns);

        var desiredSize = GetFlowAlgorithm(context).Measure(
            availableSize,
            context,
            true,
            MinItemSpacing,
            LineSpacing,
            _maximumRowsOrColumns,
            _scrollOrientation,
            false);

        // If after Measure the first item is in the realization rect, then we revoke grid state's ownership,
        // and only use the layout when to clear it when it's done.
        gridState.EnsureFirstElementOwnership(context);

        return desiredSize;
    }

    protected internal override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var value = GetFlowAlgorithm(context).Arrange(
            finalSize,
            context,
            true,
            (FlowLayoutAlgorithm.LineAlignment)_itemsJustification);

        return value;
    }

    protected internal override void OnItemsChangedCore(VirtualizingLayoutContext context, object? source, NotifyCollectionChangedEventArgs args)
    {
        GetFlowAlgorithm(context).OnItemsSourceChanged(source, args, context);
        // Always invalidate layout to keep the view accurate.
        InvalidateLayout();

        var gridState = (UniformGridLayoutState)context.LayoutState!;
        gridState.ClearElementOnDataSourceChange(context, args);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
        {
            var orientation = change.GetNewValue<Orientation>();

            //Note: For UniformGridLayout Vertical Orientation means we have a Horizontal ScrollOrientation. Horizontal Orientation means we have a Vertical ScrollOrientation.
            //i.e. the properties are the inverse of each other.
            _scrollOrientation = orientation == Orientation.Horizontal ? ScrollOrientation.Vertical : ScrollOrientation.Horizontal;
        }
        else if (change.Property == MinColumnSpacingProperty)
        {
            _minColumnSpacing = change.GetNewValue<double>();
        }
        else if (change.Property == MinRowSpacingProperty)
        {
            _minRowSpacing = change.GetNewValue<double>();
        }
        else if (change.Property == ItemsJustificationProperty)
        {
            _itemsJustification = change.GetNewValue<UniformGridLayoutItemsJustification>();
        }
        else if (change.Property == ItemsStretchProperty)
        {
            _itemsStretch = change.GetNewValue<UniformGridLayoutItemsStretch>();
        }
        else if (change.Property == MinItemWidthProperty)
        {
            _minItemWidth = change.GetNewValue<double>();
        }
        else if (change.Property == MinItemHeightProperty)
        {
            _minItemHeight = change.GetNewValue<double>();
        }
        else if (change.Property == MaximumRowsOrColumnsProperty)
        {
            _maximumRowsOrColumns = change.GetNewValue<int>();
        }

        InvalidateLayout();
    }

    private int GetItemsCountPerLine(VirtualizingLayoutContext context, Size availableSize)
    {
        return Math.Max(1,
            (int) Math.Min(_maximumRowsOrColumns,
                (this.Minor(availableSize) + MinItemSpacing) / GetMinorItemSizeWithSpacing(context)));
    }

    private double GetMinorItemSizeWithSpacing(VirtualizingLayoutContext context)
    {
        var gridState = (UniformGridLayoutState)context.LayoutState!;
        return (_scrollOrientation == ScrollOrientation.Vertical
            ? gridState.EffectiveItemWidth
            : gridState.EffectiveItemHeight) + MinItemSpacing;
    }

    private double GetMajorSize(VirtualizingLayoutContext context, int lineCount)
    {
        var gridState = (UniformGridLayoutState) context.LayoutState!;
        var majorItemSize = _scrollOrientation == ScrollOrientation.Vertical
            ? gridState.EffectiveItemHeight
            : gridState.EffectiveItemWidth;
        var lineSpacing = LineSpacing;
        return Math.Max(0, ((majorItemSize + lineSpacing) * lineCount) - lineSpacing);
    }

    private double GetMajorItemSizeWithSpacing(VirtualizingLayoutContext context)
    {
        var gridState = (UniformGridLayoutState) context.LayoutState!;
        return (_scrollOrientation == ScrollOrientation.Vertical
            ? gridState.EffectiveItemHeight
            : gridState.EffectiveItemWidth) + LineSpacing;
    }

    private Rect GetLayoutRectForDataIndex(
        int index,
        int itemsPerLine,
        Rect lastExtent,
        VirtualizingLayoutContext context)
    {
        int lineIndex = index / itemsPerLine;
        int indexInLine = index % itemsPerLine;

        var gridState = (UniformGridLayoutState)context.LayoutState!;
        Rect bounds = this.MinorMajorRect(
            (indexInLine * GetMinorItemSizeWithSpacing(context)) + this.MinorStart(lastExtent),
            (lineIndex * GetMajorItemSizeWithSpacing(context)) + this.MajorStart(lastExtent),
            _scrollOrientation == ScrollOrientation.Vertical ? gridState.EffectiveItemWidth : gridState.EffectiveItemHeight,
            _scrollOrientation == ScrollOrientation.Vertical ? gridState.EffectiveItemHeight : gridState.EffectiveItemWidth);

        return bounds;
    }

    private void InvalidateLayout() => InvalidateMeasure();

    private static FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context) => ((UniformGridLayoutState)context.LayoutState!).FlowAlgorithm;
}
