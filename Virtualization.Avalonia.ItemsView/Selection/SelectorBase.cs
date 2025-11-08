using Avalonia.Controls;
using Avalonia.Controls.Selection;

namespace Virtualization.Avalonia;

internal class SelectorBase
{
    public virtual void OnInteractedAction(int index, bool ctrl, bool shift) { }
    public virtual void OnFocusedAction(int index, bool ctrl, bool shift) { }

    protected ISelectionModel? SelectionModel { get; private set; }

    public void SetSelectionModel(ISelectionModel selectionModel)
    {
        SelectionModel = selectionModel;
    }

    public void DeselectWithAnchorPreservation(int index)
    {
        if (index is -1) throw new Exception();

        if (SelectionModel is not null)
        {
            var anchorIndex = SelectionModel.AnchorIndex;

            SelectionModel.Deselect(index);

            if (anchorIndex != -1)
            {
                SelectionModel.AnchorIndex = anchorIndex;
            }
        }
    }

    protected bool IsSelected(int index)
    {
        var isSelected = false;

        if (SelectionModel is not null)
        {
            isSelected = SelectionModel.IsSelected(index);
        }

        return isSelected;
    }

    protected virtual bool CanSelect(int index)
    {
        return SelectionModel is not null;
    }

    public virtual void SelectAll()
    {
        SelectionModel?.SelectAll();
    }

    public virtual void Clear()
    {
        SelectionModel?.Clear();
    }
}