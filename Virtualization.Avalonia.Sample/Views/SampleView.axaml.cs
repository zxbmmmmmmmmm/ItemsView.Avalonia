using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Virtualization.Avalonia.Sample;

public partial class SampleView : UserControl
{
    public SampleView()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object? sender, RoutedEventArgs e)
    {
        var scrollViewer = ItemsView.FindDescendantOfType<ItemsRepeater>();
        var scrollViewerChildren = scrollViewer.Children;
    }
}
