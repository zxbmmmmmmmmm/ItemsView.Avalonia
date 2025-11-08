using Avalonia.Controls;
using Virtualization.Avalonia.Sample.Models;
using PropertyGenerator.Avalonia;

namespace Virtualization.Avalonia.Sample;

public partial class ItemControl : UserControl
{
    public ItemControl()
    {
        InitializeComponent();
    }

    [GeneratedDirectProperty]
    public partial Item? Item { get; set; }

    partial void OnItemPropertyChanged(Item? newValue)
    {
        newValue?.LoadedTimes += 1;
    }
}