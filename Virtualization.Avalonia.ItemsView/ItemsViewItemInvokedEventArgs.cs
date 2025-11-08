using Avalonia.Interactivity;

namespace Virtualization.Avalonia;

public class ItemsViewItemInvokedEventArgs : RoutedEventArgs
{
    internal ItemsViewItemInvokedEventArgs(object? invokedItem)
    {
        InvokedItem = invokedItem;
    }
    public object? InvokedItem { get; }
}