using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using PropertyGenerator.Avalonia;

namespace Virtualization.Avalonia.Layouts;

public sealed partial class FlowLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates
{

    /// <inheritdoc />
    protected internal override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        var state = context.LayoutState;
        FlowLayoutState? flowState = null;
        if (state != null)
            flowState = GetAsFlowState(state);

        if (flowState == null)
        {
            if (state != null)
                throw new InvalidOperationException("LayoutState must derive from StackLayoutState.");

            // Custom deriving layouts could potentially be stateful.
            // If that is the case, we will just create the base state required by UniformGridLayout ourselves.
            flowState = new FlowLayoutState();
        }

        flowState.InitializeForContext(context, this);
    }

    /// <inheritdoc />
    protected internal override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        var stackState = GetAsFlowState(context.LayoutState);
        stackState?.UninitializeForContext(context);
    }

    /// <inheritdoc />
    protected internal override void OnItemsChangedCore(VirtualizingLayoutContext context, object? source, NotifyCollectionChangedEventArgs args)
    {
        if (context.LayoutState != null)
        {
            var flow = GetAsFlowState(context.LayoutState).FlowAlgorithm;
            flow.OnItemsSourceChanged(source, args, context);
        }

        InvalidateLayout();
    }

    /// <inheritdoc />
    protected internal override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        if (context.LayoutState == null)
            return default;


        var desiredSize = GetFlowAlgorithm(context).Measure(
            availableSize, context, false /*isWrapping*/, MinItemSpacing/*minItemsSpacing*/,
            MinItemSpacing, int.MaxValue /*maxItemsPerLine*/,
            ScrollOrientation.Vertical, false);

        return desiredSize;
    }

    /// <inheritdoc />
    protected internal override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        if (context.LayoutState == null)
            return default;

        var value = GetFlowAlgorithm(context).Arrange(
            finalSize, context, true /*isWrapping*/,
            FlowLayoutAlgorithm.LineAlignment.Start);

        return value;
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, VirtualizingLayoutContext context)
    {
        return availableSize;
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize, VirtualizingLayoutContext context)
    {
        return desiredSize;
    }

    bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace)
    {
        return remainingSpace <= 0;
    }

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(Size availableSize, VirtualizingLayoutContext context)
    {
        int anchorIndex = -1;
        double offset = double.NaN;

        int itemsCount = context.ItemCount;
        if (itemsCount > 0)
        {
            Rect realizationRect = context.RealizationRect;
            var state = context.LayoutState;
            var flowState = GetAsFlowState(state);
            Rect lastExtent = flowState.FlowAlgorithm.LastExtent;

            double averageItemsPerLine = 0;
            double averageLineSize = GetAverageLineInfo(availableSize, context, flowState, ref averageItemsPerLine);
            Debug.Assert(averageItemsPerLine != 0);

            double extentMajorSize = lastExtent.Height == 0
                ? (itemsCount / averageItemsPerLine) * averageLineSize
                : lastExtent.Height;

            if (itemsCount > 0 && realizationRect.Height > 0)
            {
                Rect extentRect = new(lastExtent.X, lastExtent.Y, availableSize.Width, extentMajorSize);

                bool overlaps =
                    (realizationRect.Y + realizationRect.Height) >= extentRect.Y &&
                    realizationRect.Y <= (extentRect.Y + extentRect.Height);

                if (overlaps)
                {
                    double realizationWindowStartWithExtent = realizationRect.Y - lastExtent.Y;
                    int lineIndex = Math.Max(0, (int) (realizationWindowStartWithExtent / averageLineSize));
                    anchorIndex = (int) (lineIndex * averageItemsPerLine);

                    // Clamp it to be within valid range
                    anchorIndex = Math.Clamp(anchorIndex, 0, itemsCount - 1);
                    offset = lineIndex * averageLineSize + lastExtent.Y;
                }
            }
        }

        return new FlowLayoutAnchorInfo { Index = anchorIndex, Offset = offset };
    }

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(int targetIndex, Size availableSize, VirtualizingLayoutContext context)
    {
        double offset = double.NaN;
        int index = -1;
        int itemsCount = context.ItemCount;

        if (targetIndex >= 0 && targetIndex < itemsCount)
        {
            index = targetIndex;
            var state = context.LayoutState;
            var flowState = GetAsFlowState(state);
            double averageItemsPerLine = 0;
            double averageLineSize = GetAverageLineInfo(availableSize, context, flowState, ref averageItemsPerLine) + LineSpacing;
            int lineIndex = (int) (targetIndex / averageItemsPerLine);
            offset = lineIndex * averageLineSize + flowState.FlowAlgorithm.LastExtent.Y;
        }

        return new FlowLayoutAnchorInfo { Index = index, Offset = offset };
    }

    Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(Size availableSize, VirtualizingLayoutContext context, Control firstRealized, int firstRealizedItemIndex, Rect firstRealizedLayoutBounds, Control lastRealized, int lastRealizedItemIndex, Rect lastRealizedLayoutBounds)
    {
        Rect extent = default;

        int itemsCount = context.ItemCount;
        if (itemsCount > 0)
        {
            double availableSizeMinor = availableSize.Width;
            var state = context.LayoutState;
            var flowState = GetAsFlowState(state);
            double averageItemsPerLine = 0;
            double averageLineSize = GetAverageLineInfo(availableSize, context, flowState, ref averageItemsPerLine);

            Debug.Assert(averageItemsPerLine != 0);
            if (firstRealized != null)
            {
                Debug.Assert(lastRealized != null);
                int linesBeforeFirst = (int) (firstRealizedItemIndex / averageItemsPerLine);
                double extentMajorStart = firstRealizedLayoutBounds.Y - linesBeforeFirst * averageLineSize;
                extent = extent.WithHeight(extentMajorStart);
                int remainingItems = itemsCount - lastRealizedItemIndex - 1;
                int remainingLinesAfterLast = (int) (remainingItems / averageItemsPerLine);
                double extentMajorSize = lastRealizedLayoutBounds.Y + lastRealizedLayoutBounds.Height -
                    extent.Y + remainingLinesAfterLast * averageLineSize;
                extent = extent.WithHeight(extentMajorSize);

                // If the available size is infinite, we will have realized all the items in one line.
                // In that case, the extent in the non virtualizing direction should be based on the
                // right/bottom of the last realized element.
                extent = extent.WithWidth(!double.IsInfinity(availableSizeMinor) ?
                    availableSizeMinor : Math.Max(0, lastRealizedLayoutBounds.X + lastRealizedLayoutBounds.Width));
            }
            else
            {
                var lineSpacing = LineSpacing;
                var minItemSpacing = MinItemSpacing;
                // We dont have anything realized. make an educated guess.
                int numLines = (int) Math.Ceiling(itemsCount / averageItemsPerLine);
                extent = !double.IsInfinity(availableSizeMinor) ?
                    new Rect(0, 0, availableSizeMinor, Math.Max(0, numLines * averageLineSize - lineSpacing))
                    :
                    new Rect(0, 0,
                    Math.Max(0, (LineHeight + minItemSpacing) * itemsCount - minItemSpacing),
                    Math.Max(0, averageLineSize - lineSpacing));
                //REPEATER_TRACE_INFO(L"%*s: \tEstimating extent with no realized elements. \n", winrt::get_self<VirtualizingLayoutContext>(context)->Indent(), LayoutId().data());
            }

            //REPEATER_TRACE_INFO(L"%*s: \tExtent is {%.0f,%.0f}. Based on average line size {%.0f} and average items per line {%.0f}. \n",
            //winrt::get_self<VirtualizingLayoutContext>(context)->Indent(), LayoutId().data(), extent.Width, extent.Height, averageLineSize, averageItemsPerLine);
        }
        else
        {
            Debug.Assert(firstRealizedItemIndex == -1);
            Debug.Assert(lastRealizedItemIndex == -1);
            //        REPEATER_TRACE_INFO(L"%*s: \tExtent is {%.0f,%.0f}. ItemCount is 0 \n",
            //winrt::get_self<VirtualizingLayoutContext>(context)->Indent(), LayoutId().data(), extent.Width, extent.Height);
        }

        return extent;
    }

    private double GetAverageLineInfo(Size availableSize, VirtualizingLayoutContext context,
    FlowLayoutState flowState, ref double avgCountInLine)
    {
        // default to 1 item per line with 0 size
        double avgLineSize = 0;
        avgCountInLine = 1;

        Debug.Assert(context.ItemCount > 0);

        var tmpElement = context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle);
        var desiredSize = flowState.FlowAlgorithm.MeasureElement(tmpElement, 0, availableSize, context);
        context.RecycleElement(tmpElement);

        int estimatedCountInLine = Math.Max(1, (int) (availableSize.Height / availableSize.Width));


        avgCountInLine = Math.Max(1, availableSize.Width / desiredSize.Width);

        return desiredSize.Height + LineSpacing;
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(Control element, int index, Size availableSize, Size measureSize, Size desiredSize, Size provisionalArrangeSize, VirtualizingLayoutContext context)
    {

    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(int startIndex, int countInLine, double lineSize, VirtualizingLayoutContext context)
    {

    }

    private FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context) =>
        GetAsFlowState(context.LayoutState!)!.FlowAlgorithm;
    private FlowLayoutState? GetAsFlowState(object state) =>
        state as FlowLayoutState;

    private void InvalidateLayout() => InvalidateMeasure();
}
