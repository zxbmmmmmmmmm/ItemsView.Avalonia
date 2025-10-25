using System.Collections;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using PropertyGenerator.Avalonia;

namespace ItemsView.Avalonia;

public partial class ItemsView
{
    [GeneratedStyledProperty]
    public partial AttachedLayout Layout { get; set; }

    [GeneratedStyledProperty]
    public partial IEnumerable? ItemsSource { get; set; }

    [GeneratedStyledProperty]
    public partial IDataTemplate? ItemTemplate { get; set; }
}