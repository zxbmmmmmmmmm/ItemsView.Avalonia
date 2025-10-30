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
    public partial ItemsViewSelectionMode SelectionMode { get; set; }

    [GeneratedStyledProperty]
    public partial bool IsItemInvokedEnabled { get; set; }

    [GeneratedDirectProperty(DefaultBindingMode = BindingMode.TwoWay, EnableDataValidation = true)]
    public partial int SelectedIndex { get; set; } = -1;

    [GeneratedDirectProperty(DefaultBindingMode = BindingMode.TwoWay, EnableDataValidation = true)]
    public partial object? SelectedItem { get; set; }

    [GeneratedDirectProperty]
    public partial int CurrentItemIndex { get; set; } = -1;

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

    partial void OnItemsSourcePropertyChanged(IEnumerable? newValue)
    {
        // When the inner ItemsRepeater has not been loaded yet, set the selection models' Source
        // right away as OnItemsRepeaterItemsSourceChanged will not be invoked.
        // There is no reason to delay the updates to OnItemsRepeaterItemsSourceChanged
        // in this case since ItemsRepeater and its children do not exist yet.

        if (_itemsRepeater is null)
        {
            var itemsSource = ItemsSource;
            _selectionModel.Source = itemsSource;
            _currentSelectionModel.Source = itemsSource;
        }
    }
}