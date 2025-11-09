using Avalonia;
using Avalonia.Controls;
using Virtualization.Avalonia.Layouts;

namespace Virtualization.Avalonia;

internal class RepeaterLayoutContext(ItemsRepeater owner) : VirtualizingLayoutContext
{
    public override object? GetItemAt(int index) => GetOwner()?.ItemsSourceView?[index];

    public override Control GetOrCreateElementAt(int index, ElementRealizationOptions options)
    {
        var owner = GetOwner() ?? throw new NullReferenceException();
        return owner.GetElementImpl(index,
            (options & ElementRealizationOptions.ForceCreate) == ElementRealizationOptions.ForceCreate,
            (options & ElementRealizationOptions.SuppressAutoRecycle) == ElementRealizationOptions.SuppressAutoRecycle);
    }

    public override void RecycleElement(Control element)
    {
        var owner = GetOwner();
#if DEBUG && REPEATER_TRACE
        var x = Log.Logger;
        Log.Debug("RepeaterLayout - RecycleElement {Index}", owner?.GetElementIndex(element));
#endif
        owner?.ClearElementImpl(element);
    }

    public override object? LayoutState
    {
        get => GetOwner()?.LayoutState;
        set
        {
            if (GetOwner() is { } ir)
                ir.LayoutState = value;
        }
    }
    
    public override int ItemCount => GetOwner()?.ItemsSourceView?.Count ?? 0;

    public override Rect VisibleRect => GetOwner()?.VisibleWindow ?? default;

    public override Rect RealizationRect => GetOwner()?.RealizationWindow ?? default;

    public override int RecommendedAnchorIndex
    {
        get
        {
            if (GetOwner() is { SuggestedAnchor: { } anchor } repeater)
                return repeater.GetElementIndex(anchor);

            return -1;
        }
    }

    public override Point LayoutOrigin
    {
        get => GetOwner()?.LayoutOrigin ?? default;
        set
        {
            if (GetOwner() is { } ir)
                ir.LayoutOrigin = value;
        }
    }

    private ItemsRepeater? GetOwner() => _owner.TryGetTarget(out var target) 
        ? target 
        : null;

    private readonly WeakReference<ItemsRepeater> _owner = new(owner);
}
