using Avalonia;
using PropertyGenerator.Avalonia;

namespace Virtualization.Avalonia.Layouts;

public partial class FlowLayout
{
    [GeneratedStyledProperty(0d)]
    public partial double LineSpacing { get; set; }

    [GeneratedStyledProperty(0d)]
    public partial double ItemSpacing { get; set; }

    [GeneratedStyledProperty(200d)]
    public partial double LineHeight { get; set; }

    [GeneratedStyledProperty(FlowLayoutItemsStretch.Stretch)]
    public partial FlowLayoutItemsStretch ItemsStretch { get; set; }

    ScrollOrientation IOrientationBasedMeasures.ScrollOrientation => ScrollOrientation.Vertical;

    private double _itemSpacing;
    private double _lineSpacing;
    private double _lineHeight = 200;

    partial void OnLineSpacingPropertyChanged(double newValue)
    {
        _lineSpacing = newValue;
        InvalidateLayout();
    }

    partial void OnItemSpacingPropertyChanged(double newValue)
    {
        _itemSpacing = newValue;
        InvalidateLayout();
    }

    partial void OnItemsStretchPropertyChanged(AvaloniaPropertyChangedEventArgs e) => OnLineHeightPropertyChanged(e);

    partial void OnLineHeightPropertyChanged(double newValue)
    {
        _lineHeight = newValue;
        InvalidateLayout();
    }

    private void InvalidateLayout() => InvalidateMeasure();
}
