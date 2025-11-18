using System;
using Avalonia;

namespace Virtualization.Avalonia.Layouts;

/// <summary>
/// Represents the base class for an object that facilitates communication between an attached layout and its host container.
/// </summary>
public abstract class LayoutContext : AvaloniaObject
{
    /// <summary>
    /// Gets or sets an object that represents the state of a layout.
    /// </summary>
    public virtual object? LayoutState { get; set; }
    
    protected internal abstract WeakReference<ItemsRepeater> Owner { get; }

    protected ItemsRepeater? GetOwner() => Owner.TryGetTarget(out var target) ? target : null;
}
