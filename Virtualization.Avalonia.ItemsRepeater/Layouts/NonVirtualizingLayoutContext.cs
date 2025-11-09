using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

/// <summary>
/// Represents the base class for layout context types that do not support virtualization.
/// </summary>
public abstract class NonVirtualizingLayoutContext : LayoutContext
{
    /// <summary>
    /// Gets the collection of child UIElements from the container that provides the context.
    /// </summary>
    public abstract IReadOnlyList<Control> Children { get; }
}
