using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Virtualization.Avalonia.Sample.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Virtualization.Avalonia.Sample.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableBindingList<Item> Items { get; set; } = [];

    [ObservableProperty]
    public partial string Name { get;
        set; } 
        = "New Item 1";

    [ObservableProperty]
    public partial int Value { get; set; } = 5;

    public int TotalValue => Items.Sum(i => i.Value);

    public MainViewModel()
    {
        Items.ListChanged += (s, e) =>
        OnPropertyChanged(nameof(TotalValue));
        for (var i = 0; i <= 100000; i++)
        {
            Items.Add(new Item { Name = $"Item {i}", Value = 10 });
        }
        Name = $"New Item {Items.Count + 1}";
    }

    [RelayCommand]
    public void Add()
    {
        Items.Add(new Item { Name = this.Name, Value = this.Value });
        Name = "New Item " + (Items.Count+1);
    }
    [RelayCommand]
    public void AddAll()
    {
        foreach (var item in Items)
        {
            item.Value += 10;
        }
    }

    [RelayCommand]
    public void Remove(Item item)
    {
        Items.Remove(item);
    }
}

[ObservableObject]
public partial class ObservableBindingList<T> : BindingList<T>, INotifyCollectionChanged
{
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    protected override void InsertItem(int index, T item)
    {
        base.InsertItem(index, item);
        CollectionChanged?.Invoke(index, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
    }

    protected override void RemoveItem(int index)
    {
        var item = this[index];
        base.RemoveItem(index);
        CollectionChanged?.Invoke(index, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged("Item[]");
    }
}
