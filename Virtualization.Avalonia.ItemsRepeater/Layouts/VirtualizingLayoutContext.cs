using System.Diagnostics.CodeAnalysis;
using Avalonia;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

/// <summary>
/// Represents the base class for layout context types that support virtualization.
/// </summary>
public abstract class VirtualizingLayoutContext : LayoutContext
{
    /// <summary>
    /// Retrieves the data item in the source found at the specified index.
    /// </summary>
    public abstract object? GetItemAt(int index);

    /// <summary>
    /// Retrieves a UIElement that represents the data item in the source found at the specified index.By default, if an element already exists, it is returned; otherwise, a new element is created.
    /// </summary>
    public Control GetOrCreateElementAt(int index) => GetOrCreateElementAt(index, ElementRealizationOptions.None);

    /// <summary>
    /// Retrieves a UIElement that represents the data item in the source found at the specified index using the specified options.
    /// </summary>
    public abstract Control GetOrCreateElementAt(int index, ElementRealizationOptions options);

    /// <summary>
    /// Clears the specified UIElement and allows it to be either re-used or released.
    /// </summary>
    public abstract void RecycleElement(Control element);
    
    /// <summary>
    /// Gets the number of items in the data.
    /// </summary>
    public abstract int ItemCount { get; }

    /// <summary>
    /// Gets an area that represents the viewport of the layout.
    /// </summary>
    public abstract Rect VisibleRect { get; }

    /// <summary>
    /// Gets an area that represents the viewport and buffer that the layout should fill with realized elements.
    /// </summary>
    public abstract Rect RealizationRect { get; }

    /// <summary>
    /// Gets the recommended index from which to start the generation and layout of elements.
    /// </summary>
    public abstract int RecommendedAnchorIndex { get; }

    /// <summary>
    /// Gets or sets the origin point for the estimated content size.
    /// </summary>
    public abstract Point LayoutOrigin
    {
        get;
        // TODO: Not in WinUI 1.5?
        set;
    }

    [field: AllowNull, MaybeNull]
    internal NonVirtualizingLayoutContext NonVirtualizingContextAdapter => field ??= new VirtualLayoutContextAdapter(this);
}
