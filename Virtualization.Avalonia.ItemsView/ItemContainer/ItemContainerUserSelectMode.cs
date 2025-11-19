using System;

namespace Virtualization.Avalonia;

[Flags]
internal enum ItemContainerUserSelectMode
{
    Auto = 1,
    UserCanSelect = 2,
    UserCannotSelect = 4,
}
