using Avalonia.Controls;
using Avalonia.Metadata;
using PropertyGenerator.Avalonia;

namespace Virtualization.Avalonia;

public partial class ItemContainer
{
    [GeneratedStyledProperty]
    public partial bool IsSelected { get; set; }

    /// <summary>
    /// Gets or sets the content to display.
    /// </summary>
    [Content]
    [GeneratedStyledProperty]
    public partial Control? Child { get; set; }

    [GeneratedDirectProperty]
    internal partial ItemContainerUserInvokeMode CanUserInvoke { get; set; } = ItemContainerUserInvokeMode.Auto;

    [GeneratedDirectProperty]
    internal partial ItemContainerUserSelectMode CanUserSelect { get; set; } = ItemContainerUserSelectMode.Auto;

    [GeneratedDirectProperty]
    internal partial ItemContainerMultiSelectMode MultiSelectMode { get; set; } = ItemContainerMultiSelectMode.Auto;

    partial void OnChildPropertyChanged(Control? oldValue, Control? newValue)
    {
        if (_rootPanel is null) return;
        if (oldValue is not null)
        {
            _rootPanel.Children.RemoveAt(_rootPanel.Children.IndexOf(oldValue));
        }
        if (newValue is not null)
        {
            _rootPanel.Children.Insert(0, newValue);
        }
    }

}