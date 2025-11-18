using System.Collections.Generic;
using System.Diagnostics;

namespace Virtualization.Avalonia.Layouts;

[DebuggerDisplay("Count = {Count}, Height = {Height}")]
internal partial class MasonryColumnLayout : List<MasonryItem>
{
    public double Height { get; private set; }

    public new void Add(MasonryItem item)
    {
        Height = item.Top + item.Height;
        base.Add(item);
    }

    public new void Clear()
    {
        Height = 0;
        base.Clear();
    }
}
