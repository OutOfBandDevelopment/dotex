# Hybrid Search — Testing Strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Draft (from the code as it stands) · **Date:** 2026-11-10

## Contents

- [Current state](#current-state)
- [Test layers](#test-layers)
- [Coverage](#coverage)

### List of Tables

1. [Table 1 — Tests to add](#table-1)

## Current state

`OoBDev.Search.Tests` holds 11 tests (2026-11-10: `VectorStoreFactoryTests`, `ServiceCollectionExtensionsTests`) and `OoBDev.Search` reports 100% line coverage. The only search tests elsewhere cover the adapters (`QdrantVectorStoreProviderTests`, `QdrantGrpcClientTests`, `OpenSearchTests`, all in their own projects) and the example application (`SearchScoringTests`, `SearchAnnotationsExampleTests`). The three obsolete shells are excluded from coverage and need no tests.

## Test layers

- **Unit** (isolated): `VectorStoreFactory` and `WrappedVectorStore<T>` with fake providers.
- **Simulate**: the factory resolved through a real `ServiceCollection` with `TryAddSearchServices()` and an in-memory fake provider.
- **Integration** (Docker Qdrant and OpenSearch from `containers/testing`): store, list and neighbour queries; the hybrid merge once built.

<a id="table-1"></a>**Table 1 — Tests to add**

| Test (name pattern `Method_Scenario_Expected`) | Tier | Proves |
|------------------------------------------------|------|--------|
| `Create_KeyedProviderRegistered_UsesKeyedProvider` | Unit | Order step 1 of provider selection |
| `Create_NoKeyedProvider_UsesFirstFactoryResult` | Unit | Step 2; a factory returning null is skipped |
| `Create_NoFactoryResult_UsesDefaultProvider` | Unit | Step 3 |
| `Create_AnyProvider_SetsCollectionName` | Unit | `CollectionName` is assigned |
| `CreateOfT_VectorStoreAttribute_UsesAttributeName` | Unit | R3, first rule |
| `CreateOfT_TableAttribute_UsesTableName` | Unit | R3, second rule |
| `CreateOfT_NoAttribute_UsesTypeName` | Unit | R3, fallback |
| `WrappedVectorStore_Calls_ForwardToWrapped` | Unit | Each of the five members is forwarded |
| `TryAddSearchServices_Twice_RegistersOnce` | Unit | `TryAdd*` behaviour |
| `TryAddSearchServices_ResolvesStoreOfT` | Simulate | Open-generic registration works end to end |
| `Hybrid_*` (join, rescale, larger score, limit, union of types) | Unit | Only when the hybrid service is built |

## Coverage

Target 80% or more for `OoBDev.Search` (the factory, the wrapper and the registration are the only live code). Report the number from `dotnet test --collect:"XPlat Code Coverage"`; measured 2026-11-10: 100% lines for `OoBDev.Search`.
