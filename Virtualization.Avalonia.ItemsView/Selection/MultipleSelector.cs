namespace Virtualization.Avalonia;

internal class MultipleSelector : SelectorBase
{
    public override void OnInteractedAction(int index, bool ctrl, bool shift)
    {
        if (SelectionModel is null) return;
        if (shift)
        {
            var anchorIndex = SelectionModel.AnchorIndex;
            var isAnchorSelected = SelectionModel.IsSelected(anchorIndex);
            var isIndexSelected = SelectionModel.IsSelected(index);

            if (isAnchorSelected != isIndexSelected)
            {
                if (isAnchorSelected)
                {
                    SelectionModel.SelectRange(anchorIndex, index);
                }
                else
                {
                    SelectionModel.DeselectRange(anchorIndex, index);
                }
            }
        }
        else if (IsSelected(index))
        {
            SelectionModel.Deselect(index);
        }
        else
        {
            SelectionModel.Select(index);
        }
    }

    public override void OnFocusedAction(int index, bool ctrl, bool shift)
    {
        if (SelectionModel is null) return;
        if (!shift) return;
        var anchorIndex = SelectionModel.AnchorIndex;
        var isAnchorSelected = SelectionModel.IsSelected(anchorIndex);

        if (isAnchorSelected)
        {
            SelectionModel.SelectRange(anchorIndex, index);
        }
        else
        {
            SelectionModel.DeselectRange(anchorIndex, index);
        }
    }
}
