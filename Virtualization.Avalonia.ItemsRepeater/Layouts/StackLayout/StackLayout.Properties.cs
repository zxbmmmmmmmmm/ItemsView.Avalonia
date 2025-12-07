using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Virtualization.Avalonia.Layouts;

public partial class StackLayout
{
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
        {
            var orientation = change.GetNewValue<Orientation>();
            _scrollOrientation = orientation is Orientation.Horizontal
                ? ScrollOrientation.Horizontal
                : ScrollOrientation.Vertical;
        }
        else if (change.Property == SpacingProperty)
        {
            _spacing = change.GetNewValue<double>();
        }

        InvalidateLayout();
    }

    private void InvalidateLayout() => InvalidateMeasure();

    /// <summary>
    /// Defines the <see cref="Spacing"/> property
    /// </summary>
    public static readonly StyledProperty<double> SpacingProperty =
        StackPanel.SpacingProperty.AddOwner<StackLayout>();

    /// <summary>
    /// Defines the <see cref="Orientation"/> property
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        StackPanel.OrientationProperty.AddOwner<StackLayout>(new(Orientation.Vertical));

    /// <summary>
    /// Gets or sets a uniform distance (in pixels) between stacked items. It is applied
    /// in the direction of the StackLayout's Orientation
    /// </summary>
    public double Spacing
    {
        get => GetValue(SpacingProperty);
        set => SetValue(SpacingProperty, value);
    }

    /// <summary>
    /// Gets or sets the dimension by which child elements are stacked
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public bool DisableVirtualization { get; set; }

    private ScrollOrientation _scrollOrientation = ScrollOrientation.Vertical;
    private double _spacing;

    ScrollOrientation IOrientationBasedMeasures.ScrollOrientation => _scrollOrientation;
}
