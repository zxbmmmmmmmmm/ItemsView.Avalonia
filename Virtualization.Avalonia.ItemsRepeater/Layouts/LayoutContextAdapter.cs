using Avalonia;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

internal class LayoutContextAdapter(NonVirtualizingLayoutContext nonVirtualizingContext) : VirtualizingLayoutContext
{
    public override object? GetItemAt(int index) => nonVirtualizingContext.Children[index];

    public override Control GetOrCreateElementAt(int index, ElementRealizationOptions options) => nonVirtualizingContext.Children[index];

    public override void RecycleElement(Control element) { }

    private int GetElementIndex(Control element) => nonVirtualizingContext.Children.IndexOf(element);

    public override object? LayoutState
    { 
        get => nonVirtualizingContext.LayoutState; 
        set => nonVirtualizingContext.LayoutState = value;
    }

    public override int ItemCount => nonVirtualizingContext.Children.Count;

    public override Rect VisibleRect  =>
        new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);

    public override Rect RealizationRect =>
        new Rect(0, 0, double.PositiveInfinity, double.PositiveInfinity);

    public override int RecommendedAnchorIndex => -1;

    public override Point LayoutOrigin
    {
        get =>  default;
        set
        {
            if (value != default)
                throw new ArgumentException("LayoutOrigin must be at (0,0) when RealizationRect is infinite sized.");
        }
    }
}
