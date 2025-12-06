using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;

namespace Virtualization.Avalonia.Layouts;

internal class FlowLayoutState
{
    public FlowLayoutAlgorithm FlowAlgorithm { get; set; } = new();

    public double AverageElementSize => TotalElementsMeasured is 0 ? 0 : _totalElementSize / TotalElementsMeasured;

    public int TotalElementsMeasured { get; private set; }

    public void InitializeForContext(VirtualizingLayoutContext context, IFlowLayoutAlgorithmDelegates callbacks)
    {
        FlowAlgorithm = new FlowLayoutAlgorithm();
        FlowAlgorithm.InitializeForContext(context, callbacks);

        _estimationBuffer = new double[BufferSize];

        context.LayoutState = this;
    }

    public void UninitializeForContext(VirtualizingLayoutContext context) =>
        FlowAlgorithm.UninitializeForContext(context);

    public void OnElementMeasured(int elementIndex, double width)
    {
        var estimationBufferIndex = elementIndex % BufferSize;
        var alreadyMeasured = _estimationBuffer[estimationBufferIndex] != 0;
        if (!alreadyMeasured)
            TotalElementsMeasured++;

        _totalElementSize -= _estimationBuffer[estimationBufferIndex];
        _totalElementSize += width;
        _estimationBuffer[estimationBufferIndex] = width;
    }

    private double _totalElementSize;
    private double[] _estimationBuffer = null!;
    private const int BufferSize = 100;
}
