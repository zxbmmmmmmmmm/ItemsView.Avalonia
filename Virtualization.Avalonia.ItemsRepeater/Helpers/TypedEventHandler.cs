namespace Virtualization.Avalonia;

/// <summary>
/// EventHandler delegate with explicit Type
/// </summary>
public delegate void EventHandler<TSender, TResult>(TSender sender, TResult args);
