using Avalonia;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

internal class FlowItem(int index)
{
    public int Index { get; } = index;

    public Size? DesiredSize { get; internal set; }
}
