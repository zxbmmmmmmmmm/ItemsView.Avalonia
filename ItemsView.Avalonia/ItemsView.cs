using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Styling;
using ItemsView.Avalonia.Helpers;
using ItemsView.Avalonia.Selection;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace ItemsView.Avalonia;

public partial class ItemsView : TemplatedControl
{
    private ScrollViewer _scrollViewer = null!;

    private ItemsRepeater _itemsRepeater = null!;

    private readonly ISelectionModel _selectionModel = new InternalSelectionModel();

    private SelectorBase _selector;

    private bool _isProcessingInteraction = false;
    public ItemsView()
    {
        UpdateSelector();
        _selectionModel.SelectionChanged += OnSelectionModelSelectionChanged;
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

    [MemberNotNull(nameof(_selector))]
    private void UpdateSelector()
    {
        _selectionModel.SingleSelect = false;

        switch (SelectionMode)
        {
            case ItemsViewSelectionMode.None:
            {
                _selectionModel.Clear();
                _selector = new NullSelector();
                break;
            }

            case ItemsViewSelectionMode.Single:
            {
                _selectionModel.SingleSelect = true;

                _selector = new SingleSelector();
                _selector.SetSelectionModel(_selectionModel);
                break;
            }

            case ItemsViewSelectionMode.Multiple:
            {
                _selector = new MultipleSelector();
                _selector.SetSelectionModel(_selectionModel);
                break;
            }

            case ItemsViewSelectionMode.Extended:
            {
                _selector = new ExtendedSelector();
                _selector.SetSelectionModel(_selectionModel);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void ApplySelectionModelSelectionChange()
    {
        // Update ItemsView properties
        SelectedItem = _selectionModel.SelectedItem;

        // For all ItemContainer children, update the IsSelected property according to the new ItemsView.SelectionModel property.

        var count = _itemsRepeater.Children.Count;

        for (var childIndex = 0; childIndex < count; childIndex++)
        {
            if (_itemsRepeater.TryGetElement(childIndex) is not ItemContainer itemContainer) continue;
            var itemIndex = _itemsRepeater.GetElementIndex(itemContainer);
            if (itemIndex < 0) continue;

            if (_selectionModel.IsSelected(itemIndex))
            {
                var canUserSelect = itemContainer.CanUserSelect;

                if (!_isProcessingInteraction || (canUserSelect & ItemContainerUserSelectMode.UserCannotSelect) == 0)
                {
                    itemContainer.IsSelected = true;
                }
                else
                {
                    // Processing a user interaction while ItemContainer.CanUserSelect is ItemContainerUserSelectMode.UserCannotSelect. Deselect that item
                    // in the selection model without touching the potential anchor.
                    _selector.DeselectWithAnchorPreservation(itemIndex);
                }
            }
            else
            {
                itemContainer.IsSelected = false;
            }
        }

        RaiseSelectionChanged();
    }
    private object? GetElementItem(Control element, out bool valueReturned)
    {
        valueReturned = false;
        if (_itemsRepeater.ItemsSourceView is not { } itemsSourceView) return null;

        var itemIndex = _itemsRepeater.GetElementIndex(element);
        if (itemIndex < 0 || itemsSourceView.Count <= itemIndex) return null;

        valueReturned = true;
        return itemsSourceView.GetAt(itemIndex);
    }

    void RaiseItemInvoked(
        Control element)
    {
        if (ItemInvoked is null) return;
        var itemInvoked = GetElementItem(element, out var itemInvokedFound);
        if (!itemInvokedFound) return;
        var itemsViewItemInvokedEventArgs = new ItemsViewItemInvokedEventArgs(itemInvoked);
        ItemInvoked.Invoke(this, itemsViewItemInvokedEventArgs);
    }

    private void RaiseSelectionChanged()
    {
        if (SelectionChanged is null) return;
        var itemsViewSelectionChangedEventArgs = new ItemsViewSelectionChangedEventArgs();
        SelectionChanged.Invoke(this, itemsViewSelectionChangedEventArgs);
    }

    private void OnSelectionModelSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs e)
    {
        ApplySelectionModelSelectionChange();
    }

    private void OnItemsRepeaterElementClearing(object? sender, ItemsRepeaterElementClearingEventArgs e)
    {
    }

    private void OnItemsRepeaterElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs e)
    {
        if (e.Element is not ItemContainer itemContainer) throw new Exception();

    }
    private void OnSourceListChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    partial void OnSelectionModePropertyChanged(ItemsViewSelectionMode newValue)
    {
        UpdateSelector();

        var count = _itemsRepeater.Children.Count;
        var multiSelectMode = ItemContainerMultiSelectMode.Auto;

        multiSelectMode |= newValue switch
        {
            ItemsViewSelectionMode.None or ItemsViewSelectionMode.Single => ItemContainerMultiSelectMode.Single,
            ItemsViewSelectionMode.Extended => ItemContainerMultiSelectMode.Extended,
            ItemsViewSelectionMode.Multiple => ItemContainerMultiSelectMode.Multiple,
            _ => throw new ArgumentOutOfRangeException(nameof(newValue), newValue, null)
        };

        var canUserSelect = newValue == ItemsViewSelectionMode.None ?
            ItemContainerUserSelectMode.Auto | ItemContainerUserSelectMode.UserCannotSelect :
            ItemContainerUserSelectMode.Auto | ItemContainerUserSelectMode.UserCanSelect;

        for (var childIndex = 0; childIndex < count; childIndex++)
        {
            var itemContainer = _itemsRepeater.TryGetElement(childIndex) as ItemContainer;

            if (itemContainer == null) continue;

            if ((itemContainer.MultiSelectMode & ItemContainerMultiSelectMode.Auto) != 0)
            {
                itemContainer.MultiSelectMode = multiSelectMode;
            }

            if ((itemContainer.CanUserSelect & ItemContainerUserSelectMode.Auto) != 0)
            {
                itemContainer.CanUserSelect = canUserSelect;
            }
        }
    }

    private ISelectionModel CreateDefaultSelectionModel()
    {
        return new InternalSelectionModel
        {
            SingleSelect = !SelectionMode.HasAllFlags(ItemsViewSelectionMode.Multiple),
        };
    }
}