# Procedural test data generation

Status: **proposed design, written 2026-11-19 on the assumed answer A of Q19 (no `Bogus` dependency; unscheduled until a consumer appears); core built 2026-11-19 (`OoBDev.Generations`: `Generator`, four attributes, interface proxy, 6 unit tests); builder hooks, null probability, DI registration and the Abstractions split are not built.** Request: [procedural test data generation](../../features/request/2026-11-10-procedural-test-data-generation.md). Epic 9 of [SharedFramework](../SharedFramework/README.md).

## Contents

1. [Behavior](#behavior)
2. [Architecture](#architecture)
3. [API sketch](#api-sketch)
4. [Rules](#rules)
5. [Testing strategy](#testing-strategy)
6. [Open points](#open-points)

### List of Tables

1. Table 1 — Attributes
2. Table 2 — Test plan

## Behavior

Create object graphs for tests and demos from attributes on a type or interface, with a seed so a run repeats exactly. The same seed and the same type always give the same graph. Interfaces and abstract types are built through a dispatch proxy (`DispatchProxy`) whose properties return generated values.

## Architecture

`OoBDev.Generations.Abstractions` (Common) holds the attributes and `IGenerator`; `OoBDev.Generations` (Framework, host-free) holds the seeded context, the provider builder and the proxy. Generation uses `System.Random` seeded explicitly; it never reads the clock (`TimeProvider` is injected where a date is generated). Registration is `TryAddGenerations()`. It is a test-support library, so it is referenced from test projects, not from production code.

**Table 1 — Attributes**

| Attribute | Generates |
|-----------|-----------|
| `GenerateString` (length range, pattern) | strings, plus name, email, phone, address and word presets |
| `GenerateInteger`, `GenerateDecimal` (range) | numbers |
| `GenerateDateTime` (range) | dates from the supplied `TimeProvider` window |
| `GenerateEnumeration` | a random member |
| `GenerateCollection`, `GenerateArray` (count range) | lists and arrays of generated items |

*Table 1 lists the attributes; a nullable property is null with a configurable probability.*

## API sketch

```csharp
public interface IGenerator { T Create<T>(); object Create(Type type); }
public sealed class GenerationContext { public GenerationContext(int seed, TimeProvider time); }
public sealed class GeneratorBuilder
{
    public GeneratorBuilder AddProvider<T>(Func<GenerationContext, T> factory);
    public IGenerator Build(int seed);
}
```

## Rules

- A type with no attributes gets defaults per property type; an unsupported type fails with a message naming the property path.
- Cycles in a graph stop at a configured depth (default 3) and yield null or an empty collection.
- Output never contains real personal data; word, name and address presets come from small built-in lists.

## Testing strategy

**Table 2 — Test plan**

| Case | Tier | Check |
|------|------|-------|
| same seed, same type | Unit | identical graphs |
| different seeds | Unit | graphs differ |
| ranges and patterns | Unit | every value inside its range or pattern, over many seeds |
| interface through the proxy | Unit | properties filled |
| cycle | Unit | stops at the depth |
| unsupported type | Unit | message names the path |

*Table 2 maps each case to its tier; the Framework target is 80% or more coverage.*

## Open points

Whether `Bogus` should supply the presets instead of the built-in lists (Q19 says no third-party dependency for now); whether the CSV utility project is the source for large name lists.
