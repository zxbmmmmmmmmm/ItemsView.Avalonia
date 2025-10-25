namespace ItemsView.Avalonia;

internal class ItemContainerInvokedEventArgs
{
    public ItemContainerInvokedEventArgs(ItemContainerInteractionTrigger interactionTrigger, object? originalSource)
    {
        InteractionTrigger = interactionTrigger;
        OriginalSource = originalSource;
    }

    public object? OriginalSource { get; }

    public ItemContainerInteractionTrigger InteractionTrigger { get; }

    public bool Handled { get; set; }
}