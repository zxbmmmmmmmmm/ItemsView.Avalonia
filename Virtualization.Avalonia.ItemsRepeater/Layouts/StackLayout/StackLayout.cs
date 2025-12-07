using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

/// <summary>
/// Represents an <i>attached layout</i> that arranges child elements into a single line that can be
/// oriented horizontally or vertically
/// </summary>
public partial class StackLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates, IOrientationBasedMeasures
{
    protected internal override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        var state = context.LayoutState;

        if (state is not StackLayoutState stackState)
        {
            if (state is not null)
                throw new InvalidOperationException($"{nameof(context.LayoutState)} must derive from {nameof(StackLayoutState)}.");

            // Custom deriving layouts could potentially be stateful.
            // If that is the case, we will just create the base state required by UniformGridLayout ourselves.
            stackState = new StackLayoutState();
        }

        stackState.InitializeForContext(context, this);
    }

    protected internal override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        var stackState = (StackLayoutState)context.LayoutState!;
        stackState.UninitializeForContext(context);
    }

    protected internal override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
    {
        var stackState = (StackLayoutState) context.LayoutState!;
        stackState.OnMeasureStart();

        var desiredSize = GetFlowAlgorithm(context).Measure(
            availableSize,
            context,
            false /*isWrapping*/,
            0 /*minItemsSpacing*/,
            _spacing,
            int.MaxValue /*maxItemsPerLine*/,
            _scrollOrientation,
            DisableVirtualization);

        return desiredSize;
    }

    protected internal override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
    {
        var value = GetFlowAlgorithm(context).Arrange(
            finalSize,
            context,
            false /*isWrapping*/,
            FlowLayoutAlgorithm.LineAlignment.Start);

        return value;
    }

    protected internal override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
    {
        GetFlowAlgorithm(context).OnItemsSourceChanged(source, args, context);

        InvalidateLayout();
    }

    private FlowLayoutAnchorInfo GetAnchorForRealizationRect(Size availableSize, VirtualizingLayoutContext context)
    {
        var anchorIndex = -1;
        var offset = double.NaN;

        var itemsCount = context.ItemsCount;
        if (itemsCount > 0)
        {
            var realizationRect = context.RealizationRect;
            var stackState = (StackLayoutState) context.LayoutState!;
            var lastExtent = stackState.FlowAlgorithm.LastExtent;

            var averageElementSize = GetAverageElementSize(availableSize, context, stackState) + _spacing;
            var realizationWindowStartWithinExtent = this.MajorStart(realizationRect) - this.MajorStart(lastExtent);
            var majorSize = this.MajorSize(lastExtent) is 0
                ? Math.Max(0, (averageElementSize * itemsCount) - _spacing)
                : this.MajorSize(lastExtent);
            if (this.MajorSize(realizationRect) >= 0 &&
                // MajorSize = 0 will account for when a nested repeater is outside the realization rect but still being measured. Also,
                // note that if we are measuring this repeater, then we are already realizing an element to figure out the size, so we could
                // just keep that element alive. It also helps in XYFocus scenarios to have an element realized for XYFocus to find a candidate
                // in the navigating direction.
                realizationWindowStartWithinExtent + this.MajorSize(realizationRect) >= 0 &&
                realizationWindowStartWithinExtent <= majorSize)
            {
                anchorIndex = (int)(realizationWindowStartWithinExtent / averageElementSize);
                offset = (anchorIndex * averageElementSize) + this.MajorStart(lastExtent);
                anchorIndex = Math.Clamp(anchorIndex, 0, itemsCount - 1);
            }
        }

        return new FlowLayoutAnchorInfo
        {
            Index = anchorIndex,
            Offset = offset
        };
    }

    private Rect GetExtent(Size availableSize, VirtualizingLayoutContext context, Control firstRealized,
        int firstRealizedItemIndex, Rect firstRealizedLayoutBounds, Control lastRealized,
        int lastRealizedItemIndex, Rect lastRealizedLayoutBounds)
    {
        var extent = new Rect();

        var itemsCount = context.ItemsCount;
        var stackState = (StackLayoutState) context.LayoutState!;
        var averageElementSize = GetAverageElementSize(availableSize, context, stackState) + _spacing;

        this.SetMinorSize(ref extent, stackState.MaxArrangeBounds);
        this.SetMajorSize(ref extent, Math.Max(0, (itemsCount * averageElementSize) - _spacing));
        if (itemsCount > 0)
        {
            if (firstRealized != null)
            {
                Debug.Assert(lastRealized != null);
                this.SetMajorStart(ref extent, this.MajorStart(firstRealizedLayoutBounds) - (firstRealizedItemIndex * averageElementSize));
                var remainingItems = itemsCount - lastRealizedItemIndex - 1;
                this.SetMajorSize(ref extent, this.MajorEnd(lastRealizedLayoutBounds) - this.MajorStart(extent) + (remainingItems * averageElementSize));
            }
            else
            {
#if DEBUG && REPEATER_TRACE
                Logger.TryGet(LogEventLevel.Verbose, "Repeater")?.Log(this,"{Layout} Estimating extent with no realized elements", LayoutId);
#endif
            }
        }
        else
        {
            Debug.Assert(firstRealizedItemIndex == -1);
            Debug.Assert(lastRealizedItemIndex == -1);
        }

#if DEBUG && REPEATER_TRACE
        Logger.TryGet(LogEventLevel.Verbose, "Repeater")?.Log(this,"{Layout}: Extent is {Extent} based on average {Avg}",
            LayoutId, extent, averageElementSize);
#endif
        return extent;
    }

    private void OnElementMeasured(Control element, int index, Size availableSize,
        Size measureSize, Size desiredSize, Size provisionalArrangeSize,
        VirtualizingLayoutContext context)
    {
        var stackState = (StackLayoutState) context.LayoutState!;
        stackState.OnElementMeasured(
            index,
            this.Major(provisionalArrangeSize),
            this.Minor(provisionalArrangeSize));
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, VirtualizingLayoutContext context) => availableSize;

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize, VirtualizingLayoutContext context)
    {
        var measureSizeMinor = this.Minor(measureSize);
        return this.MinorMajorSize(
            !double.IsInfinity(measureSizeMinor) ?
                Math.Max(measureSizeMinor, this.Minor(desiredSize)) :
                this.Minor(desiredSize),
            this.Major(desiredSize));
    }

    bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace, VirtualizingLayoutContext context) => true;

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(Size availableSize, VirtualizingLayoutContext context) =>
        GetAnchorForRealizationRect(availableSize, context);

    int IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorIndexForTargetElement(int targetIndex, Size availableSize, VirtualizingLayoutContext context)
    {
        var index = -1;
        var itemsCount = context.ItemsCount;

        if (targetIndex >= 0 && targetIndex < itemsCount)
            index = targetIndex;

        return index;
        /*
        double offset = double.NaN;
        int index = -1;
        int itemsCount = context.ItemsCount;

        if (targetIndex >= 0 && targetIndex < itemsCount)
        {
            index = targetIndex;
            var stackState = (StackLayoutState) context.LayoutState!;
            double averageElementSize = GetAverageElementSize(availableSize, context, stackState) + _itemSpacing;
            offset = (index * averageElementSize) + this.MajorStart(stackState.FlowAlgorithm.LastExtent);
        }

        return new FlowLayoutAnchorInfo { Index = index, Offset = offset };
        */
    }

    Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(Size availableSize, VirtualizingLayoutContext context, 
        Control firstRealized, int firstRealizedIndex, Rect firstRealizedLayoutBounds, Control lastRealized, 
        int lastRealizedItemIndex, Rect lastRealizedLayoutBounds)
    {
        return GetExtent(availableSize, context, firstRealized,
            firstRealizedIndex, firstRealizedLayoutBounds,
            lastRealized, lastRealizedItemIndex, lastRealizedLayoutBounds);
    }
    
    void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(Control element, int index, Size availableSize, 
        Size measureSize, Size desiredSize, Size provisionalArrangeSize, VirtualizingLayoutContext context)
    {
        OnElementMeasured(element, index, availableSize, measureSize, desiredSize,
            provisionalArrangeSize, context);
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(int startIndex, int countInLine,
        double lineSize, VirtualizingLayoutContext context)
    {
    }

    private static double GetAverageElementSize(Size availableSize, VirtualizingLayoutContext context, StackLayoutState state)
    {
        var averageElementSize = 0d;
        if (context.ItemsCount > 0)
        {
            if (state.TotalElementsMeasured == 0)
            {
                var tmpElement = context.GetOrCreateElementAt(0,
                    ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle);
                state.FlowAlgorithm.MeasureElement(tmpElement, 0, availableSize, context);
                context.RecycleElement(tmpElement);
            }

            Debug.Assert(state.TotalElementsMeasured > 0);
            averageElementSize = Math.Round(state.TotalElementSize / state.TotalElementsMeasured);
        }

        return averageElementSize;
    }

    private static FlowLayoutAlgorithm GetFlowAlgorithm(VirtualizingLayoutContext context) => ((StackLayoutState) context.LayoutState!).FlowAlgorithm;

    // !!! WARNING !!!
    // Any storage here needs to be related to layout configuration. 
    // layout specific state needs to be stored in StackLayoutState.
}
