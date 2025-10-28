using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace ItemsView.Avalonia;

public partial class ItemsView
{
    private readonly HashSet<Key> _keysDown = new();

    private Rect _keyboardNavigationReferenceRect = new Rect(-1.0f, -1.0f, -1.0f, -1.0f);

    // Incremented in SetFocusElementIndex when a navigation key processing causes a new item to get focus.
    // This will trigger an OnScrollViewBringingIntoView call where it is decremented.
    // Used to delay a navigation key processing until the content has settled on a new viewport.
    private byte _navigationKeyBringIntoViewPendingCount = 0;

    private bool _isProcessingInteraction = false;

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        _keysDown.Add(e.Key);
    }

    protected override void OnKeyUp(KeyEventArgs e)
    {
        base.OnKeyUp(e);
        _keysDown.Remove(e.Key);
    }

    public bool IsKeyDown(Key key)
    {
        return _keysDown.Contains(key);
    }

    bool ProcessInteraction(
        Control control,
        NavigationMethod focusState)
    {
        var index = GetElementIndex(control);

        if (index < 0) throw new Exception();

        // When the focusState is Pointer, the element not only gets focus but is also brought into view by SetFocusElementIndex's StartBringIntoView call.
        var handled = SetCurrentElementIndex(index, focusState, true /*forceKeyboardNavigationReferenceReset*/, focusState == NavigationMethod.Pointer /*startBringIntoView*/);

        var isCtrlDown = IsKeyDown(Key.LeftCtrl) || IsKeyDown(Key.RightCtrl);
        var isShiftDown = IsKeyDown(Key.LeftShift) || IsKeyDown(Key.RightShift);

        try
        {
            _isProcessingInteraction = true;
            _selector.OnInteractedAction(index, isCtrlDown, isShiftDown);
        }
        finally
        {
            _isProcessingInteraction = false;
        }

        return handled;
    }

    bool SetFocusElementIndex(
        int index,
        NavigationMethod focusState,
        bool startBringIntoView = false,
        bool expectBringIntoView = false)
    {
        if (index != -1 /*&& focusState != NavigationMethod.Unfocused*/)
        {
            if (TryGetElement(index) is { } element)
            {
                bool success = element.Focus(focusState);

                if (success)
                {
                    if (_scrollViewer is { } scrollView)
                    {
                        if (expectBringIntoView)
                        {
                            _navigationKeyBringIntoViewPendingCount++;
                        }
                        else if (startBringIntoView)
                        {
                            element.BringIntoView();
                        }
                    }
                }

                return success;
            }
        }

        return false;
    }
}