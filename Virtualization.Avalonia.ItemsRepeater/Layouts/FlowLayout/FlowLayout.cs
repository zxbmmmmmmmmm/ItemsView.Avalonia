using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

public sealed partial class FlowLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates, IOrientationBasedMeasures
{
    /// <inheritdoc />
    protected internal override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        var state = context.LayoutState;

        if (state is not FlowLayoutState flowState)
        {
            if (state is not null)
                throw new InvalidOperationException($"{nameof(context.LayoutState)} must derive from {nameof(FlowLayoutState)}.");

            // Custom deriving layouts could potentially be stateful.
            // If that is the case, we will just create the base state required by UniformGridLayout ourselves.
            flowState = new FlowLayoutState();
        }

        flowState.InitializeForContext(context, this);
    }

    /// <inheritdoc />
    protected internal override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        var stackState = context.LayoutState as FlowLayoutState;
        stackState?.UninitializeForContext(context);
    }

    /// <inheritdoc />
    protected internal override void OnItemsChangedCore(VirtualizingLayoutContext context, object? source, NotifyCollectionChangedEventArgs args)
    {
        GetFlowAlgorithm(context).OnItemsSourceChanged(source, args, context);
        InvalidateLayout();
    }

    /// <inheritdoc />
    protected internal override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        var state = (FlowLayoutState) context.LayoutState!;
        state.OnMeasureStart();

        var desiredSize = GetFlowAlgorithm(context).Measure(
            availableSize,
            context,
            false /*isWrapping*/,
            _itemSpacing,
            _lineSpacing,
            int.MaxValue /*maxItemsPerLine*/,
            ScrollOrientation.Vertical,
            false);

        return desiredSize;
    }

    /// <inheritdoc />
    protected internal override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var value = GetFlowAlgorithm(context).Arrange(
            finalSize,
            context,
            true /*isWrapping*/,
            FlowLayoutAlgorithm.LineAlignment.Start);

        return value;
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, VirtualizingLayoutContext context)
    {
        this.SetMajor(ref availableSize, _lineHeight);
        return availableSize;
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize, VirtualizingLayoutContext context)
    {
        this.SetMajor(ref desiredSize, _lineHeight);
        return desiredSize;
    }

    bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace, VirtualizingLayoutContext context)
    {
        var breakLine = remainingSpace < 0;
        if (breakLine)
        {
            var state = (FlowLayoutState) context.LayoutState!;
            state.OnBreakLine();
        }
        return breakLine;
    }

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(Size availableSize, VirtualizingLayoutContext context)
    {
        var anchorIndex = -1;
        var offset = double.NaN;

        var itemsCount = context.ItemsCount;
        if (itemsCount > 0)
        {
            var lineSize = _lineHeight + _lineSpacing;
            var realizationRect = context.RealizationRect;
            var lastExtent = GetFlowAlgorithm(context).LastExtent;

            var averageItemsPerLine = GetAverageCountInLine(availableSize, context);
            Debug.Assert(averageItemsPerLine != 0);

            var majorSize = this.MajorSize(lastExtent);
            if (majorSize is 0)
                majorSize = (Math.Ceiling(itemsCount / averageItemsPerLine) * lineSize) - _lineSpacing;

            if (this.MajorSize(realizationRect) > 0)
            {
                var realizationWindowStartWithinExtent = this.MajorStart(realizationRect) - this.MajorStart(lastExtent);

                var overlaps =
                    realizationWindowStartWithinExtent + this.MajorSize(realizationRect) >= 0 &&
                    realizationWindowStartWithinExtent <= majorSize;
                if (overlaps)
                {
                    var o = Math.Max(0, realizationWindowStartWithinExtent + _lineSpacing);
                    var anchorLineIndex = (int) (o / lineSize);
                    anchorIndex = Math.Clamp((int) (anchorLineIndex * averageItemsPerLine), 0, itemsCount - 1);
                    offset = (anchorLineIndex * lineSize) + this.MajorStart(lastExtent);
                }
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
        var itemsCount = context.ItemsCount;

        if (targetIndex >= 0 && targetIndex < itemsCount)
            index = targetIndex;

        return index;
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
        Rect extent = default;

        var itemsCount = context.ItemsCount;
        if (itemsCount > 0)
        {
            var availableSizeMinor = this.Minor(availableSize);
            var lineSize = _lineHeight + _lineSpacing;
            var averageItemsPerLine = GetAverageCountInLine(availableSize, context);
            Debug.Assert(averageItemsPerLine != 0);

            var estimatedLines = (int) ((itemsCount / averageItemsPerLine) + 1);

            this.SetMinorSize(
                ref extent,
                !double.IsInfinity(availableSizeMinor)
                    ? availableSizeMinor
                    : Math.Max(0d, itemsCount * (_itemSpacing + _lineHeight)));

            if (firstRealized != null)
            {
                var linesBeforeFirst = (int) (firstRealizedItemIndex / averageItemsPerLine);
                var extentMajorStart = this.MajorStart(firstRealizedLayoutBounds) - (linesBeforeFirst * lineSize);
                this.SetMajorStart(ref extent, extentMajorStart);
                var remainingItems = itemsCount - lastRealizedItemIndex - 1;
                var remainingLinesAfterLast = (int) (remainingItems / averageItemsPerLine);
                var extentMajorSize = this.MajorEnd(lastRealizedLayoutBounds) - this.MajorStart(extent) + (remainingLinesAfterLast * lineSize);
                this.SetMajorSize(ref extent, extentMajorSize);

                // If the available size is infinite, we will have realized all the items in one line.
                // In that case, the extent in the non virtualizing direction should be based on the
                // right/bottom of the last realized element.
            }
            else
            {
                var majorSize = Math.Max(0, (estimatedLines * lineSize) - _lineSpacing);
                this.SetMajorSize(ref extent, majorSize);
                // We dont have anything realized. make an educated guess.
            }
        }
        else
        {
            Debug.Assert(firstRealizedItemIndex == -1);
            Debug.Assert(lastRealizedItemIndex == -1);
        }

        return extent;
    }

    private double GetAverageCountInLine(Size availableSize, VirtualizingLayoutContext context)
    {
        var flowState = (FlowLayoutState) context.LayoutState!;
        var realizedCount = (double)flowState.TotalElementsMeasured;
        if (realizedCount is not 0)
            return realizedCount / flowState.TotalLines;

        Debug.Assert(context.ItemsCount > 0);

        var tmpElement = context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle);
        var desiredSize = GetFlowAlgorithm(context).MeasureElement(tmpElement, 0, availableSize, context);
        context.RecycleElement(tmpElement);

        var avgCountInLine = Math.Max(1, this.Minor(availableSize) / this.Minor(desiredSize));

        return avgCountInLine;
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(
        Control element,
        int index,
        Size availableSize,
        Size measureSize,
        Size desiredSize,
        Size provisionalArrangeSize,
        VirtualizingLayoutContext context)
    {
        var flowState = (FlowLayoutState) context.LayoutState!;
        flowState.OnElementMeasured(index, this.Minor(provisionalArrangeSize), element.GetHashCode());
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(int startIndex, int countInLine, double lineSize, VirtualizingLayoutContext context)
    {
    }

    private static FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context) => ((FlowLayoutState) context.LayoutState!).FlowAlgorithm;
}
