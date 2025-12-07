// This source file is adapted from the WinUI project.
// (https://github.com/microsoft/microsoft-ui-xaml)
//
// Licensed to The Avalonia Project under MIT License, courtesy of The .NET Foundation.

using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
#if DEBUG && REPEATER_TRACE
using Avalonia.Logging;
#endif

namespace Virtualization.Avalonia.Layouts;

/// <summary>
/// Positions elements sequentially from left to right or top to bottom in a wrapping layout.
/// </summary>
public partial class UniformGridLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates, IOrientationBasedMeasures
{
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

    bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace, VirtualizingLayoutContext context) => remainingSpace < 0;

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(
        Size availableSize,
        VirtualizingLayoutContext context)
    {
        var offset = double.NaN;
        var anchorIndex = -1;

        var itemsCount = context.ItemsCount;
        var realizationRect = context.RealizationRect;
        if (itemsCount > 0 && this.MajorSize(realizationRect) > 0)
        {
            var lastExtent = GetFlowAlgorithm(context).LastExtent;
            var itemsPerLine = GetItemsCountPerLine(context, availableSize);

            var majorSize = this.MajorSize(lastExtent);
            if (majorSize is 0)
                majorSize = Math.Max(0, ((itemsCount / itemsPerLine + 1) * GetMajorItemSizeWithSpacing(context)) - _lineSpacing);

            var realizationWindowStartWithinExtent = this.MajorStart(realizationRect) - this.MajorStart(lastExtent);
            if (realizationWindowStartWithinExtent + this.MajorSize(realizationRect) >= 0 &&
                realizationWindowStartWithinExtent <= majorSize)
            {
                var o = Math.Max(0, realizationWindowStartWithinExtent + _lineSpacing);
                var anchorLineIndex = (int)(o / GetMajorItemSizeWithSpacing(context));
                anchorIndex = Math.Clamp(anchorLineIndex * itemsPerLine, 0, itemsCount - 1);
                offset = this.MajorStart(GetLayoutRectForDataIndex(anchorIndex, itemsPerLine, lastExtent, context));
            }
        }

        return new FlowLayoutAnchorInfo
        {
            Index = anchorIndex,
            Offset = offset
        };
    }

    int IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorIndexForTargetElement(int targetIndex, Size availableSize, VirtualizingLayoutContext context)
    {
        var index = -1;
        var count = context.ItemsCount;
        if (targetIndex >= 0 && targetIndex < count)
        {
            var itemsPerLine = GetItemsCountPerLine(context, availableSize);
            index = targetIndex / itemsPerLine * itemsPerLine;
        }

        return index;
        /*
        var index = -1;
        var offset = double.NaN;
        var count = context.ItemsCount;
        if (targetIndex >= 0 && targetIndex < count)
        {
            var itemsPerLine = GetItemsCountPerLine(context, availableSize);
            var indexOfFirstInLine = targetIndex / itemsPerLine * itemsPerLine;
            index = indexOfFirstInLine;
            var state = (UniformGridLayoutState) context.LayoutState!;
            offset = this.MajorStart(GetLayoutRectForDataIndex(indexOfFirstInLine, itemsPerLine, state.FlowAlgorithm.LastExtent, context));
        }

        return new FlowLayoutAnchorInfo
        {
            Index = index,
            Offset = offset
        };
        */
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
        var itemsCount = context.ItemsCount;
        var availableSizeMinor = this.Minor(availableSize);
        var itemsPerLine = GetItemsCountPerLine(context, availableSize);

        if (itemsCount > 0)
        {
            this.SetMinorSize(
                ref extent,
                !double.IsInfinity(availableSizeMinor)
                    ? availableSizeMinor
                    : Math.Max(0d, (itemsPerLine * GetMinorItemSizeWithSpacing(context)) - _itemSpacing));

            if (firstRealized != null)
            {
                var linesBeforeFirst = firstRealizedItemIndex / itemsPerLine;
                var extentMajorStart = this.MajorStart(firstRealizedLayoutBounds) - GetMajorSize(context, linesBeforeFirst);
                this.SetMajorStart(ref extent, extentMajorStart);
                var remainingLines = (itemsCount / itemsPerLine) - (lastRealizedItemIndex / itemsPerLine) - 1;
                var extentMajorSize = this.MajorEnd(lastRealizedLayoutBounds) - this.MajorStart(extent) + GetMajorSize(context, remainingLines);
                this.SetMajorSize(ref extent, extentMajorSize);
            }
            else
            {
                var majorSize = GetMajorSize(context, (itemsCount / itemsPerLine) + 1);
                this.SetMajorSize(ref extent, majorSize);
#if DEBUG && REPEATER_TRACE
                Logger.TryGet(LogEventLevel.Verbose, "Repeater")?.Log(this, $"{nameof(UniformGridLayout)}: Estimating extent with no realized elements");
#endif
            }
        }
        
#if DEBUG && REPEATER_TRACE
        Logger.TryGet(LogEventLevel.Verbose, "Repeater")?.Log(this, $"{nameof(UniformGridLayout)}: Extent is ({extent.Size}). Based on items per line {itemsPerLine}");
#endif
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
        gridState.EnsureElementSize(availableSize, context, _minItemWidth, _minItemHeight, _itemsStretch, Orientation, _itemSpacing, _maximumRowsOrColumns);

        var desiredSize = GetFlowAlgorithm(context).Measure(
            availableSize,
            context,
            true,
            _itemSpacing,
            _lineSpacing,
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

    private int GetItemsCountPerLine(VirtualizingLayoutContext context, Size availableSize)
    {
        return Math.Max(1,
            (int) Math.Min(_maximumRowsOrColumns,
                (this.Minor(availableSize) + _itemSpacing) / GetMinorItemSizeWithSpacing(context)));
    }

    private double GetMinorItemSizeWithSpacing(VirtualizingLayoutContext context)
    {
        var gridState = (UniformGridLayoutState)context.LayoutState!;
        return (_scrollOrientation == ScrollOrientation.Vertical
            ? gridState.EffectiveItemWidth
            : gridState.EffectiveItemHeight) + _itemSpacing;
    }

    private double GetMajorSize(VirtualizingLayoutContext context, int lineCount)
    {
        var gridState = (UniformGridLayoutState) context.LayoutState!;
        var majorItemSize = _scrollOrientation == ScrollOrientation.Vertical
            ? gridState.EffectiveItemHeight
            : gridState.EffectiveItemWidth;
        var lineSpacing = _lineSpacing;
        return Math.Max(0, ((majorItemSize + lineSpacing) * lineCount) - lineSpacing);
    }

    private double GetMajorItemSizeWithSpacing(VirtualizingLayoutContext context)
    {
        var gridState = (UniformGridLayoutState) context.LayoutState!;
        return (_scrollOrientation == ScrollOrientation.Vertical
            ? gridState.EffectiveItemHeight
            : gridState.EffectiveItemWidth) + _lineSpacing;
    }

    private Rect GetLayoutRectForDataIndex(
        int index,
        int itemsPerLine,
        Rect lastExtent,
        VirtualizingLayoutContext context)
    {
        var lineIndex = index / itemsPerLine;
        var indexInLine = index % itemsPerLine;

        var gridState = (UniformGridLayoutState)context.LayoutState!;
        var bounds = this.MinorMajorRect(
            (indexInLine * GetMinorItemSizeWithSpacing(context)) + this.MinorStart(lastExtent),
            (lineIndex * GetMajorItemSizeWithSpacing(context)) + this.MajorStart(lastExtent),
            _scrollOrientation == ScrollOrientation.Vertical ? gridState.EffectiveItemWidth : gridState.EffectiveItemHeight,
            _scrollOrientation == ScrollOrientation.Vertical ? gridState.EffectiveItemHeight : gridState.EffectiveItemWidth);

        return bounds;
    }

    private static FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context) => ((UniformGridLayoutState)context.LayoutState!).FlowAlgorithm;
}
