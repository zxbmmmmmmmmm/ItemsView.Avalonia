// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.

using System.Collections.Specialized;
using Avalonia;
using Avalonia.Layout;
using PropertyGenerator.Avalonia;

namespace ItemsView.Avalonia.Layouts.FlowLayout;

public sealed partial class FlowLayout : VirtualizingLayout
{
    [GeneratedStyledProperty(0d)]
    public partial double LineSpacing { get; set; }

    [GeneratedStyledProperty(0d)]
    public partial double MinItemSpacing { get; set; }

    [GeneratedStyledProperty(200d)]
    public partial double LineHeight { get; set; }

    [GeneratedStyledProperty(FlowLayoutItemsStretch.Stretch)]
    public partial FlowLayoutItemsStretch ItemsStretch { get; set; }

    partial void OnLineSpacingPropertyChanged(AvaloniaPropertyChangedEventArgs e) => OnLineHeightPropertyChanged(e);

    partial void OnMinItemSpacingPropertyChanged(AvaloniaPropertyChangedEventArgs e) => OnLineHeightPropertyChanged(e);

    partial void OnItemsStretchPropertyChanged(AvaloniaPropertyChangedEventArgs e) => OnLineHeightPropertyChanged(e);

    partial void OnLineHeightPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        InvalidateMeasure();
        InvalidateArrange();
    }

    /// <inheritdoc />
    protected override void InitializeForContextCore(VirtualizingLayoutContext context)
    {
        context.LayoutState = new FlowLayoutState(context);
        base.InitializeForContextCore(context);
    }

    /// <inheritdoc />
    protected override void UninitializeForContextCore(VirtualizingLayoutContext context)
    {
        context.LayoutState = null;
        base.UninitializeForContextCore(context);
    }

    /// <inheritdoc />
    protected override void OnItemsChangedCore(VirtualizingLayoutContext context, object source, NotifyCollectionChangedEventArgs args)
    {
        var state = (FlowLayoutState)context.LayoutState;

        switch (args.Action)
        {
            case NotifyCollectionChangedAction.Add:
                state.ClearMeasureFromIndex(args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Move:
                var minIndex = Math.Min(args.NewStartingIndex, args.OldStartingIndex);
                state.ClearMeasureFromIndex(minIndex);
                state.RecycleElementAt(args.OldStartingIndex);
                state.RecycleElementAt(args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Remove:
                state.ClearMeasureFromIndex(args.OldStartingIndex);
                break;
            case NotifyCollectionChangedAction.Replace:
                state.ClearMeasureFromIndex(args.NewStartingIndex);
                state.RecycleElementAt(args.NewStartingIndex);
                break;
            case NotifyCollectionChangedAction.Reset:
                state.Clear();
                break;
        }

        base.OnItemsChangedCore(context, source, args);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(VirtualizingLayoutContext context, Size parentMeasure)
    {
        var spacingMeasure = new Size(MinItemSpacing, LineSpacing);

        var state = (FlowLayoutState)context.LayoutState;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (state.AvailableWidth != parentMeasure.Width
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            || spacingMeasure != state.Spacing)
        {
            state.ClearMeasure();
            state.AvailableWidth = parentMeasure.Width;
            state.Spacing = spacingMeasure;
        }
        state.LineHeight = LineHeight;

        var realizationBounds = context.RealizationRect;
        Point? nextPosition = new Point();
        var currentRow = new List<FlowItem>();
        var currentRowInfo = new RowInfo();
        var itemStretch = ItemsStretch;
        for (var i = 0; i < context.ItemCount; ++i)
        {
            Point currentPosition;
            var item = state.GetItemAt(i);

            if (nextPosition is { } nextPos)
            {
                item.Position = currentPosition = nextPos;
                nextPosition = null;
            }
            else
                currentPosition = item.Position ?? new Point();

            if (currentPosition.Y + LineHeight < realizationBounds.Top)
            {
                // Item is "above" the bounds
                if (item.Element is not null)
                {
                    context.RecycleElement(item.Element);
                    item.Element = null;
                }

                continue;
            }

            if (currentPosition.Y > realizationBounds.Bottom)
            {
                // Item is "below" the bounds.
                if (item.Element is not null)
                {
                    context.RecycleElement(item.Element);
                    item.Element = null;
                }

                // We don't need to measure anything below the bounds
                break;
            }

            item.Element = context.GetOrCreateElementAt(i);
            item.Element.Measure(new(double.PositiveInfinity, LineHeight));
            if (item.DesiredSize is null)
            {
                item.DesiredSize = item.Element.DesiredSize;
            }
            else if (item.DesiredSize != item.Element!.DesiredSize)
            {
                state.ClearMeasureFromIndex(i + 1);
                item.DesiredSize = item.Element.DesiredSize;
            }

            if (CalcNextPosition(item.DesiredSize.Value) && currentPosition.Y > realizationBounds.Bottom)
            {
                // Item is "below" the bounds.
                if (item.Element is not null)
                {
                    context.RecycleElement(item.Element);
                    item.Element = null;
                }

                // We don't need to measure anything below the bounds
                break;
            }

            continue;

            bool CalcNextPosition(Size desiredSize)
            {
                item.Measure = desiredSize;

                if (desiredSize.Width is 0)
                {
                    nextPosition = currentPosition;
                    return false;
                }

                var excessLength = currentPosition.X + desiredSize.Width - parentMeasure.Width;

                if (excessLength + spacingMeasure.Width > 0)
                {
                    if (itemStretch is not FlowLayoutItemsStretch.Stretch)
                    {
                        currentRow.Clear();
                        currentRowInfo = new RowInfo(1);
                        item.IndexOfRow = 0;
                        item.Position = currentPosition = new(0, currentPosition.Y + LineHeight + spacingMeasure.Height);
                        item.RowInfo = currentRowInfo;
                        currentRowInfo.Length += desiredSize.Width;
                        currentRow.Add(item);
                        nextPosition = currentPosition.WithX(desiredSize.Width + spacingMeasure.Width);
                        return true;
                    }

                    var shrinkScale = (parentMeasure.Width - currentRow.Count * spacingMeasure.Width) / (currentRowInfo.Length + desiredSize.Width);
                    var enlargeScale = (parentMeasure.Width - (currentRow.Count - 1) * spacingMeasure.Width) / currentRowInfo.Length;

                    // shrinkScale < enlargeScale
                    // find the one that is closer to 1
                    // length excessed
                    if (1 / shrinkScale < enlargeScale)
                    {
                        item.RowInfo = currentRowInfo;
                        currentRow.Add(item);
                        currentRowInfo.ItemCount = currentRow.Count;
                        item.IndexOfRow = currentRowInfo.ItemCount - 1;
                        // is not used before next assignment
                        // currentRowLength += currentMeasure.Width;
                        Resize(shrinkScale);
                        currentRow.Clear();
                        currentRowInfo = new RowInfo(1);
                        // New Row
                        nextPosition = currentPosition = new(0, currentPosition.Y + LineHeight + spacingMeasure.Height);
                    }
                    // length exceeded after adding space
                    else
                    {
                        Resize(enlargeScale);
                        currentRowInfo.ItemCount = currentRow.Count;
                        currentRow.Clear();
                        currentRowInfo = new RowInfo(1);
                        // New Row
                        item.Position = currentPosition = new(0, currentPosition.Y + LineHeight + spacingMeasure.Height);
                        item.RowInfo = currentRowInfo;
                        item.IndexOfRow = 0;

                        currentRow.Add(item);
                        currentRowInfo.Length += desiredSize.Width;

                        nextPosition = currentPosition.WithX(desiredSize.Width + spacingMeasure.Width);

                        return true;
                    }

                    void Resize(double scale)
                    {
                        var nextPositionX = .0;
                        var tempPositionX = currentPosition.X;
                        var tempPositionY = currentPosition.Y;

                        foreach (var justifiedItem in currentRow)
                        {
                            tempPositionX = nextPositionX;
                            justifiedItem.Position = new Point(tempPositionX, tempPositionY);
                            var tempMeasure = new Size(justifiedItem.Measure!.Value.Width * scale, justifiedItem.Measure!.Value.Height);
                            justifiedItem.Measure = tempMeasure;
                            justifiedItem.Element?.Measure(tempMeasure);
                            nextPositionX = tempPositionX + tempMeasure.Width + spacingMeasure.Width;
                        }
                    }
                }
                else
                {
                    currentRow.Add(item);
                    item.RowInfo = currentRowInfo;
                    currentRowInfo.ItemCount = currentRow.Count;
                    item.IndexOfRow = currentRowInfo.ItemCount - 1;
                    currentRowInfo.Length += desiredSize.Width;
                    currentPosition = new Point(currentPosition.X + desiredSize.Width + spacingMeasure.Width, currentPosition.Y);
                    nextPosition = currentPosition;
                }

                return false;
            }
        }
        // update value with the last line
        // if the last loop is (parentMeasure.Width > currentMeasure.Width + lineMeasure.Width) the total isn't calculated then calculate it
        // if the last loop is (parentMeasure.Width > currentMeasure.Width) the currentMeasure isn't added to the total so add it here
        // for the last condition it is zeros so adding it will make no difference
        // this way is faster than an if condition in every loop for checking the last item
        // Propagating an infinite size causes a crash. This can happen if the parent is scrollable and infinite in the opposite
        // axis to the panel. Clearing to zero prevents the crash.
        // This is likely an incorrect use of the control by the developer, however we need stability here so setting a default that won't crash.
        var totalMeasure = new Size(double.IsInfinity(parentMeasure.Width) ? 0 : Math.Ceiling(parentMeasure.Width), state.GetHeight());

        return totalMeasure;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size parentMeasure)
    {
        if (context.ItemCount > 0)
        {
            var realizationBounds = context.RealizationRect;
            var itemStretch = ItemsStretch;
            var minItemSpacing = MinItemSpacing;
            //  var viewHeight = realizationBounds.Height /= 3;
            //  realizationBounds.Y += viewHeight;

            var state = (FlowLayoutState)context.LayoutState;
            bool ArrangeItem(FlowItem item)
            {
                if (item is { Measure: null } or { Position: null })
                {
                    return false;
                }

                var desiredMeasure = item.Measure.Value;
                var desiredSize = item.DesiredSize.Value;

                var position = item.Position.Value;

                if (position.Y + desiredMeasure.Height >= realizationBounds.Top && position.Y <= realizationBounds.Bottom)
                {
                    var child = context.GetOrCreateElementAt(item.Index);

                    switch (itemStretch)
                    {
                        // place the item
                        case FlowLayoutItemsStretch.Stretch:
                            child.Arrange(new(position, desiredMeasure));
                            break;
                        case FlowLayoutItemsStretch.Start:
                            child.Arrange(new(position, desiredSize));
                            break;
                        case FlowLayoutItemsStretch.End:
                            {
                                var spacing = parentMeasure.Width - item.RowInfo.Length;
                                position = position.WithX(position.X + spacing - minItemSpacing * (item.RowInfo.ItemCount - 1));
                                child.Arrange(new(position, desiredSize));
                                break;
                            }
                        case FlowLayoutItemsStretch.Center:
                            {
                                var spacing = (parentMeasure.Width - item.RowInfo.Length) / 2;
                                position = position.WithX(position.X + spacing);
                                child.Arrange(new(position, desiredSize));
                                break;
                            }
                        case FlowLayoutItemsStretch.Justify:
                            {
                                var spacing = (parentMeasure.Width - item.RowInfo.Length) / (item.RowInfo.ItemCount - 1);
                                if (item.RowInfo.ItemCount is not 1)
                                    position = position.WithX(position.X + spacing * item.IndexOfRow);
                                child.Arrange(new(position, desiredSize));
                                break;
                            }
                    }
                }
                else if (position.Y > realizationBounds.Bottom)
                {
                    return false;
                }

                return true;
            }

            for (var i = 0; i < context.ItemCount; ++i)
            {
                _ = ArrangeItem(state.GetItemAt(i));
            }
        }

        return parentMeasure;
    }
}
