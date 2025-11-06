using Avalonia.Controls;
using Avalonia.Interactivity;
using System;

namespace ItemsView.Avalonia.Sample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Current = this;
        }
        public static MainWindow Current { get; private set; }

        private void OpenMasonryGallery_OnClick(object? sender, RoutedEventArgs e)
        {
            var win = new MasonryGalleryWindow();
            win.Show();
        }
    }
}