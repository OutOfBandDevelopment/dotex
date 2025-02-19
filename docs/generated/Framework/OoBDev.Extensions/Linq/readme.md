**README File**

This repository contains a set of C# extension classes that provide additional functionality for working with asynchronous enumerable sequences, LINQ extensions, and generic dictionaries.

**Summary**

The `AsyncEnumerableExtensions` class provides a set of methods for converting between asynchronous enumerable sequences, tasks, and enumerables. This includes methods for converting an asynchronous enumerable sequence to a list, task, or enumerable, as well as methods for projecting each element of a sequence into a new form. The `DictionaryExtensions` class provides additional methods for working with generic dictionaries, including a method for trying to get a value from a dictionary using a custom key comparer.

**Technical Summary**

The `AsyncEnumerableExtensions` class uses several design patterns and architectural patterns to achieve its functionality. The async enumerable sequences are implemented using the `IAsyncEnumerable<T>` interface, which is a part of the .NET Standard 2.1 library. The `ToListAsync` method uses the `IAsyncEnumerable<T>` interface to asynchronously enumerate the sequence and convert it to a list. The `Select` method uses the `Func<T, Task<TResult>>` delegate to asynchronously project each element of the sequence.

The `DictionaryExtensions` class uses the builder design pattern to create a new dictionary with a custom key comparer. The `ChangeComparer` method uses the `IDictionary<TKey, TValue>` interface to rebuild the dictionary using the new comparer.

**Component Diagram**

```plantuml
@startuml
class AsyncEnumerableExtensions {
  IAsyncEnumerable<T> ->|Asynchronous Sequence| AsAsyncEnumerable<T>
  IAsyncEnumerable<T> ->|Convert to List| ToListAsync<T>
  IAsyncEnumerable<T> ->|Project Each Element| Select<T, TResult>
}

class DictionaryExtensions {
  IDictionary<TKey, TValue> ->|Try Get Value| TryGetValue<TKey, TValue>
  IDictionary<TKey, TValue> ->|Change KeyComparer| ChangeComparer<TKey, TValue>
}

component AsyncEnumerableExtensions {
  + AsAsyncEnumerable(T)
  + ToListAsync(T)
  + Select(T, TResult)
}

component DictionaryExtensions {
  + TryGetValue(TKey, TValue)
  + ChangeComparer(TKey, TValue)
}

AsyncEnumerableExtensions --* AsyncEnumerableExtensions
AsyncEnumerableExtensions --* DictionaryExtensions
DictionaryExtensions --* DictionaryExtensions
@enduml
```

In this diagram, the `AsyncEnumerableExtensions` and `DictionaryExtensions` classes are represented as components, which are related through the interfaces they implement. The `IAsyncEnumerable<T>` interface is implemented by the `AsyncEnumerableExtensions` class, which provides the `AsAsyncEnumerable<T>`, `ToListAsync<T>`, and `Select<T, TResult>` methods. The `IDictionary<TKey, TValue>` interface is implemented by the `DictionaryExtensions` class, which provides the `TryGetValue<TKey, TValue>` and `ChangeComparer<TKey, TValue>` methods.