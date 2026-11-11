# Feature request: blob containers defined per point of use

- **Status:** triaged
- **Requested by:** owner (from `docs/Framework/BlobStorage.md`, removed 2026-11-10)
- **Date:** 2026-11-10
- **Area:** storage, `OoBDev.Azure.StorageAccount`, document services

## Need

A common way to access blob containers where each point of use defines its own container, and the provider (Azure Blob, S3, file system) is chosen by configuration.

## Proposed behavior

- A generic interface lets several container providers be registered side by side.
- Configuration selects the provider for each container (`TryAddConfiguredKeyedService<T>`).
- An attribute tags a container by a generic key.

## Acceptance criteria

- Given two containers keyed `invoices` and `exports` configured to different providers, then each resolves to its own provider.
- Given a missing key, then startup fails with the key name.

## Existing code checked

Azurite is in the integration stack; S3 tests wait for an S3 adapter (`BACKLOG.md` section 3). `docs/design/DocumentServices/PersistenceService` may already define the abstraction; compare before building.

## Security and operations

Credentials come from options, never code; containers are private by default.

## Priority

- **Impact:** blocks an S3 adapter and per-feature storage.
- **Needed by:** no date
- **Workaround today:** use the storage client directly

## Open questions

Is this the PersistenceService design under another name?

## Triage notes

None yet.

2026-11-11: triaged. Accepted as a backlog item: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, SharedFramework requests.
