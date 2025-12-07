using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia;

namespace Virtualization.Avalonia.Layouts;

internal class FlowLayoutState
{
    public FlowLayoutAlgorithm FlowAlgorithm { get; set; } = new();

    public double AverageElementSize => TotalElementsMeasured is 0 ? 0 : _totalElementSize / TotalElementsMeasured;

    public int TotalElementsMeasured { get; private set; }

    public int TotalLines { get; private set; } = 1;

    public void InitializeForContext(VirtualizingLayoutContext context, IFlowLayoutAlgorithmDelegates callbacks)
    {
        FlowAlgorithm = new FlowLayoutAlgorithm();
        FlowAlgorithm.InitializeForContext(context, callbacks);

        context.LayoutState = this;
    }

    public void UninitializeForContext(VirtualizingLayoutContext context) =>
        FlowAlgorithm.UninitializeForContext(context);

    public void OnElementMeasured(int elementIndex, double width, int controlHashCode)
    {
        ref var value = ref CollectionsMarshal.GetValueRefOrAddDefault(_set, controlHashCode, out var exists);
        if (exists)
            _totalElementSize -= value;
        else
            TotalElementsMeasured++;

        value = width;
        _totalElementSize += value;
    }

    public void OnBreakLine() => TotalLines++;

    public void OnMeasureStart()
    {
        _set = [];
        TotalLines = 1;
        _totalElementSize = 0;
        TotalElementsMeasured = 0;
    }

    private double _totalElementSize;
    private Dictionary<int, double> _set = null!;
}
