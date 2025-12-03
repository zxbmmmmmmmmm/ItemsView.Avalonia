using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;

namespace Virtualization.Avalonia.Layouts;

internal class FlowLayoutState
{
    private readonly List<FlowItem> _items = [];

    public FlowLayoutAlgorithm FlowAlgorithm { get; set; } = new();

    public void InitializeForContext(VirtualizingLayoutContext context, IFlowLayoutAlgorithmDelegates callbacks)
    {
        FlowAlgorithm ??= new FlowLayoutAlgorithm();
        FlowAlgorithm.InitializeForContext(context, callbacks);
        context.LayoutState = this;
    }

    public void UninitializeForContext(VirtualizingLayoutContext context) =>
    FlowAlgorithm.UninitializeForContext(context);
}
