using Avalonia.Controls;
using ItemsView.Avalonia.Sample.Models;
using PropertyGenerator.Avalonia;

namespace ItemsView.Avalonia.Sample;

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