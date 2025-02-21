**README.md**

This repository contains a set of unit tests for two extension libraries: `OoBDev.Extensions.Linq` and `OoBDev.Extensions.Linq.Tests`. The libraries provide additional functionality for working with asynchronous enumerables and dictionaries.

**Summary**

The `OoBDev.Extensions.Linq` library provides an `AsyncEnumerableExtensions` class that offers methods for converting asynchronous enumerables to lists, and the `DictionaryExtensions` class that provides methods for working with dictionaries, including a `TryGetValue` method.

**Technical Summary**

The design pattern used in this code is the Singleton pattern, where the `TestContext` class is initialized only once and shared across all tests. The `AsyncEnumerableExtensions` class uses the `IAsyncEnumerable<T>` interface, which is part of the .NET 6 Async Enumerables feature. The `DictionaryExtensions` class uses the `Dictionary<TKey, TValue>` class from the .NET Framework.

**Component Diagram**

```plantuml
@startuml
class AsyncEnumerableExtensionsTests {
  - TestContext
  - CreateTestData()
  - ToListAsyncTest()
}

class DictionaryExtensionsTests {
  - TestContext
  - TryGetValueTest()
}

class OoBDev.Extensions.Linq {
  + AsyncEnumerableExtensions
  + DictionaryExtensions
}

class .NET Framework {
  - Dictionary<TKey, TValue>
}

class .NET 6 {
  - IAsyncEnumerable<T>
}

AsyncEnumerableExtensionsTests --* OoBDev.Extensions.Linq
DictionaryExtensionsTests --* OoBDev.Extensions.Linq
.NET Framework --* Dictionary<TKey, TValue>
.NET 6 --* IAsyncEnumerable<T>

@enduml
```