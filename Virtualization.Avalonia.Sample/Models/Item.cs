using CommunityToolkit.Mvvm.ComponentModel;

namespace Virtualization.Avalonia.Sample.Models;

public partial class Item : ObservableObject
{
    [ObservableProperty]
    public partial string? Name { get; set; }

    [ObservableProperty]
    public partial int Value { get; set; }

    [ObservableProperty]
    public partial int LoadedTimes { get; set; }

    [ObservableProperty]
    public partial string? Description { get; set; }
}