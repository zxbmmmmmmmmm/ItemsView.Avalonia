using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using ItemsView.Avalonia.Selection;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using ItemsView.Avalonia.Helpers;

namespace ItemsView.Avalonia;

public partial class ItemsView : TemplatedControl
{
    private ScrollViewer _scrollViewer = null!;

    private ItemsRepeater _itemsRepeater = null!;

    private ISelectionModel _selectionModel = new InternalSelectionModel();

    private UpdateState? _updateState;

    /// <summary>
    /// Gets or sets the model that holds the current selection.
    /// </summary>
    [AllowNull]
    protected ISelectionModel Selection
    {
        get => _updateState?.Selection.HasValue == true ?
                _updateState.Selection.Value :
                GetOrCreateSelectionModel();
        set
        {
            value ??= CreateDefaultSelectionModel();

            if (_updateState is object)
            {
                _updateState.Selection = new Optional<ISelectionModel>(value);
            }
            else if (_selection != value)
            {
                if (value.Source != null && value.Source != ItemsView.Source)
                {
                    throw new ArgumentException(
                        "The supplied ISelectionModel already has an assigned Source but this " +
                        "collection is different to the Items on the control.");
                }

                var oldSelection = _selection?.SelectedItems.ToArray();
                DeinitializeSelectionModel(_selection);
                _selection = value;

                if (oldSelection?.Length > 0)
                {
                    RaiseEvent(new SelectionChangedEventArgs(
                        SelectionChangedEvent,
                        oldSelection,
                        Array.Empty<object>()));
                }

                InitializeSelectionModel(_selection);
                var selectedItems = SelectedItems;
                _oldSelectedItems.TryGetTarget(out var oldSelectedItems);
                if (oldSelectedItems != selectedItems)
                {
                    RaisePropertyChanged(SelectedItemsProperty, oldSelectedItems, selectedItems);
                    _oldSelectedItems.SetTarget(selectedItems);
                }
            }
        }
    }
    private void InitializeSelectionModel(ISelectionModel model)
    {
        if (_updateState is null)
        {
            TryInitializeSelectionSource(model, false);
        }

        model.PropertyChanged += OnSelectionModelPropertyChanged;
        model.SelectionChanged += OnSelectionModelSelectionChanged;
        model.LostSelection += OnSelectionModelLostSelection;

        if (model.SingleSelect)
        {
            SelectionMode &= ~SelectionMode.Multiple;
        }
        else
        {
            SelectionMode |= SelectionMode.Multiple;
        }

        _oldSelectedIndex = model.SelectedIndex;
        _oldSelectedItem.Target = model.SelectedItem;

        if (_updateState is null && AlwaysSelected && model.Count == 0)
        {
            model.SelectedIndex = 0;
        }

        UpdateContainerSelection();

        if (SelectedIndex != -1)
        {
            RaiseEvent(new SelectionChangedEventArgs(
                SelectionChangedEvent,
                Array.Empty<object>(),
                Selection.SelectedItems.ToArray()));
        }
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var scrollViewer = e.NameScope.Find<ScrollViewer>("PART_ScrollView");
        var itemsRepeater = e.NameScope.Find<ItemsRepeater>("PART_ItemsRepeater");
        if (scrollViewer is null || itemsRepeater is null) throw new Exception();
        UpdateScrollViewer(scrollViewer);
        UpdateItemsRepeater(itemsRepeater);
    }

    private void UpdateScrollViewer(ScrollViewer scrollViewer)
    {
        _scrollViewer = scrollViewer;
    }

    private void UpdateItemsRepeater(ItemsRepeater itemsRepeater)
    {
        UnhookItemsRepeaterEvents();
        _itemsRepeater = itemsRepeater;
        HookItemsRepeaterEvents();
        HookItemsSourceViewEvents();
    }

    private void HookItemsRepeaterEvents()
    {
        _itemsRepeater.ElementPrepared += OnItemsRepeaterElementPrepared;
        _itemsRepeater.ElementClearing += OnItemsRepeaterElementClearing;
    }

    private void HookItemsSourceViewEvents()
    {
        if (_itemsRepeater.ItemsSourceView is not { } itemsSourceView) return;
        itemsSourceView.CollectionChanged += OnSourceListChanged;
    }


    private void UnhookItemsRepeaterEvents()
    {
        
    }

    private void OnItemsRepeaterElementClearing(object? sender, ItemsRepeaterElementClearingEventArgs e)
    {
        throw new NotImplementedException();
    }

    private void OnItemsRepeaterElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs e)
    {
        if (e.Element is not ItemContainer itemContainer) throw new Exception(); 
    }
    private void OnSourceListChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc/>
    public override void BeginInit()
    {
        base.BeginInit();
        BeginUpdating();
    }

    /// <inheritdoc/>
    public override void EndInit()
    {
        base.EndInit();
        EndUpdating();
    }

    private void BeginUpdating()
    {
        _updateState ??= new UpdateState();
        _updateState.UpdateCount++;
    }

    private void EndUpdating()
    {

    }

    private ISelectionModel CreateDefaultSelectionModel()
    {
        return new InternalSelectionModel
        {
            SingleSelect = !SelectionMode.HasAllFlags(SelectionMode.Multiple),
        };
    }

    // When in a BeginInit..EndInit block, or when the DataContext is updating, we need to
    // defer changes to the selection model because we have no idea in which order properties
    // will be set. Consider:
    //
    // - Both Items and SelectedItem are bound
    // - The DataContext changes
    // - The binding for SelectedItem updates first, producing an item
    // - Items is searched to find the index of the new selected item
    // - However Items isn't yet updated; the item is not found
    // - SelectedIndex is incorrectly set to -1
    //
    // This logic cannot be encapsulated in SelectionModel because the selection model can also
    // be bound, consider:
    //
    // - Both Items and Selection are bound
    // - The DataContext changes
    // - The binding for Items updates first
    // - The new items are assigned to Selection.Source
    // - The binding for Selection updates, producing a new SelectionModel
    // - Both the old and new SelectionModels have the incorrect Source
    private class UpdateState
    {
        public int UpdateCount { get; set; }
        public Optional<ISelectionModel> Selection { get; set; }
        public Optional<IList?> SelectedItems { get; set; }
        public Optional<int> SelectedIndex { get; set; }
        public Optional<object?> SelectedItem { get; set; }
        public Optional<object?> SelectedValue { get; set; }
    }
}