using Avalonia.Controls;

namespace Virtualization.Avalonia.Sample;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Current = this;
    }

    public static MainWindow Current { get; private set; } = null!;
}