using Avalonia;
using Avalonia.Controls;

namespace Virtualization.Avalonia.Layouts;

internal class FlowItem(int index)
{
    public int Index { get; } = index;

    public Size? DesiredSize { get; internal set; }

    public Size? Measure { get; internal set; }

    public Point? Position { get; internal set; }

    public Control? Element { get; internal set; }

    public int IndexOfRow { get; internal set; }

    public RowInfo RowInfo { get; internal set; } = null!;
}

internal class RowInfo(int itemCount = 0, double length = 0)
{
    public int ItemCount { get; set; } = itemCount;

    public double Length { get; set; } = length;
}

