using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Layout;
using PropertyGenerator.Avalonia;
using System.Collections;

namespace ItemsView.Avalonia;

public partial class ItemsView
{
    [GeneratedStyledProperty]
    public partial AttachedLayout Layout { get; set; }

    [GeneratedStyledProperty]
    public partial IEnumerable? ItemsSource { get; set; }

    [GeneratedStyledProperty]
    public partial IDataTemplate? ItemTemplate { get; set; }

    [GeneratedStyledProperty]
    public partial SelectionMode SelectionMode { get; set; }

    [GeneratedDirectProperty(DefaultValue = -1,DefaultBindingMode = BindingMode.TwoWay, EnableDataValidation = true)]
    public partial int SelectedIndex { get; set; }

    [GeneratedDirectProperty(DefaultBindingMode = BindingMode.TwoWay, EnableDataValidation = true)]
    public partial object? SelectedItem { get; set; }

    /// <summary>
    /// Defines the <see cref="SelectionChanged"/> event.
    /// </summary>
    public static readonly RoutedEvent<ItemsViewSelectionChangedEventArgs> SelectionChangedEvent =
        RoutedEvent.Register<ItemsView, ItemsViewSelectionChangedEventArgs>(
            nameof(SelectionChanged),
            RoutingStrategies.Bubble);

    /// <summary>
    /// Defines the <see cref="ItemInvoked"/> event.
    /// </summary>
    public static readonly RoutedEvent<ItemsViewItemInvokedEventArgs> ItemInvokedEvent =
        RoutedEvent.Register<ItemsView, ItemsViewItemInvokedEventArgs>(
            nameof(ItemInvoked),
            RoutingStrategies.Bubble);

    public event EventHandler<ItemsViewItemInvokedEventArgs>? ItemInvoked;
    public event EventHandler<ItemsViewSelectionChangedEventArgs>? SelectionChanged;
}