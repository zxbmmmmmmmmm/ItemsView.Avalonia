namespace Virtualization.Avalonia;

internal class SingleSelector : SelectorBase
{
    private bool _followFocus = true;

    public void FollowFocus(bool followFocus)
    {
        _followFocus = followFocus;
    }

    public override void OnInteractedAction(int index, bool ctrl, bool shift)
    {
        if (SelectionModel is null) return;
        SelectionModel.SingleSelect = true;
        if (!ctrl || !IsSelected(index))
        {
            SelectionModel.Select(index);
        }
        else
        {
            SelectionModel.Deselect(index);
        }
    }

    public override void OnFocusedAction(int index, bool ctrl, bool shift)
    {
        if (SelectionModel is null) return;
        SelectionModel.SingleSelect = true;
        if (!ctrl && _followFocus)
        {
            SelectionModel.Select(index);
        }
    }
}