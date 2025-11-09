using System.Collections;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

internal class VirtualLayoutContextAdapter(VirtualizingLayoutContext context) : NonVirtualizingLayoutContext
{
    protected override IReadOnlyList<Control> ChildrenCore()
    {
        if (GetContext() is not { } context)
            throw new NullReferenceException();
        _children ??= new(context);
        return _children;
    }

    public override object? LayoutState
    { 
        get => GetContext()?.LayoutState;
        set
        {
            if (GetContext() is { } vlc)
                vlc.LayoutState = value;
        } 
    }

    private VirtualizingLayoutContext? GetContext() => _virtualizingContext.TryGetTarget(out var target) ? target : null;

    private readonly WeakReference<VirtualizingLayoutContext> _virtualizingContext = new(context);
    private ChildrenCollection? _children;

    // WinUI makes this Generic, but C# doesn't like the indexer getting a control
    // with returning a generic type
    private class ChildrenCollection(VirtualizingLayoutContext context) : IReadOnlyList<Control>
    {
        public int Count => context.ItemCount;

        public Control this[int index] => context.GetOrCreateElementAt(index, ElementRealizationOptions.None);

        public IEnumerator<Control> GetEnumerator() => this.Cast<Control>().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
