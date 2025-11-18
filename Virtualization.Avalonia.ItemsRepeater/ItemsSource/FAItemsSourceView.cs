using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Collections;

namespace Virtualization.Avalonia;

// Source is combo of ItemsSourceView & InspectingDataSource

/// <summary>
/// Represents a standardized view of the supported interactions between a given ItemsSource object and an ItemsRepeater control.
/// </summary>
public sealed class FAItemsSourceView : IReadOnlyList<object?>, IEnumerable
{
    public FAItemsSourceView(IEnumerable source)
    {
        ArgumentNullException.ThrowIfNull(source);
        _vector = source;
        ListenToCollectionChanges();

        _uniqueIdMapping = source as IKeyIndexMapping;
    }

    /// <summary>
    /// Gets the number of items in the collection.
    /// </summary>
    public int Count
    {
        get
        {
            if (_cachedSize == -1)
            {
                // Call the override the very first time. After this,
                // we can just update the size when there is a data source change.
                _cachedSize = GetSizeCore();
            }

            return _cachedSize;
        }
    }

    /// <summary>
    /// Gets a value that indicates whether the items source can provide a unique key for each item.
    /// </summary>
    public bool HasKeyIndexMapping =>
        HasKeyIndexMappingCore();

    /// <summary>
    /// Occurs when the collection has changed to indicate the reason for the change and which items changed.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>
    /// Retrieves the item at the specified index.
    /// </summary>

    public object? this[int index] => GetAtCore(index);

    /// <summary>
    /// Retrieves the index of the item that has the specified unique identifier (key).
    /// </summary>
    public string KeyFromIndex(int index) =>
        KeyFromIndexCore(index);

    /// <summary>
    /// Retrieves the index of the item that has the specified unique identifier (key).
    /// </summary>
    public int IndexFromKey(string id) =>
        IndexFromKeyCore(id);

    /// <summary>
    /// Retrieves the index of the specified item.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public int IndexOf(object value) =>
        IndexOfCore(value);

    /// <summary>
    /// Called when the ItemsSource has raised a CollectionChanged event
    /// </summary>
    /// <param name="args"></param>
    private void OnItemsSourceChanged(NotifyCollectionChangedEventArgs args)
    {
        _cachedSize = GetSizeCore();
        CollectionChanged?.Invoke(this, args);
    }

    /// <summary>
    /// Gets the count of the underlying collection
    /// </summary>
    private int GetSizeCore()
    {
        if (_vector is IList list)
            return list.Count;

        return _vector.Count();
    }

    /// <summary>
    /// Gets the item at the specified index from the underlying collection
    /// </summary>
    private object? GetAtCore(int index)
    {
        if (_vector is IList list)
            return list[index];

        return _vector.ElementAt(index);
    }

    /// <summary>
    /// Gets whether this underlying supports Key-Index mapping
    /// </summary>
    /// <returns></returns>
    private bool HasKeyIndexMappingCore() => 
        _uniqueIdMapping != null;

    /// <summary>
    /// Gets the key from the specified index
    /// </summary>
    private string KeyFromIndexCore(int index)
    {
        if (_uniqueIdMapping != null)
            return _uniqueIdMapping.KeyFromIndex(index);

        throw new NotImplementedException();
    }

    /// <summary>
    /// Gets the Index from the specified key
    /// </summary>
    private int IndexFromKeyCore(string id)
    {
        if (_uniqueIdMapping != null)
            return _uniqueIdMapping.IndexFromKey(id);

        throw new NotImplementedException();
    }

    /// <summary>
    /// Queries the underlying collection for the item at the specified index
    /// </summary>
    private int IndexOfCore(object value)
    {
        var index = -1;
        if (_vector is IList list)
        {
            index = list.IndexOf(value);
        }
        else
        {
            index = _vector.IndexOf(value);
        }

        return index;
    }

    private void UnListenToCollectionChanges()
    {
        _eventToken?.Dispose();
        _eventToken = null;
    }

    private void ListenToCollectionChanges()
    {
        if (_vector is not INotifyCollectionChanged incc)
            return;
        _eventToken = incc.GetWeakCollectionChangedObservable()
            .Subscribe(new SimpleObserver<NotifyCollectionChangedEventArgs>(OnCollectionChanged));
    }

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
    {
        OnItemsSourceChanged(args);
    }

    private int _cachedSize = -1;
    private readonly IEnumerable _vector;
    private readonly IKeyIndexMapping? _uniqueIdMapping;
    private IDisposable? _eventToken;

    public IEnumerator<object?> GetEnumerator() => _vector.Cast<object?>().GetEnumerator();
    
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
