using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

internal class VirtualLayoutContextAdapter(VirtualizingLayoutContext context) : NonVirtualizingLayoutContext
{
    public override IReadOnlyList<Control> Children 
    {
        get
        {
            if (GetContext() is not { } ctx)
                throw new NullReferenceException();
            _children ??= new(ctx);
            return _children;
        }
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

    protected internal override WeakReference<ItemsRepeater> Owner => context.Owner;

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
