using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ItemsView.Avalonia.Layouts.FlowLayout;
using ItemsView.Avalonia.Layouts.MasonryLayout;
using ItemsView.Avalonia.Sample.Models;
using SixLabors.ImageSharp.Processing;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using StringComparer = System.StringComparer;

namespace ItemsView.Avalonia.Sample.ViewModels;

public partial class SampleViewModel : ObservableObject
{
    [ObservableProperty]
    public partial IDataTemplate? ItemTemplate { get; set; }

    [ObservableProperty]
    public partial IEnumerable? Items { get; set; }

    [ObservableProperty]
    public partial AttachedLayout? Layout { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<DataSource> DataSources { get; set; } = [];

    [ObservableProperty]
    public partial DataSource? SelectedDataSource { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<AttachedLayout> Layouts { get; set; } = [];

    [ObservableProperty]
    public partial AttachedLayout? SelectedLayout { get; set; }

    public SampleViewModel()
    {
        InitializeDataSources();
        InitializeLayouts();

        SelectedDataSource = DataSources.FirstOrDefault();
        SelectedLayout = Layouts.FirstOrDefault();
    }

    private void InitializeDataSources()
    {
        var itemTemplate = Application.Current!.FindResource("ItemTemplate") as IDataTemplate;
        var imageTemplate = Application.Current!.FindResource("ImageTemplate") as IDataTemplate;

        var items = new ObservableCollection<Item>();
        for (var i = 0; i < 1000; i++)
        {
            items.Add(new Item
            {
                Value = i,
                Name = $"Item {i}",
                Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit."
            });
        }
        DataSources.Add(new DataSource { Name = "Default Items", Items = items, ItemTemplate = itemTemplate! });

        var images = new ObservableCollection<Bitmap>();
        DataSources.Add(new DataSource { Name = "Gallery Images", Items = images, ItemTemplate = imageTemplate! });
    }

    private void InitializeLayouts()
    {
        Layouts.Add(new StackLayout());
        Layouts.Add(new FlowLayout());
        Layouts.Add(new MasonryLayout());
        Layouts.Add(new WrapLayout());
        Layouts.Add(new UniformGridLayout());
    }

    partial void OnSelectedDataSourceChanged(DataSource? value)
    {
        if (value is null) return;

        Items = value.Items;
        ItemTemplate = value.ItemTemplate;
    }

    partial void OnSelectedLayoutChanged(AttachedLayout? value)
    {
        Layout = value;
    }

    public async Task LoadFolderAsync(string folder)
    {
        var dataSource = DataSources.FirstOrDefault(ds => ds.Name == "Gallery Images");
        if (dataSource?.Items is not ObservableCollection<Bitmap> images) return;

        images.Clear();
        var supported = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tif", ".tiff" };
        var files = Directory.EnumerateFiles(folder)
            .Where(f => supported.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
            .ToArray();

        foreach (var file in files)
        {
            try
            {
                await using var stream = File.OpenRead(file);
                using var image = await SixLabors.ImageSharp.Image.LoadAsync(stream);

                const int thumbnailSize = 300;
                var options = new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(thumbnailSize, thumbnailSize),
                    Mode = ResizeMode.Max
                };
                image.Mutate(x => x.Resize(options));

                await using var memoryStream = new MemoryStream();
                await image.SaveAsPngAsync(memoryStream);
                memoryStream.Position = 0;

                var bmp = new Bitmap(memoryStream);
                images.Add(bmp);
            }
            catch
            {
                // ignore invalid images
            }
        }
    }

    [RelayCommand]
    public async Task OpenFolder()
    {
        var dialog = new OpenFolderDialog();
        var folder = await dialog.ShowAsync(MainWindow.Current);
        if (!string.IsNullOrWhiteSpace(folder))
        {
            await LoadFolderAsync(folder);
        }
    }

}
public class DataSource
{
    public required string Name { get; set; }
    public required IEnumerable Items { get; set; }
    public required IDataTemplate ItemTemplate { get; set; }
}