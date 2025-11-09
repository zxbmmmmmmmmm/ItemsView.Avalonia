using Avalonia;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

internal class LayoutContextAdapter(NonVirtualizingLayoutContext nonVirtualizingContext) : VirtualizingLayoutContext
{
    protected internal override object? LayoutStateCore 
    { 
        get => nonVirtualizingContext.LayoutState; 
        set => nonVirtualizingContext.LayoutState = value;
    }

    protected internal override int ItemCountCore() => nonVirtualizingContext.Children.Count;

    protected override object? GetItemAtCore(int index) => nonVirtualizingContext.Children[index];

    protected override Control GetOrCreateElementAtCore(int index, ElementRealizationOptions options) => nonVirtualizingContext.Children[index];

    protected override void RecycleElementCore(Control element) { }

    private int GetElementIndexCore(Control element)
    {
        var children = nonVirtualizingContext.Children;
        return children.IndexOf(element);
    }

    protected override Rect VisibleRectCore() =>
        new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);

    protected override Rect RealizationRectCore() =>
        new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);

    protected override int RecommendedAnchorIndexCore() => -1;

    protected override Point LayoutOriginCore() => default;

    protected override void LayoutOriginCore(Point value)
    {
        if (value != default)
        {
            throw new ArgumentException("LayoutOrigin must be at (0,0) when RealizationRect is infinite sized.");
        }
    }
}
