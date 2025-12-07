// This source file is adapted from the WinUI project.
// (https://github.com/microsoft/microsoft-ui-xaml)
//
// Licensed to The Avalonia Project under MIT License, courtesy of The .NET Foundation.

using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Virtualization.Avalonia.Layouts;

/// <summary>
/// Represents the state of a <see cref="UniformGridLayout"/>.
/// </summary>
public class UniformGridLayoutState
{
    // We need to measure the element at index 0 to know what size to measure all other items. 
    // If FlowlayoutAlgorithm has already realized element 0 then we can use that. 
    // If it does not, then we need to do context.GetElement(0) at which point we have requested an element and are on point to clear it.
    // If we are responsible for clearing element 0 we keep _cachedFirstElement valid. 
    // If we are not (because FlowLayoutAlgorithm is holding it for us) then we just null out this field and use the one from FlowLayoutAlgorithm.
    private Control? _cachedFirstElement;

    internal FlowLayoutAlgorithm FlowAlgorithm { get; } = new FlowLayoutAlgorithm();

    internal double EffectiveItemWidth { get; private set; }

    internal double EffectiveItemHeight { get; private set; }

    internal void InitializeForContext(VirtualizingLayoutContext context, IFlowLayoutAlgorithmDelegates callbacks)
    {
        FlowAlgorithm.InitializeForContext(context, callbacks);
        context.LayoutState = this;
    }

    internal void UninitializeForContext(VirtualizingLayoutContext context)
    {
        FlowAlgorithm.UninitializeForContext(context);

        if (_cachedFirstElement != null)
        {
            context.RecycleElement(_cachedFirstElement);
        }
    }

    internal void EnsureElementSize(
        Size availableSize,
        VirtualizingLayoutContext context,
        double layoutItemWidth,
        double layoutItemHeight,
        UniformGridLayoutItemsStretch stretch,
        Orientation orientation,
        double itemSpacing,
        int maxItemsPerLine)
    {
        if (maxItemsPerLine is 0)
            maxItemsPerLine = 1;

        if (context.ItemsCount > 0)
        {
            // If the first element is realized we don't need to cache it or to get it from the context
            if (FlowAlgorithm.GetElementIfRealized(0) is { } realizedElement)
            {
                realizedElement.Measure(availableSize);
                SetSize(realizedElement, layoutItemWidth, layoutItemHeight, availableSize, stretch, orientation, itemSpacing, maxItemsPerLine);
                _cachedFirstElement = null;
            }
            else
            {
                // we only cache if we aren't realizing it
                _cachedFirstElement ??= context.GetOrCreateElementAt(
                    0,
                    ElementRealizationOptions.ForceCreate | ElementRealizationOptions.SuppressAutoRecycle); // expensive

                _cachedFirstElement.Measure(availableSize);

                SetSize(_cachedFirstElement, layoutItemWidth, layoutItemHeight, availableSize, stretch, orientation, itemSpacing, maxItemsPerLine);

                // See if we can move ownership to the flow algorithm. If we can, we do not need a local cache.
                var added = FlowAlgorithm.TryAddElement0(_cachedFirstElement);
                if (added)
                    _cachedFirstElement = null;
            }
        }
    }

    private void SetSize(
        Layoutable element,
        double layoutItemWidth,
        double layoutItemHeight,
        Size availableSize,
        UniformGridLayoutItemsStretch stretch,
        Orientation orientation,
        double itemSpacing,
        int maxItemsPerLine)
    {
        if (maxItemsPerLine is 0)
            maxItemsPerLine = 1;

        EffectiveItemWidth = double.IsNaN(layoutItemWidth) ? element.DesiredSize.Width : layoutItemWidth;
        EffectiveItemHeight = double.IsNaN(layoutItemHeight) ? element.DesiredSize.Height : layoutItemHeight;

        var availableSizeMinor = orientation == Orientation.Horizontal ? availableSize.Width : availableSize.Height;

        var itemSizeMinor = orientation == Orientation.Horizontal ? EffectiveItemWidth : EffectiveItemHeight;

        double extraMinorPixelsForEachItem = 0.0;
        if (!double.IsInfinity(availableSizeMinor))
        {
            var numItemsPerColumn = (int)Math.Min(
                maxItemsPerLine,
                Math.Max(1.0, availableSizeMinor / (itemSizeMinor + itemSpacing)));
            var usedSpace = (numItemsPerColumn * (itemSizeMinor + itemSpacing)) - itemSpacing;
            var remainingSpace = availableSizeMinor - usedSpace;
            extraMinorPixelsForEachItem = (int)(remainingSpace / numItemsPerColumn);
        }

        switch (stretch)
        {
            case UniformGridLayoutItemsStretch.Fill:
            {
                if (orientation == Orientation.Horizontal)
                    EffectiveItemWidth += extraMinorPixelsForEachItem;
                else
                    EffectiveItemHeight += extraMinorPixelsForEachItem;
                break;
            }
            case UniformGridLayoutItemsStretch.Uniform:
            {
                var itemSizeMajor = orientation == Orientation.Horizontal ? EffectiveItemHeight : EffectiveItemWidth;
                var extraMajorPixelsForEachItem = itemSizeMajor * (extraMinorPixelsForEachItem / itemSizeMinor);
                if (orientation == Orientation.Horizontal)
                {
                    EffectiveItemWidth += extraMinorPixelsForEachItem;
                    EffectiveItemHeight += extraMajorPixelsForEachItem;
                }
                else
                {
                    EffectiveItemHeight += extraMinorPixelsForEachItem;
                    EffectiveItemWidth += extraMajorPixelsForEachItem;
                }

                break;
            }
        }
    }

    internal void EnsureFirstElementOwnership(VirtualizingLayoutContext context)
    {
        if (_cachedFirstElement != null && FlowAlgorithm.GetElementIfRealized(0) != null)
        {
            // We created the element, but then flowlayout algorithm took ownership, so we can clear it and
            // let flowlayout algorithm do its thing.
            context.RecycleElement(_cachedFirstElement);
            _cachedFirstElement = null;
        }
    }

    internal void ClearElementOnDataSourceChange(
        VirtualizingLayoutContext context,
        NotifyCollectionChangedEventArgs args)
    {
        if (_cachedFirstElement is null)
            return;
        var shouldClear = args.Action switch
        {
            NotifyCollectionChangedAction.Add => args.NewStartingIndex == 0,
            NotifyCollectionChangedAction.Replace => args.NewStartingIndex == 0 || args.OldStartingIndex == 0,
            NotifyCollectionChangedAction.Remove => args.OldStartingIndex == 0,
            NotifyCollectionChangedAction.Reset => true,
            NotifyCollectionChangedAction.Move => throw new NotImplementedException(),
            _ => false
        };

        if (shouldClear)
        {
            context.RecycleElement(_cachedFirstElement);
            _cachedFirstElement = null;
        }
    }
}
