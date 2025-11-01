using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Styling;
using ItemsView.Avalonia.Helpers;
using ItemsView.Avalonia.Selection;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Linq;

namespace ItemsView.Avalonia;

public partial class ItemsView : TemplatedControl
{
    private ScrollViewer _scrollViewer = null!;
    private ItemsRepeater _itemsRepeater = null!;

    // 新增：删除/虚拟化后待补焦的索引
    private int _pendingFocusIndex = -1;

    private readonly ISelectionModel _selectionModel = new InternalSelectionModel();
    private readonly ISelectionModel _currentSelectionModel = new InternalSelectionModel { SingleSelect = true };

    private SelectorBase _selector;

    private readonly HashSet<ItemContainer> _itemContainers = [];
    public ItemsView()
    {
        UpdateSelector();
        _selectionModel.SelectionChanged += OnSelectionModelSelectionChanged;
        _currentSelectionModel.SelectionChanged += OnCurrentElementSelectionModelSelectionChanged;
    }

    public void Select(int itemIndex)
    {
        _selectionModel.Select(itemIndex);
    }

    public void Deselect(int itemIndex)
    {
        _selectionModel.Deselect(itemIndex);
    }

    public bool IsSelected(int itemIndex)
    {
        return _selectionModel.IsSelected(itemIndex);
    }

    public void SelectAll()
    {
        _selectionModel.SelectAll();
    }

    public void DeselectAll()
    {
        _selectionModel.Clear();
    }

