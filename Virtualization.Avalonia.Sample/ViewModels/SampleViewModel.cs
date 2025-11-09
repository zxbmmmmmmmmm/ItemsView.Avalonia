using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Virtualization.Avalonia.Sample.Models;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Virtualization.Avalonia.Layouts;

namespace Virtualization.Avalonia.Sample.ViewModels;

public partial class SampleViewModel : ObservableObject
{
    [ObservableProperty]
    public partial IDataTemplate? ItemTemplate { get; set; } = Application.Current?.FindResource("TextItemsTemplate") as IDataTemplate;

    [ObservableProperty]
    public partial IEnumerable? Items { get; set; } = TextItems;

    public static IReadOnlyList<DataType> DataSources { get; } = Enum.GetValues<DataType>();

    [ObservableProperty]
    public partial DataType SelectedDataSource { get; set; } = DataType.TextItems;

    public static IReadOnlyList<Layout> Layouts { get; } = [new StackLayout(), new FlowLayout(), new MasonryLayout(), new UniformGridLayout(), new WrapLayout()];

    [ObservableProperty]
    public partial Layout? SelectedLayout { get; set; } = Layouts.FirstOrDefault();

    private static IReadOnlyList<Item>? TextItems => field ??= InitializeDataSources();

    private static Item[] InitializeDataSources()
    {
        var textItems = new Item[1000];
        for (var i = 0; i < 1000; i++)
        {
            textItems[i] = new()
            {
                Value = i,
                Name = $"Item {i}",
                Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit."
            };
        }
        return textItems;
    }

    partial void OnSelectedDataSourceChanged(DataType value)
    {
        ItemTemplate = Application.Current?.FindResource($"{value}Template") as IDataTemplate;

        Items = value is DataType.TextItems ? TextItems : null;
    }

    public void LoadFolder(string folder)
    {
        if (SelectedDataSource is not DataType.AsyncImageItems and not DataType.ImageItems)
            return;
        var supported = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tif", ".tiff" };
        var items = Directory.EnumerateFiles(folder)
            .Where(f => supported.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase));
        Items = SelectedDataSource switch
        {
            DataType.AsyncImageItems => items.Select(t => new ImageUri(t)).ToArray(),
            DataType.ImageItems => items.Select(t => Bitmap.DecodeToWidth(File.OpenRead(t), 300)).ToArray(),
            _ => Items
        };
    }

    [RelayCommand]
    public async Task OpenFolder()
    {
        if (TopLevel.GetTopLevel(MainWindow.Current) is not { } topLevel)
            return;
        var folder = await topLevel.StorageProvider.OpenFolderPickerAsync(new());
        if (folder is not [{ } first] || first.TryGetLocalPath() is not { } path)
            return;
        LoadFolder(path);
    }
}

public record ImageUri(string Uri)
{
    public Task<Bitmap> BitmapAsync => Task.Run(() => Bitmap.DecodeToWidth(File.OpenRead(Uri), 300));
}

public enum DataType
{
    TextItems,
    ImageItems,
    AsyncImageItems
}