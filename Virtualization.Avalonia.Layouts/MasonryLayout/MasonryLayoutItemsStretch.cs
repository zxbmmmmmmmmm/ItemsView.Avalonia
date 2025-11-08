namespace Virtualization.Avalonia.Layouts;

/// <summary>
/// Defines constants that specify how items are sized to fill the available space in a <see cref="MasonryLayout"/>.
/// </summary>
public enum MasonryLayoutItemsStretch
{
    /// <summary>
    /// The items' width is determined by the <see cref="MasonryLayout.MinColumnWidth"/>.
    /// </summary>
    Start,
    End,
    Center,
    Justify,
    /// <summary>
    /// The items' width is determined by the parent's width. The minimum width is determined by the <see cref="MasonryLayout.MinColumnWidth"/>.
    /// </summary>
    Stretch,
}