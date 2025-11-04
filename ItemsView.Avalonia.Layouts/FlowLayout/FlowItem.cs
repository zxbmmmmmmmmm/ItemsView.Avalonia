// Copyright (c) Pixeval.Controls.
// Licensed under the GPL v3 License.


using Avalonia;
using Avalonia.Layout;

namespace ItemsView.Avalonia.Layouts.FlowLayout;

internal class FlowItem(int index)
{
    public int Index { get; } = index;

    public Size? DesiredSize { get; internal set; }

    public Size? Measure { get; internal set; }

    public Point? Position { get; internal set; }

    public Layoutable? Element { get; internal set; }
}
