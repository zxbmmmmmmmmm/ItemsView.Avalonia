using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

public sealed partial class FlowLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates
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
        var desiredSize = GetFlowAlgorithm(context).Measure(
            availableSize,
            context,
            false /*isWrapping*/,
            MinItemSpacing /*minItemsSpacing*/,
            LineSpacing,
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

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, VirtualizingLayoutContext context) => availableSize;

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize, VirtualizingLayoutContext context) => desiredSize;

    bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace) => remainingSpace < 0;

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(Size availableSize, VirtualizingLayoutContext context)
    {
        int anchorIndex = -1;
        double offset = double.NaN;

        int itemsCount = context.ItemsCount;
        if (itemsCount > 0)
        {
            var lineSpacing = LineSpacing;
            Rect realizationRect = context.RealizationRect;
            Rect lastExtent = GetFlowAlgorithm(context).LastExtent;
            var lineSize = LineHeight + LineSpacing;

            double averageItemsPerLine = GetAverageCountInLine(availableSize, context);
            Debug.Assert(averageItemsPerLine != 0);

            double extentMajorSize = lastExtent.Height == 0
                ? (Math.Ceiling(itemsCount / averageItemsPerLine) * lineSize) - lineSpacing
                : lastExtent.Height;

            if (realizationRect.Height > 0)
            {
                Rect extentRect = new(lastExtent.X, lastExtent.Y, availableSize.Width, extentMajorSize);

                var overlaps =
                    realizationRect.Bottom >= extentRect.Y &&
                    realizationRect.Y <= extentRect.Bottom;
                if (overlaps)
                {
                    var realizationWindowStartWithinExtent = realizationRect.Y - lastExtent.Y;
                    var o = Math.Max(0, realizationWindowStartWithinExtent + LineSpacing);
                    var anchorLineIndex = (int) (o / lineSize);
                    anchorIndex = Math.Clamp((int) (anchorLineIndex * averageItemsPerLine), 0, itemsCount - 1);
                    offset = (anchorLineIndex * lineSize) + lastExtent.Y;
                }
            }
        }

        return new FlowLayoutAnchorInfo
        {
            Index = anchorIndex,
            Offset = offset
        };
    }

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(int targetIndex, Size availableSize, VirtualizingLayoutContext context)
    {
        double offset = double.NaN;
        int index = -1;
        int itemsCount = context.ItemsCount;

        if (targetIndex >= 0 && targetIndex < itemsCount)
        {
            index = targetIndex;
            var lineSize = LineHeight + LineSpacing;
            double averageItemsPerLine = GetAverageCountInLine(availableSize, context);
            int lineIndex = (int) (targetIndex / averageItemsPerLine);
            offset = (lineIndex * lineSize) + GetFlowAlgorithm(context).LastExtent.Y;
        }

        return new FlowLayoutAnchorInfo { Index = index, Offset = offset };
    }

    Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(Size availableSize, VirtualizingLayoutContext context, Control firstRealized, int firstRealizedItemIndex, Rect firstRealizedLayoutBounds, Control lastRealized, int lastRealizedItemIndex, Rect lastRealizedLayoutBounds)
    {
        Rect extent = default;

        int itemsCount = context.ItemsCount;
        if (itemsCount > 0)
        {
            double availableSizeMinor = availableSize.Width;
            var lineSize = LineHeight + LineSpacing;
            double averageItemsPerLine = GetAverageCountInLine(availableSize, context);

            Debug.Assert(averageItemsPerLine != 0);
            if (firstRealized != null)
            {
                Debug.Assert(lastRealized != null);
                int linesBeforeFirst = (int) (firstRealizedItemIndex / averageItemsPerLine);
                double extentMajorStart = firstRealizedLayoutBounds.Y - (linesBeforeFirst * lineSize);
                extent = extent.WithHeight(extentMajorStart);
                int remainingItems = itemsCount - lastRealizedItemIndex - 1;
                int remainingLinesAfterLast = (int) (remainingItems / averageItemsPerLine);
                double extentMajorSize = lastRealizedLayoutBounds.Y + lastRealizedLayoutBounds.Height -
                    extent.Y + (remainingLinesAfterLast * lineSize);
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
                extent = !double.IsInfinity(availableSizeMinor)
                    ? new Rect(0, 0, availableSizeMinor, Math.Max(0, (numLines * lineSize) - lineSpacing))
                    : new Rect(0, 0,
                        Math.Max(0, ((LineHeight + minItemSpacing) * itemsCount) - minItemSpacing),
                        Math.Max(0, lineSize - lineSpacing));
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

    private static double GetAverageCountInLine(Size availableSize, VirtualizingLayoutContext context)
    {
        var flowState = (FlowLayoutState) context.LayoutState!;
        if (flowState.AverageElementSize is var avg and not 0)
            return Math.Max(1, Math.Floor(availableSize.Width / avg));

        Debug.Assert(context.ItemsCount > 0);

        var tmpElement = context.GetOrCreateElementAt(0, ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle);
        var desiredSize = GetFlowAlgorithm(context).MeasureElement(tmpElement, 0, availableSize, context);
        context.RecycleElement(tmpElement);

        var avgCountInLine = Math.Max(1, availableSize.Width / desiredSize.Width);

        return avgCountInLine;
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(Control element, int index, Size availableSize, Size measureSize, Size desiredSize, Size provisionalArrangeSize, VirtualizingLayoutContext context)
    {
        var flowState = (FlowLayoutState) context.LayoutState!;
        flowState.OnElementMeasured(index, provisionalArrangeSize.Width);
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(int startIndex, int countInLine, double lineSize, VirtualizingLayoutContext context)
    {
    }

    private static FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context) => ((FlowLayoutState) context.LayoutState!).FlowAlgorithm;

    private void InvalidateLayout() => InvalidateMeasure();
}
