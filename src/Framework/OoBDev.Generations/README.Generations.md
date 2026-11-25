# OoBDev.Generations

Seeded test and demo data: the same seed and type always give the same object graph. `new Generator(seed, timeProvider, maxDepth)` builds classes with a public parameterless constructor, interfaces (a `DispatchProxy` whose getters return generated values, fixed on first read), arrays, lists and primitives. Attributes steer a property: `GenerateString` (word, name, email, phone, address presets from small built-in lists, never real data), `GenerateNumber` (range), `GenerateDateTime` (days back from the supplied clock), `GenerateCollection` (count range). Unattributed properties get defaults per type; nested objects stop at `maxDepth` (null); an unsupported type throws a message naming the property path.

```csharp
var person = new Generator(seed: 7, TimeProvider.System).Create<Person>();
```

A test-support library: reference it from test projects only (the Example web API is the one exception: it uses it to show seeded generation on `/data/generation`). Not built yet: the `GeneratorBuilder` provider hooks, nullable null probability, `TryAddGenerations()` registration and a separate Abstractions project. Design: [TestDataGeneration](../../../docs/design/TestDataGeneration/README.md).
