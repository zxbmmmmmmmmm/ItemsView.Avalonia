using Avalonia;

namespace Virtualization.Avalonia.Layouts;

/// <summary>
/// Gets the orientation, if any, in which items are laid out based on their index in the source collection.
/// </summary>
public enum IndexBasedLayoutOrientation
{
    /// <summary>
    /// There is no correlation between the items' layout and their index number.
    /// </summary>
    None = 0,

    /// <summary>
    /// Items are laid out vertically with increasing indices.
    /// </summary>
    TopToBottom = 1,

    /// <summary>
    /// Items are laid out horizontally with increasing indices.
    /// </summary>
    LeftToRight = 2
}

/// <summary>
/// Represents the base class for an object that sizes and arranges child elements for a host.
/// </summary>
public abstract class Layout : AvaloniaObject
{    
    /// <summary>
    /// 
    /// </summary>
    public IndexBasedLayoutOrientation IndexBasedLayoutOrientation { get; protected internal set; }

    /// <summary>
    /// Occurs when the measurement state (layout) has been invalidated.
    /// </summary>
    public event EventHandler<Layout, EventArgs>? MeasureInvalidated;

    /// <summary>
    /// Occurs when the arrange state(layout) has been invalidated.
    /// </summary>
    public event EventHandler<Layout, EventArgs>? ArrangeInvalidated;

    private static VirtualizingLayoutContext GetVirtualizingLayoutContext(LayoutContext context)
    {
        switch (context)
        {
            case VirtualizingLayoutContext vlc:
                return vlc;
            case NonVirtualizingLayoutContext nvlc:
            {
                return nvlc.VirtualizingContextAdapter;
            }
            default:
                throw new NotImplementedException();
        }
    }

    private static NonVirtualizingLayoutContext GetNonVirtualizingLayoutContext(LayoutContext context)
    {
        switch (context)
        {
            case NonVirtualizingLayoutContext nvlc:
                return nvlc;
            case VirtualizingLayoutContext vlc:
                return vlc.NonVirtualizingContextAdapter;
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Initializes any per-container state the layout requires when it is attached to a UIElement container.
    /// </summary>
    public void InitializeForContext(LayoutContext context)
    {
        switch (this)
        {
            case VirtualizingLayout vl:
            {
                var vc = GetVirtualizingLayoutContext(context);
                vl.InitializeForContextCore(vc);
                break;
            }
            case NonVirtualizingLayout nvl:
            {
                var nvc = GetNonVirtualizingLayoutContext(context);
                nvl.InitializeForContextCore(nvc);
                break;
            }
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Removes any state the layout previously stored on the UIElement container.
    /// </summary>
    public void UninitializeForContext(LayoutContext context)
    {
        switch (this)
        {
            case VirtualizingLayout vl:
            {
                var vc = GetVirtualizingLayoutContext(context);
                vl.UninitializeForContextCore(vc);
                break;
            }
            case NonVirtualizingLayout nvl:
            {
                var nvc = GetNonVirtualizingLayoutContext(context);
                nvl.UninitializeForContextCore(nvc);
                break;
            }
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Suggests a DesiredSize for a container element. A container element that supports attached layouts 
    /// should call this method from their own MeasureOverride implementations to form a recursive layout update. 
    /// The attached layout is expected to call the Measure for each of the container’s UIElement children.
    /// </summary>
    public Size Measure(LayoutContext context, Size availableSize)
    {
        switch (this)
        {
            case VirtualizingLayout vl:
            {
                var vc = GetVirtualizingLayoutContext(context);
                return vl.MeasureOverride(vc, availableSize);
            }
            case NonVirtualizingLayout nvl:
            {
                var nvc = GetNonVirtualizingLayoutContext(context);
                return nvl.MeasureOverride(nvc, availableSize);
            }
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Positions child elements and determines a size for a container UIElement. Container elements that 
    /// support attached layouts should call this method from their layout override implementations to 
    /// form a recursive layout update.
    /// </summary>
    public Size Arrange(LayoutContext context, Size finalSize)
    {
        switch (this)
        {
            case VirtualizingLayout vl:
            {
                var vc = GetVirtualizingLayoutContext(context);
                return vl.ArrangeOverride(vc, finalSize);
            }
            case NonVirtualizingLayout nvl:
            {
                var nvc = GetNonVirtualizingLayoutContext(context);
                return nvl.ArrangeOverride(nvc, finalSize);
            }
            default:
                throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Invalidates the measurement state (layout) for all UIElement containers that reference this layout.
    /// </summary>
    protected void InvalidateMeasure() =>
        MeasureInvalidated?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// Invalidates the arrange state (layout) for all UIElement containers that reference this layout. 
    /// After the invalidation, the UIElement will have its layout updated, which occurs asynchronously.
    /// </summary>
    protected void InvalidateArrange() =>
        ArrangeInvalidated?.Invoke(this, EventArgs.Empty);

    /// <summary>
    /// 
    /// </summary>
    protected internal virtual ItemCollectionTransitionProvider? CreateDefaultItemTransitionProvider() => null;
}
