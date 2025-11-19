using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using PropertyGenerator.Avalonia;

namespace Virtualization.Avalonia.Layouts;

public sealed partial class FlowLayout : VirtualizingLayout, IFlowLayoutAlgorithmDelegates
{

    /// <inheritdoc />
    protected internal override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        context.LayoutState = new FlowLayoutState(context);
    }

    /// <inheritdoc />
    protected internal override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        context.LayoutState = null;
    }

    /// <inheritdoc />
    protected internal override void OnItemsChangedCore(VirtualizingLayoutContext context, object? source, NotifyCollectionChangedEventArgs args)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected internal override Size MeasureOverride(VirtualizingLayoutContext context, Size parentMeasure)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    protected internal override Size ArrangeOverride(VirtualizingLayoutContext context, Size parentMeasure)
    {
        throw new NotImplementedException();
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetMeasureSize(int index, Size availableSize, VirtualizingLayoutContext context)
    {
        throw new NotImplementedException();
    }

    Size IFlowLayoutAlgorithmDelegates.Algorithm_GetProvisionalArrangeSize(int index, Size measureSize, Size desiredSize, VirtualizingLayoutContext context)
    {
        throw new NotImplementedException();
    }

    bool IFlowLayoutAlgorithmDelegates.Algorithm_ShouldBreakLine(int index, double remainingSpace)
    {
        throw new NotImplementedException();
    }

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForRealizationRect(Size availableSize, VirtualizingLayoutContext context)
    {
        throw new NotImplementedException();
    }

    FlowLayoutAnchorInfo IFlowLayoutAlgorithmDelegates.Algorithm_GetAnchorForTargetElement(int targetIndex, Size availableSize, VirtualizingLayoutContext ccontext)
    {
        throw new NotImplementedException();
    }

    Rect IFlowLayoutAlgorithmDelegates.Algorithm_GetExtent(Size availableSize, VirtualizingLayoutContext context, Control firstRealized, int firstRealizedItemIndex, Rect firstRealizedLayoutBounds, Control lastRealized, int lastRealizedItemIndex, Rect lastRealizedLayoutBounds)
    {
        throw new NotImplementedException();
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnElementMeasured(Control element, int index, Size availableSize, Size measureSize, Size desiredSize, Size provisionalArrangeSize, VirtualizingLayoutContext context)
    {
        throw new NotImplementedException();
    }

    void IFlowLayoutAlgorithmDelegates.Algorithm_OnLineArranged(int startIndex, int countInLine, double lineSize, VirtualizingLayoutContext context)
    {
        throw new NotImplementedException();
    }
}
