namespace ItemsView.Avalonia.Selection;

internal class ExtendedSelector : SelectorBase
{
    public override void OnInteractedAction(int index, bool ctrl, bool shift)
    {
        if (SelectionModel is null) return;
        if (shift)
        {
            var anchorIndex = SelectionModel.AnchorIndex;
            SelectionModel.Clear();
            SelectionModel.AnchorIndex = anchorIndex;
            SelectionModel.SelectRange(anchorIndex, index);
        }
        else if (ctrl)
        {
            if (IsSelected(index))
            {
                SelectionModel.Deselect(index);
            }
            else
            {
                SelectionModel.Select(index);
            }
        }
        else
        {
            // Only clear selection if interacting with a different item.
            if (!IsSelected(index))
            {
                SelectionModel.Clear();
                SelectionModel.Select(index);
            }
        }
    }

    public override void OnFocusedAction(int index, bool ctrl, bool shift)
    {
        if (SelectionModel is null) return;
        if (shift && ctrl)
        {
            SelectionModel.SelectRange(SelectionModel.AnchorIndex, index);
        }
        else if (shift)
        {
            var anchorIndex = SelectionModel.AnchorIndex;
            SelectionModel.Clear();
            SelectionModel.AnchorIndex = anchorIndex;
            SelectionModel.SelectRange(anchorIndex, index);
        }
        else if (!ctrl)
        {
            SelectionModel.Clear();
            SelectionModel.Select(index);
        }
    }
}