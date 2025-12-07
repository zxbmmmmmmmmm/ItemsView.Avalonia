using Avalonia;
using Avalonia.Layout;

namespace Virtualization.Avalonia.Layouts;

public partial class UniformGridLayout
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
        {
            var orientation = change.GetNewValue<Orientation>();

            //Note: For UniformGridLayout Vertical Orientation means we have a Horizontal ScrollOrientation. Horizontal Orientation means we have a Vertical ScrollOrientation.
            //i.e. the properties are the inverse of each other.
            _scrollOrientation = orientation is Orientation.Horizontal
                ? ScrollOrientation.Vertical
                : ScrollOrientation.Horizontal;
        }
        else if (change.Property == ItemSpacingProperty)
        {
            _itemSpacing = change.GetNewValue<double>();
        }
        else if (change.Property == LineSpacingProperty)
        {
            _lineSpacing = change.GetNewValue<double>();
        }
        else if (change.Property == ItemsJustificationProperty)
        {
            _itemsJustification = change.GetNewValue<UniformGridLayoutItemsJustification>();
        }
        else if (change.Property == ItemsStretchProperty)
        {
            _itemsStretch = change.GetNewValue<UniformGridLayoutItemsStretch>();
        }
        else if (change.Property == MinItemWidthProperty)
        {
            _minItemWidth = change.GetNewValue<double>();
        }
        else if (change.Property == MinItemHeightProperty)
        {
            _minItemHeight = change.GetNewValue<double>();
        }
        else if (change.Property == MaximumRowsOrColumnsProperty)
        {
            _maximumRowsOrColumns = change.GetNewValue<int>();
        }

        InvalidateLayout();
    }

    private void InvalidateLayout() => InvalidateMeasure();

    /// <summary>
    /// Defines the <see cref="ItemsJustification"/> property.
    /// </summary>
    public static readonly StyledProperty<UniformGridLayoutItemsJustification> ItemsJustificationProperty =
        AvaloniaProperty.Register<UniformGridLayout, UniformGridLayoutItemsJustification>(nameof(ItemsJustification));

    /// <summary>
    /// Defines the <see cref="ItemsStretch"/> property.
    /// </summary>
    public static readonly StyledProperty<UniformGridLayoutItemsStretch> ItemsStretchProperty =
        AvaloniaProperty.Register<UniformGridLayout, UniformGridLayoutItemsStretch>(nameof(ItemsStretch));

    /// <summary>
    /// Defines the <see cref="MinItemHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinItemHeightProperty =
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(MinItemHeight));

    /// <summary>
    /// Defines the <see cref="MinItemWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(MinItemWidth));

    /// <summary>
    /// Defines the <see cref="ItemSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemSpacingProperty =
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(ItemSpacing));

    /// <summary>
    /// Defines the <see cref="LineSpacing"/> property.
    /// </summary>
    public static readonly StyledProperty<double> LineSpacingProperty =
        AvaloniaProperty.Register<UniformGridLayout, double>(nameof(LineSpacing));

    /// <summary>
    /// Defines the <see cref="MaximumRowsOrColumns"/> property.
    /// </summary>
    public static readonly StyledProperty<int> MaximumRowsOrColumnsProperty =
        AvaloniaProperty.Register<UniformGridLayout, int>(nameof(MaximumRowsOrColumns));

    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackLayout.OrientationProperty.AddOwner<UniformGridLayout>(new StyledPropertyMetadata<Orientation>(Orientation.Horizontal));

    /// <summary>
    /// Gets or sets a value that indicates how items are aligned on the non-scrolling or non-
    /// virtualizing axis.
    /// </summary>
    /// <value>
    /// An enumeration value that indicates how items are aligned. The default is Start.
    /// </value>
    public UniformGridLayoutItemsJustification ItemsJustification
    {
        get => GetValue(ItemsJustificationProperty);
        set => SetValue(ItemsJustificationProperty, value);
    }

    /// <summary>
    /// Gets or sets a value that indicates how items are sized to fill the available space.
    /// </summary>
    /// <value>
    /// An enumeration value that indicates how items are sized to fill the available space.
    /// The default is None.
    /// </value>
    /// <remarks>
    /// This property enables adaptive layout behavior where the items are sized to fill the
    /// available space along the non-scrolling axis, and optionally maintain their aspect ratio.
    /// </remarks>
    public UniformGridLayoutItemsStretch ItemsStretch
    {
        get => GetValue(ItemsStretchProperty);
        set => SetValue(ItemsStretchProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum height of each item.
    /// </summary>
    /// <value>
    /// The minimum height (in pixels) of each item. The default is NaN, in which case the
    /// height of the first item is used as the minimum.
    /// </value>
    public double MinItemHeight
    {
        get => GetValue(MinItemHeightProperty);
        set => SetValue(MinItemHeightProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum width of each item.
    /// </summary>
    /// <value>
    /// The minimum width (in pixels) of each item. The default is NaN, in which case the width
    /// of the first item is used as the minimum.
    /// </value>
    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum space between items on the horizontal axis.
    /// </summary>
    /// <remarks>
    /// The spacing may exceed this minimum value when <see cref="ItemsJustification"/> is set
    /// to SpaceEvenly, SpaceAround, or SpaceBetween.
    /// </remarks>
    public double ItemSpacing
    {
        get => GetValue(ItemSpacingProperty);
        set => SetValue(ItemSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the minimum space between items on the vertical axis.
    /// </summary>
    /// <remarks>
    /// The spacing may exceed this minimum value when <see cref="ItemsJustification"/> is set
    /// to SpaceEvenly, SpaceAround, or SpaceBetween.
    /// </remarks>
    public double LineSpacing
    {
        get => GetValue(LineSpacingProperty);
        set => SetValue(LineSpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the maximum row or column count.
    /// </summary>
    public int MaximumRowsOrColumns
    {
        get => GetValue(MaximumRowsOrColumnsProperty);
        set => SetValue(MaximumRowsOrColumnsProperty, value);
    }

    /// <summary>
    /// Gets or sets the axis along which items are laid out.
    /// </summary>
    /// <value>
    /// One of the enumeration values that specifies the axis along which items are laid out.
    /// The default is Horizontal.
    /// </value>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    ScrollOrientation IOrientationBasedMeasures.ScrollOrientation => _scrollOrientation;

    private ScrollOrientation _scrollOrientation = ScrollOrientation.Vertical;
    private double _minItemWidth = double.NaN;
    private double _minItemHeight = double.NaN;
    private double _itemSpacing;
    private double _lineSpacing;
    private UniformGridLayoutItemsJustification _itemsJustification;
    private UniformGridLayoutItemsStretch _itemsStretch;
    private int _maximumRowsOrColumns = int.MaxValue;
}
