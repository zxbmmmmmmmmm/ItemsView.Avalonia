using Avalonia;
using Avalonia.Controls;
using Virtualization.Avalonia.Layouts;

namespace Virtualization.Avalonia;

internal class RepeaterLayoutContext(ItemsRepeater owner) : VirtualizingLayoutContext
{
    protected internal override int ItemCountCore() => GetOwner()?.ItemsSourceView?.Count ?? 0;

    protected override Control GetOrCreateElementAtCore(int index, ElementRealizationOptions options)
    {
        var owner = GetOwner() ?? throw new NullReferenceException();
        return owner.GetElementImpl(index,
            (options & ElementRealizationOptions.ForceCreate) == ElementRealizationOptions.ForceCreate,
            (options & ElementRealizationOptions.SuppressAutoRecycle) == ElementRealizationOptions.SuppressAutoRecycle);
    }

    protected internal override object? LayoutStateCore
    {
        get => GetOwner()?.LayoutState;
        set
        {
            if (GetOwner() is { } ir)
                ir.LayoutState = value;
        }
    }

    protected override object? GetItemAtCore(int index) => GetOwner()?.ItemsSourceView?[index];

    protected override void RecycleElementCore(Control element)
    {
        var owner = GetOwner();
#if DEBUG && REPEATER_TRACE
        var x = Log.Logger;
        Log.Debug("RepeaterLayout - RecycleElement {Index}", owner?.GetElementIndex(element));
#endif
        owner?.ClearElementImpl(element);
    }

    protected override Rect VisibleRectCore() => GetOwner()?.VisibleWindow ?? default;

    protected override Rect RealizationRectCore() => GetOwner()?.RealizationWindow ?? default;

    protected override int RecommendedAnchorIndexCore()
    {
        if (GetOwner() is { SuggestedAnchor: { } anchor } repeater)
        {
            return repeater.GetElementIndex(anchor);
        }

        return -1;
    }

    protected override Point LayoutOriginCore() => GetOwner()?.LayoutOrigin ?? default;

    protected override void LayoutOriginCore(Point value)
    {
        if (GetOwner() is { } ir)
            ir.LayoutOrigin = value;
    }

    private ItemsRepeater? GetOwner() => _owner.TryGetTarget(out var target) 
        ? target 
        : null;

    private readonly WeakReference<ItemsRepeater> _owner = new(owner);
}
