using Avalonia.Interactivity;

namespace ItemsView.Avalonia;

public class ItemsViewItemInvokedEventArgs : RoutedEventArgs
{
    internal ItemsViewItemInvokedEventArgs(object? invokedItem)
    {
        InvokedItem = invokedItem;
    }
    public object? InvokedItem { get; }
}