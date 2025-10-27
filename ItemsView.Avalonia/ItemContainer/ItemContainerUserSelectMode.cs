namespace ItemsView.Avalonia;

[Flags]
internal enum ItemContainerUserSelectMode
{
    Auto = 1,
    UserCanSelect = 2,
    UserCannotSelect = 4,
}