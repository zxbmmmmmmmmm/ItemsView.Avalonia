using Avalonia;
using PropertyGenerator.Avalonia;

namespace Virtualization.Avalonia.Layouts;

public partial class FlowLayout
{
    [GeneratedStyledProperty(0d)]
    public partial double LineSpacing { get; set; }

    [GeneratedStyledProperty(0d)]
    public partial double MinItemSpacing { get; set; }

    [GeneratedStyledProperty(200d)]
    public partial double LineHeight { get; set; }

    [GeneratedStyledProperty(FlowLayoutItemsStretch.Stretch)]
    public partial FlowLayoutItemsStretch ItemsStretch { get; set; }

    partial void OnLineSpacingPropertyChanged(AvaloniaPropertyChangedEventArgs e) => OnLineHeightPropertyChanged(e);

    partial void OnMinItemSpacingPropertyChanged(AvaloniaPropertyChangedEventArgs e) => OnLineHeightPropertyChanged(e);

    partial void OnItemsStretchPropertyChanged(AvaloniaPropertyChangedEventArgs e) => OnLineHeightPropertyChanged(e);

    partial void OnLineHeightPropertyChanged(AvaloniaPropertyChangedEventArgs e)
    {
        InvalidateMeasure();
        InvalidateArrange();
    }
}
