# OoBDev.Extensions

Small, dependency-light helpers used across the framework: stream chunking, JSON and XML stream deserialization, async enumerable helpers, reflection and resource helpers, command line option binding, in-memory configuration and an ambient accessor.

**Table 1 — Contents**

| Namespace / type | What it gives you |
|------------------|-------------------|
| `OoBDev.Extensions.IO` `StreamExtensions` | `CopyOf(stream)` (rewound memory copy); `SplitStreamAsync(contextLength = 768, overlap = 0)` yields `ContentChunk` (text, sequence, start, length) and needs a seekable stream |
| `FileTools` | `SplitFileAsync(filename, chunkLength, overlap)`: the same chunking for a file |
| `StreamJsonDeserializeExtensions` | `AsJson<T>()`, `AsJsonAsync<T>()` and `Type` overloads, with optional `JsonSerializerOptions`; null stream gives null |
| `StreamXmlDeserializeExtensions` | `AsXml<T>(extraTypes)`, `AsXmlAsync<T>()` and `Type` overloads |
| `OoBDev.Extensions.Linq` `AsyncEnumerableExtensions` | `ToListAsync`, `ToReadOnlyCollectionAsync`, `ToSetAsync`, `AsEnumerableAsync`, `AsAsyncEnumerable` and an async `Select` |
| `DictionaryExtensions` | `TryGetValue(key, out value, comparer)` and `ChangeComparer(comparer)` (both copy the dictionary) |
| `OoBDev.Extensions.Reflection` `ReflectionExtensions` | `GetShortTypeName`, `GetAttributes`, `GetPropertiesByAttribute<T>`, `GetStaticMethod`, `GetInstanceMethod`, `GetParametersTypes`, `TryParse`, `MakeSafe` and others |
| `ResourceExtensions` | `GetResourceStream` (object, type or assembly) and `GetResourceAsString(Async)` for embedded resources |
| `OoBDev.Extensions.Configuration` `ConfigurationBuilderExtensions` | `AddInMemoryCollection` with `(key, value)` tuples (the first value wins on duplicate keys) |
| `CommandLine` | Binds `--name value` options to a class marked with `CommandParameterAttribute`; see below |
| `OoBDev.Extensions.Accessors` | `IAccessor<T>`: an `AsyncLocal` value holder registered by `AddAccessor<T>()` (singleton, `TryAdd`) |
| `OoBDev.Extensions` `ServiceCollectionExtensions` | `AddAccessor<T>()` and `GetSingletonInstance<TService, TInstance>(out instance)` (reuses a registered instance or creates one and registers it) |
| `StringTools` | `SplitBy(length = 80, breaker = ' ')` and `WriteAsLines()` |

## Command line options

```csharp
config.AddCommandLine(args, CommandLine.BuildParameters<MyOptions>());   // --input and --Input both bind
if (CommandLine.IsHelpRequest(args))                                    // -h, --help, -?
    Console.WriteLine(CommandLine.Usage("my-tool", "What it does.", typeof(MyOptions)));
```

`BuildParameters<T>` maps the `Short` and `Value` names of each property's `CommandParameterAttribute` and the property name to `--name` switches (ignoring case). `Usage` lists every switch with its type, `(required)` marks and the `Description`; a boolean needs an explicit `true` or `false`. The tools in `src/Tools` use it.

## Chunking example

```csharp
await using var file = File.OpenRead("notes.txt");
await foreach (var chunk in file.SplitStreamAsync(contextLength: 1024, overlap: 128))
{
    // chunk.Data, chunk.Sequence, chunk.Start, chunk.Length
}
```
