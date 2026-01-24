using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace OoBDev.System.Collections.ObjectModel;

/// <summary>
/// Represents a dictionary that provides notifications when items are added, removed, or when the collection is refreshed.
/// Implements INotifyCollectionChanged and INotifyPropertyChanged for data binding scenarios.
/// </summary>
/// <typeparam name="TKey">The type of keys in the dictionary, which must be non-nullable.</typeparam>
/// <typeparam name="TValue">The type of values in the dictionary.</typeparam>
public class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, INotifyCollectionChanged, INotifyPropertyChanged
     where TKey : notnull
{
    /// <summary>
    /// Gets the underlying dictionary that stores the key-value pairs.
    /// </summary>
    protected IDictionary<TKey, TValue> Dictionary { get; } = new Dictionary<TKey, TValue>();

    #region Constructors
    /// <summary>
    /// Initializes a new instance of the ObservableDictionary class that is empty.
    /// </summary>
    public ObservableDictionary()
    {
    }

    /// <summary>
    /// Initializes a new instance of the ObservableDictionary class that contains elements copied from the specified dictionary.
    /// </summary>
    /// <param name="dictionary">The dictionary whose elements are copied to the new ObservableDictionary.</param>
    public ObservableDictionary(IDictionary<TKey, TValue> dictionary) => Dictionary = new Dictionary<TKey, TValue>(dictionary);

    /// <summary>
    /// Initializes a new instance of the ObservableDictionary class that uses the specified IEqualityComparer&lt;T&gt;.
    /// </summary>
    /// <param name="comparer">The IEqualityComparer&lt;T&gt; implementation to use when comparing keys.</param>
    public ObservableDictionary(IEqualityComparer<TKey> comparer) => Dictionary = new Dictionary<TKey, TValue>(comparer);

    /// <summary>
    /// Initializes a new instance of the ObservableDictionary class that is empty and has the specified initial capacity.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the ObservableDictionary can contain.</param>
    public ObservableDictionary(int capacity) => Dictionary = new Dictionary<TKey, TValue>(capacity);

    /// <summary>
    /// Initializes a new instance of the ObservableDictionary class that contains elements copied from the specified dictionary and uses the specified IEqualityComparer&lt;T&gt;.
    /// </summary>
    /// <param name="dictionary">The dictionary whose elements are copied to the new ObservableDictionary.</param>
    /// <param name="comparer">The IEqualityComparer&lt;T&gt; implementation to use when comparing keys.</param>
    public ObservableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) => Dictionary = new Dictionary<TKey, TValue>(dictionary, comparer);

    /// <summary>
    /// Initializes a new instance of the ObservableDictionary class that is empty, has the specified initial capacity, and uses the specified IEqualityComparer&lt;T&gt;.
    /// </summary>
    /// <param name="capacity">The initial number of elements that the ObservableDictionary can contain.</param>
    /// <param name="comparer">The IEqualityComparer&lt;T&gt; implementation to use when comparing keys.</param>
    public ObservableDictionary(int capacity, IEqualityComparer<TKey> comparer) => Dictionary = new Dictionary<TKey, TValue>(capacity, comparer);

    #endregion

    #region IDictionary<TKey,TValue> Members

    /// <summary>
    /// Adds an element with the provided key and value to the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to add.</param>
    /// <param name="value">The value of the element to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    /// <exception cref="ArgumentException">Thrown when an element with the same key already exists.</exception>
    public void Add(TKey key, TValue value) => Insert(key, value, true);

    /// <summary>
    /// Determines whether the dictionary contains an element with the specified key.
    /// </summary>
    /// <param name="key">The key to locate in the dictionary.</param>
    /// <returns>True if the dictionary contains an element with the key; otherwise, false.</returns>
    public bool ContainsKey(TKey key) => Dictionary.ContainsKey(key);

    /// <summary>
    /// Gets a collection containing the keys in the dictionary.
    /// </summary>
    public ICollection<TKey> Keys => Dictionary.Keys;

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key whose value to get.</param>
    /// <param name="value">When this method returns, the value associated with the specified key, if found; otherwise, the default value for the type of the value parameter.</param>
    /// <returns>True if the dictionary contains an element with the specified key; otherwise, false.</returns>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => Dictionary.TryGetValue(key, out value);

    /// <summary>
    /// Gets a collection containing the values in the dictionary.
    /// </summary>
    public ICollection<TValue> Values => Dictionary.Values;

    /// <summary>
    /// Determines whether the dictionary contains a specific key-value pair.
    /// </summary>
    /// <param name="item">The key-value pair to locate in the dictionary.</param>
    /// <returns>True if the key-value pair is found in the dictionary; otherwise, false.</returns>
    public bool Contains(KeyValuePair<TKey, TValue> item) => Dictionary.Contains(item);

    /// <summary>
    /// Copies the elements of the dictionary to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the dictionary.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Dictionary.CopyTo(array, arrayIndex);

    /// <summary>
    /// Gets the number of elements contained in the dictionary.
    /// </summary>
    public int Count => Dictionary.Count;

    /// <summary>
    /// Gets a value indicating whether the dictionary is read-only.
    /// </summary>
    public bool IsReadOnly => Dictionary.IsReadOnly;

    /// <summary>
    /// Removes the first occurrence of a specific key-value pair from the dictionary.
    /// </summary>
    /// <param name="item">The key-value pair to remove from the dictionary.</param>
    /// <returns>True if the item was successfully removed from the dictionary; otherwise, false.</returns>
    public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);

    /// <summary>
    /// Removes the element with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <returns>True if the element is successfully removed; otherwise, false. This method also returns false if key was not found in the dictionary.</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    public bool Remove(TKey key)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        var removed = Dictionary.Remove(key);
        if (removed)
            OnCollectionChanged();

        return removed;
    }

    /// <summary>
    /// Gets or sets the element with the specified key.
    /// </summary>
    /// <param name="key">The key of the element to get or set.</param>
    /// <returns>The element with the specified key.</returns>
    /// <exception cref="ArgumentNullException">Thrown when key is null.</exception>
    /// <exception cref="KeyNotFoundException">Thrown when the property is retrieved and key is not found.</exception>
    public TValue this[TKey key]
    {
        get => Dictionary[key];
        set => Insert(key, value, false);
    }

    #endregion

    #region ICollection<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Adds a key-value pair to the dictionary.
    /// </summary>
    /// <param name="item">The key-value pair to add to the dictionary.</param>
    /// <exception cref="ArgumentNullException">Thrown when the key in item is null.</exception>
    /// <exception cref="ArgumentException">Thrown when an element with the same key already exists.</exception>
    public void Add(KeyValuePair<TKey, TValue> item) => Insert(item.Key, item.Value, true);

    /// <summary>
    /// Removes all items from the dictionary.
    /// </summary>
    public void Clear()
    {
        if (Dictionary.Count > 0)
        {
            Dictionary.Clear();
            OnCollectionChanged();
        }
    }

    #endregion

    #region IEnumerable<KeyValuePair<TKey,TValue>> Members

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator for the dictionary.</returns>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Dictionary.GetEnumerator();

    #endregion

    #region IEnumerable Members

    /// <summary>
    /// Returns an enumerator that iterates through the dictionary.
    /// </summary>
    /// <returns>An enumerator for the dictionary.</returns>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Dictionary).GetEnumerator();

    #endregion

    #region INotifyCollectionChanged Members

    /// <summary>
    /// Occurs when the collection changes, either by adding or removing items, or when the collection is refreshed.
    /// </summary>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    #endregion

    #region INotifyPropertyChanged Members

    /// <summary>
    /// Occurs when a property value changes, including Count, Keys, Values, and the indexer.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    /// <summary>
    /// Adds multiple key-value pairs to the dictionary in a single operation.
    /// Raises a single CollectionChanged event for all added items.
    /// </summary>
    /// <param name="items">The dictionary containing items to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when items is null.</exception>
    /// <exception cref="ArgumentException">Thrown when any key in items already exists in the dictionary.</exception>
    public void AddRange(IDictionary<TKey, TValue> items)
    {
        ArgumentNullException.ThrowIfNull(items, nameof(items));

        if (items.Count > 0)
        {
            if (items.Keys.Any(Dictionary.ContainsKey))
                throw new ArgumentException("An item with the same key has already been added.");
            else
                foreach (var item in items) Dictionary.Add(item);

            OnCollectionChanged(NotifyCollectionChangedAction.Add, items.ToArray());
        }
    }

    private void Insert(TKey key, TValue value, bool add)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        if (Dictionary.TryGetValue(key, out TValue? item))
        {
            if (add) throw new ArgumentException("An item with the same key has already been added.");
            if (Equals(item, value)) return;
            Dictionary[key] = value;

            OnCollectionChanged(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, item));
        }
        else
        {
            Dictionary[key] = value;

            OnCollectionChanged(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value));
        }
    }

    private const string IndexerName = "Item[]";

    private void OnPropertyChanged()
    {
        OnPropertyChanged(nameof(Count));
        OnPropertyChanged(IndexerName);
        OnPropertyChanged(nameof(Keys));
        OnPropertyChanged(nameof(Values));
    }

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private void OnCollectionChanged()
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> changedItem)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, changedItem));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, KeyValuePair<TKey, TValue> newItem, KeyValuePair<TKey, TValue> oldItem)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItem, oldItem));
    }

    private void OnCollectionChanged(NotifyCollectionChangedAction action, IList newItems)
    {
        OnPropertyChanged();
        CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, newItems));
    }
}
