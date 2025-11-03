using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using ItemsView.Avalonia.Layouts.MasonryLayout;

namespace ItemsView.Avalonia.Sample;

public partial class MasonryGalleryWindow : Window
{
    public MasonryGalleryViewModel ViewModel { get; } = new();

    public MasonryGalleryWindow()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }

    private async void OpenFolderButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog();
        var folder = await dialog.ShowAsync(this);
        if (string.IsNullOrWhiteSpace(folder)) return;

        await ViewModel.LoadFolderAsync(folder);
    }
}

public partial class MasonryGalleryViewModel : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<Bitmap> Images { get; private set; } = [];

    [ObservableProperty]
    public partial MasonryLayoutItemsStretch ItemsStretch { get; set; } = MasonryLayoutItemsStretch.Stretch;

    [ObservableProperty]
    public partial double DesiredColumnWidth { get; set; } = 250;

    [ObservableProperty]
    public partial double ColumnSpacing { get; set; } = 8;

    [ObservableProperty]
    public partial double RowSpacing { get; set; } = 8;

    public async System.Threading.Tasks.Task LoadFolderAsync(string folder)
    {
        Images.Clear();
        var supported = new[] { ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp", ".tif", ".tiff" };
        var files = Directory.EnumerateFiles(folder)
        .Where(f => supported.Contains(Path.GetExtension(f), StringComparer.OrdinalIgnoreCase))
        .ToArray();

        foreach (var file in files)
        {
            try
            {
                await using var stream = File.OpenRead(file);
                var bmp = new Bitmap(stream);
                Images.Add(bmp);
            }
            catch
            {
                // ignore invalid images
            }
        }
    }
}
