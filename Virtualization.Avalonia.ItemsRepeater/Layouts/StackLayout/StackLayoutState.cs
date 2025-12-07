using System.Buffers;

namespace Virtualization.Avalonia.Layouts;

internal class StackLayoutState
{
    public FlowLayoutAlgorithm FlowAlgorithm { get; private set; } = null!;

    public double TotalElementSize { get; private set; }

    public double MaxArrangeBounds { get; private set; }

    public int TotalElementsMeasured { get; private set; }

    public void InitializeForContext(VirtualizingLayoutContext context, IFlowLayoutAlgorithmDelegates callbacks)
    {
        FlowAlgorithm = new();
        FlowAlgorithm.InitializeForContext(context, callbacks);

        _estimationBuffer = ArrayPool<double>.Shared.Rent(BufferSize);

        context.LayoutState = this;
    }

    public void UninitializeForContext(VirtualizingLayoutContext context)
    {
        ArrayPool<double>.Shared.Return(_estimationBuffer);
        FlowAlgorithm.UninitializeForContext(context);
    }

    public void OnElementMeasured(int elementIndex, double majorSize, double minorSize)
    {
        var estimationBufferIndex = elementIndex % BufferSize;
        var alreadyMeasured = _estimationBuffer[estimationBufferIndex] != 0;
        if (!alreadyMeasured) 
            TotalElementsMeasured++;

        TotalElementSize -= _estimationBuffer[estimationBufferIndex];
        TotalElementSize += majorSize;
        _estimationBuffer[estimationBufferIndex] = majorSize;

        MaxArrangeBounds = Math.Max(MaxArrangeBounds, minorSize);
    }

    public void OnMeasureStart() => MaxArrangeBounds = 0;

    private double[] _estimationBuffer = null!;
    private const int BufferSize = 100;
}
