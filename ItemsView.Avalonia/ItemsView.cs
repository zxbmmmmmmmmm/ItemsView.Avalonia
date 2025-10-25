using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using ItemsView.Avalonia.Selection;
using System.Diagnostics.CodeAnalysis;

namespace ItemsView.Avalonia;

public partial class ItemsView : TemplatedControl
{
    private ScrollViewer _scrollViewer = null!;

    private ItemsRepeater _itemsRepeater = null!;

    private ISelectionModel _selectionModel = new InternalSelectionModel();

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
}