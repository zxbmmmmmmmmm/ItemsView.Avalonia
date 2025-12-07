namespace Virtualization.Avalonia.Layouts;

/// <summary>
/// Defines constants that specify how items are sized to fill the available space.
/// </summary>
public enum UniformGridLayoutItemsStretch
{
    /// <summary>
    /// The item retains its natural size. Use of extra space is determined by the
    /// <see cref="UniformGridLayout.ItemsJustification"/> property.
    /// </summary>
    None = 0,

    /// <summary>
    /// The item is sized to fill the available space in the non-scrolling direction. Item size
    /// in the scrolling direction is not changed.
    /// </summary>
    Fill = 1,

    /// <summary>
    /// The item is sized to both fill the available space in the non-scrolling direction and
    /// maintain its aspect ratio.
    /// </summary>
    Uniform = 2,
}
