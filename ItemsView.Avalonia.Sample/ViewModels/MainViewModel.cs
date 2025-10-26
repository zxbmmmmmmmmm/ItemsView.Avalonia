using ItemsView.Avalonia.Sample.Models;
using System.Collections.ObjectModel;

namespace ItemsView.Avalonia.Sample.ViewModels;

public class MainViewModel
{
    public ObservableCollection<Item> Items { get; set; } = [new(1,"Bob"),new(2,"Alice"),new(3,"John")];
}