using Avalonia.Controls;
using Avalonia.Layout;
using System.Xml.Linq;

namespace Virtualization.Avalonia.Layouts;

internal class MasonryItem
{
    public MasonryItem(int index)
    {
        this.Index = index;
    }

    public double Top { get; internal set; }

    public double Height { get; internal set; }

    public int Index { get; }

    public Layoutable? Element { get; internal set; }
}