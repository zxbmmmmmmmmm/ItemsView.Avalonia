using CommunityToolkit.Mvvm.ComponentModel;

namespace ItemsView.Avalonia.Sample.Models;

public partial class Item : ObservableObject
{
    [ObservableProperty]
    public partial string? Name { get; set; }

    [ObservableProperty]
    public partial int Value { get; set; }
}