    public void InvertSelection()
    {
        if (_itemsRepeater.ItemsSourceView is { } itemsSourceView)
        {
            var selectedIndexes = _selectionModel.SelectedIndexes;
            int indexEnd = itemsSourceView.Count - 1;

            // We loop backwards through the selected indices so we can deselect as we go
            for (int i = selectedIndexes.Count - 1; i >= 0; i--)
            {
                var index = selectedIndexes[i];
                // Select all the unselected items
                if (index < indexEnd)
                {
                    _selectionModel.SelectRange(index + 1, indexEnd);
                }

                _selectionModel.Deselect(index);
                indexEnd = index - 1;
            }

            // Select the remaining unselected items at the beginning of the collection
            if (indexEnd >= 0)
            {
                _selectionModel.SelectRange(0, indexEnd);
            }
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
        _scrollViewer.BringIntoViewOnFocusChange = false;
        _scrollViewer.ScrollChanged += (s, e) =>
        {

        };
    }

    private void UpdateItemsRepeater(ItemsRepeater itemsRepeater)
    {
        UnhookItemsRepeaterEvents();
        UnhookItemsSourceViewEvents();
        _itemsRepeater = itemsRepeater;
        KeyboardNavigation.SetTabNavigation(_itemsRepeater, KeyboardNavigationMode.Continue);

        HookItemsRepeaterEvents();
        HookItemsSourceViewEvents();
    }



    private void OnItemsRepeaterPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == ItemsRepeater.ItemsSourceProperty)
        {

            HookItemsSourceViewEvents();
            var itemsSource = ItemsSource;

            // Updating the selection model's ItemsSource here rather than earlier in OnPropertyChanged/OnItemsSourceChanged so that
            // Layout.OnItemsChangedCore is executed before OnSelectionModelSelectionChanged. Otherwise OnSelectionModelSelectionChanged
            // would operate on out-of-date ItemsRepeater children.
            _selectionModel.Source = itemsSource;
            _currentSelectionModel.Source = itemsSource;
        }
    }

    private void HookItemsRepeaterEvents()
    {
        _itemsRepeater.ElementPrepared += OnItemsRepeaterElementPrepared;
        _itemsRepeater.ElementClearing += OnItemsRepeaterElementClearing;
        _itemsRepeater.PropertyChanged += OnItemsRepeaterPropertyChanged;
    }

    private void UnhookItemsRepeaterEvents()
    {
        if (_itemsRepeater is null) return;
        _itemsRepeater.ElementPrepared -= OnItemsRepeaterElementPrepared;
        _itemsRepeater.ElementClearing -= OnItemsRepeaterElementClearing;
        _itemsRepeater.PropertyChanged -= OnItemsRepeaterPropertyChanged;
    }

    private void HookItemsSourceViewEvents()
    {
        if (_itemsRepeater.ItemsSourceView is not { } itemsSourceView) return;
        itemsSourceView.CollectionChanged += OnSourceListChanged;
    }
    private void UnhookItemsSourceViewEvents()
    {
        if (_itemsRepeater?.ItemsSourceView is not { } itemsSourceView) return;
        itemsSourceView.CollectionChanged -= OnSourceListChanged;
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
            if (_itemsRepeater.Children?[childIndex] is not ItemContainer itemContainer) continue;
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
    private Control? TryGetElement(int index)
    {
        return _itemsRepeater.TryGetElement(index);
    }

    private int GetElementIndex(Control element)
    {
        return _itemsRepeater.GetElementIndex(element);
    }


    private int GetCurrentElementIndex()
    {
        return _currentSelectionModel.SelectedIndex;
    }

    bool CanRaiseItemInvoked(
        ItemContainerInteractionTrigger interactionTrigger,
        ItemContainer itemContainer)
    {

        var canUserInvoke = itemContainer.CanUserInvoke;


        if ((canUserInvoke & (ItemContainerUserInvokeMode.UserCannotInvoke | ItemContainerUserInvokeMode.UserCanInvoke)) != (ItemContainerUserInvokeMode.UserCanInvoke))
        {
            return false;
        }

        var cannotRaiseItemInvoked =
            (!IsItemInvokedEnabled ||
             (SelectionMode == ItemsViewSelectionMode.None && interactionTrigger == ItemContainerInteractionTrigger.DoubleTap) ||
             (SelectionMode != ItemsViewSelectionMode.None && (interactionTrigger == ItemContainerInteractionTrigger.Tap || interactionTrigger == ItemContainerInteractionTrigger.SpaceKey)));

        var canRaiseItemInvoked = !cannotRaiseItemInvoked;

        return canRaiseItemInvoked;
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

    private void OnItemsRepeaterElementPrepared(object? sender, ItemsRepeaterElementPreparedEventArgs e)
    {
        if (e.Element is not ItemContainer itemContainer) throw new Exception();

        var index = e.Index;
        if ((itemContainer.CanUserInvoke & ItemContainerUserInvokeMode.Auto) != 0)
        {
            var canUserInvoke = ItemContainerUserInvokeMode.Auto;

            canUserInvoke |= IsItemInvokedEnabled ? ItemContainerUserInvokeMode.UserCanInvoke : ItemContainerUserInvokeMode.UserCannotInvoke;

            itemContainer.CanUserInvoke = canUserInvoke;
        }

        if ((itemContainer.MultiSelectMode & ItemContainerMultiSelectMode.Auto) != 0)
        {
            var multiSelectMode = ItemContainerMultiSelectMode.Auto;

            multiSelectMode |= SelectionMode switch
            {
                ItemsViewSelectionMode.None or ItemsViewSelectionMode.Single => ItemContainerMultiSelectMode.Single,
                ItemsViewSelectionMode.Extended => ItemContainerMultiSelectMode.Extended,
                ItemsViewSelectionMode.Multiple => ItemContainerMultiSelectMode.Multiple,
                _ => throw new ArgumentOutOfRangeException()
            };

            itemContainer.MultiSelectMode = multiSelectMode;
        }

        if ((itemContainer.CanUserSelect & ItemContainerUserSelectMode.Auto) != 0)
        {
            ItemContainerUserSelectMode canUserSelect = ItemContainerUserSelectMode.Auto;

            canUserSelect |= SelectionMode == ItemsViewSelectionMode.None ? ItemContainerUserSelectMode.UserCannotSelect : ItemContainerUserSelectMode.UserCanSelect;

            itemContainer.CanUserSelect = canUserSelect;
        }

        var isSelectionModelSelected = _selectionModel.IsSelected(index);

        if (itemContainer.IsSelected)
        {
            // The ItemsSource may be a list of ItemContainers, some of them having IsSelected==True. Account for this situation
            // by updating the selection model accordingly. Only selected containers are pushed into the selection model to avoid
            // clearing any potential selections already present in that model, which are pushed into the ItemContainers next.
            if (!isSelectionModelSelected && SelectionMode != ItemsViewSelectionMode.None)
            {
                // When SelectionMode is None, ItemContainer.IsSelected will be reset below.
                // For all other selection modes, simply select the item.
                // No need to go through the SingleSelector, MultipleSelector or ExtendedSelector policy.
                _selectionModel.Select(index);

                // Access the new selection status for the same ItemContainer so it can be updated accordingly below.
                isSelectionModelSelected = _selectionModel.IsSelected(index);
            }
        }

        itemContainer.IsSelected = isSelectionModelSelected;

        SetItemsViewItemContainerRevokers(itemContainer);

        // 新增：若该索引是待补焦的目标，在容器准备好后再设置焦点
        if (_pendingFocusIndex == index)
        {
            // 非指针交互，不触发 BringIntoView
            SetFocusElementIndex(index, NavigationMethod.Tab, startBringIntoView: false, expectBringIntoView: false);
            _pendingFocusIndex = -1;
        }

        if (_itemsRepeater.ItemsSourceView is { } itemsSourceView)
        {
            itemContainer.SetValue(AutomationProperties.PositionInSetProperty, index + 1);
            itemContainer.SetValue(AutomationProperties.SizeOfSetProperty, itemsSourceView.Count);
        }
    }

    private void OnItemsRepeaterElementClearing(object? sender, ItemsRepeaterElementClearingEventArgs args)
    {
        if (args.Element is not ItemContainer itemContainer) throw new Exception();

        // Clear all the revokers first before touching ItemContainer properties to avoid side effects.
        // For example, if you clear IsSelected before clearing revokers, we will listen to that change and
        // update SelectionModel which is incorrect.
        ClearItemsViewItemContainerRevokers(itemContainer);

        if ((itemContainer.CanUserInvoke & ItemContainerUserInvokeMode.Auto) != 0)
        {
            itemContainer.CanUserInvoke = ItemContainerUserInvokeMode.Auto;
        }

        if ((itemContainer.MultiSelectMode & ItemContainerMultiSelectMode.Auto) != 0)
        {
            itemContainer.MultiSelectMode = ItemContainerMultiSelectMode.Auto;
        }

        if ((itemContainer.CanUserSelect & ItemContainerUserSelectMode.Auto) != 0)
        {
            itemContainer.CanUserSelect = ItemContainerUserSelectMode.Auto;
        }

        itemContainer.IsSelected = false;

        itemContainer.ClearValue(AutomationProperties.PositionInSetProperty);
        itemContainer.ClearValue(AutomationProperties.SizeOfSetProperty);
    }

    private void SetItemsViewItemContainerRevokers(ItemContainer itemContainer)
    {
        if (_itemContainers.Add(itemContainer))
        {
            itemContainer.KeyDown += OnItemsViewElementKeyDown;
            itemContainer.GotFocus += OnItemsViewElementGettingFocus;
            itemContainer.ItemInvoked += OnItemsViewItemContainerItemInvoked;
            itemContainer.PropertyChanged += OnItemsViewItemContainerPropertyChanged;
        }
    }
    void ClearItemsViewItemContainerRevokers(
        ItemContainer itemContainer)
    {
        if (_itemContainers.Remove(itemContainer))
        {
            itemContainer.KeyDown -= OnItemsViewElementKeyDown;
            itemContainer.GotFocus -= OnItemsViewElementGettingFocus;
            itemContainer.ItemInvoked -= OnItemsViewItemContainerItemInvoked;
            itemContainer.PropertyChanged -= OnItemsViewItemContainerPropertyChanged;
        }
    }

    private void OnItemsViewItemContainerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ItemContainer.IsSelectedProperty) return;

        if (sender is not ItemContainer itemContainer) throw new Exception();
        var itemIndex = GetElementIndex(itemContainer);
        if (itemIndex is -1) return;

        var isSelectionModelSelected = _selectionModel.IsSelected(itemIndex);

        if (itemContainer.IsSelected)
        {
            if (!isSelectionModelSelected)
            {
                if (SelectionMode == ItemsViewSelectionMode.None)
                {
                    // Permission denied.
                    itemContainer.IsSelected = false;
                }
                else
                {
                    // For all other selection modes, simply select the item.
                    // No need to go through the SingleSelector, MultipleSelector or ExtendedSelector policy.
                    _selectionModel.Select(itemIndex);
                }
            }
        }
        else
        {
            if (isSelectionModelSelected)
            {
                // For all selection modes, simply deselect the item & preserve the anchor if any.
                _selector.DeselectWithAnchorPreservation(itemIndex);
            }
        }
    }

    private void OnItemsViewItemContainerItemInvoked(ItemContainer? itemContainer, ItemContainerInvokedEventArgs e)
    {
        if (itemContainer is null) return;

        var interactionTrigger = e.InteractionTrigger;
        bool handled = e.Handled;

        switch (interactionTrigger)
        {
            case ItemContainerInteractionTrigger.PointerReleased:
                {
                    handled |= ProcessInteraction(itemContainer, NavigationMethod.Pointer);
                    break;
                }

            case ItemContainerInteractionTrigger.EnterKey:
            case ItemContainerInteractionTrigger.SpaceKey:
                {
                    handled |= ProcessInteraction(itemContainer, NavigationMethod.Tab);
                    break;
                }

            case ItemContainerInteractionTrigger.Tap:
            case ItemContainerInteractionTrigger.DoubleTap:
            case ItemContainerInteractionTrigger.AutomationInvoke:
            case ItemContainerInteractionTrigger.PointerPressed:// TODO: Handle Tap/DoubleTap in ItemContainer
                {
                    break;
                }

            default:
                {
                    return;
                }
        }

        if (!e.Handled &&
            interactionTrigger != ItemContainerInteractionTrigger.PointerReleased &&
            CanRaiseItemInvoked(interactionTrigger, itemContainer))
        {
            if (GetElementIndex(itemContainer) == -1) throw new Exception();

            RaiseItemInvoked(itemContainer);
        }

        e.Handled = handled;
    }



    bool SetCurrentElementIndex(
        int index,
        NavigationMethod focusState,
        bool forceKeyboardNavigationReferenceReset,
        bool startBringIntoView = false,
        bool expectBringIntoView = false)
    {
        var currentElementIndex = GetCurrentElementIndex();

        if (index != currentElementIndex)
        {
            if (index == -1)
            {
                _currentSelectionModel.Clear();
            }
            else
            {
                _currentSelectionModel.Select(index);
            }

            if (index == -1 || Math.Abs(_keyboardNavigationReferenceRect.X - (-1.0)) < 0.01 || forceKeyboardNavigationReferenceReset)
            {
                //UpdateKeyboardNavigationReference();
            }
        }

        return SetFocusElementIndex(index, focusState, startBringIntoView, expectBringIntoView);
    }

    // Raised when the current element index changed.
    void OnCurrentElementSelectionModelSelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs args)
    {
        var currentElementIndex = GetCurrentElementIndex();

        CurrentItemIndex = currentElementIndex;
    }

    private void OnSourceListChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (_itemsRepeater.ItemsSourceView is not { } itemsSourceView)
            return;

        // 仅处理 Remove/Reset，其他保持当前焦点（SelectionModel 通常会处理索引偏移）
        switch (e.Action)
        {
            case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
            {
                var current = GetCurrentElementIndex();
                var removedIndex = e.OldStartingIndex;
                var removedCount = e.OldItems?.Count ?? 1;
                var newCount = itemsSourceView.Count; // 已经是移除后的数量

                // 计算新的焦点索引：
                // 1) 当前焦点位于被移除区间：将焦点放到移除起点（该位置的新元素）
                //    若超界，则退到前一个索引；
                // 2) 当前焦点在被移除区间之后：索引左移 removedCount；
                // 3) 其他情况：保持不变。
                int newIndex = current;
                if (current >= removedIndex && current < removedIndex + removedCount)
                {
                    newIndex = Math.Min(removedIndex, newCount - 1);
                }
                else if (current > removedIndex)
                {
                    newIndex = current - removedCount;
                }

                // 规范化
                if (newIndex < 0 || newCount == 0)
                    newIndex = -1;

                // 应用当前项/焦点（不触发滚动）
                if (newIndex != GetCurrentElementIndex())
                {
                    var focused = SetCurrentElementIndex(
                        newIndex,
                        NavigationMethod.Tab,
                        forceKeyboardNavigationReferenceReset: true,
                        startBringIntoView: false,
                        expectBringIntoView: false);

                    // 若容器尚未实现，记录待补焦索引，等 ElementPrepared 时再聚焦
                    if (!focused && newIndex >= 0)
                    {
                        _pendingFocusIndex = newIndex;
                    }
                }

                break;
            }

            case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
            {
                // 数据源整体刷新：若还有元素，将焦点设为 0；否则清空
                var newCount = itemsSourceView.Count;
                var newIndex = newCount > 0 ? 0 : -1;

                var focused = SetCurrentElementIndex(
                    newIndex,
                    NavigationMethod.Tab,
                    forceKeyboardNavigationReferenceReset: true,
                    startBringIntoView: false,
                    expectBringIntoView: false);

                if (!focused && newIndex >= 0)
                {
                    _pendingFocusIndex = newIndex;
                }

                break;
            }
        }
    }

    partial void OnSelectionModePropertyChanged(ItemsViewSelectionMode newValue)
    {
        UpdateSelector();

        if (!IsLoaded) return;

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