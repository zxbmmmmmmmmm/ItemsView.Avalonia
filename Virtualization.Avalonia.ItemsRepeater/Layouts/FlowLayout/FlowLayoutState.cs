using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;

namespace Virtualization.Avalonia.Layouts;

internal class FlowLayoutState(VirtualizingLayoutContext context)
{
    private readonly List<FlowItem> _items = [];

    public FlowLayoutAlgorithm FlowAlgorithm { get; set; } = new();
}
