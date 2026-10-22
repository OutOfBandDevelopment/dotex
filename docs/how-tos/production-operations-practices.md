# Production operations practices

**Last Updated:** 2026-10-22 · **Audience:** teams that run services built on this code base. These practices were taken from a sibling project's operating rules and generalized; they are recommendations, not framework behavior.

## Contents

1. [Treat source data as read-only](#1-treat-source-data-as-read-only)
2. [Database migrations](#2-database-migrations)
3. [Restarting and killing processes](#3-restarting-and-killing-processes)
4. [Deployment gates](#4-deployment-gates)
5. [Testing against production-shaped data](#5-testing-against-production-shaped-data)

## 1. Treat source data as read-only

A service that ingests files, documents or records from a share, bucket or upstream database must not modify them.

- Enforce it with the platform, not with a promise: a read-only mount, a read-only database role, or a read-only token (for example a storage SAS token with list and read only).
- Findings such as duplicates are recorded in the service's own store; cleanup is proposed to a person and never performed automatically.
- Data the service derives (indexes, embeddings, conversions, thumbnails) may be rebuilt or purged, with a backup first. Never extend a purge to a source location.

Applies to the loaders, document and search services (`OoBDev.DataLoader`, `OoBDev.Documents`, `OoBDev.Search`, `OoBDev.FileRagEngine.Cli`).

## 2. Database migrations

**Table 1 — Migration approval by risk**

| Migration | Before applying to the live database |
|---|---|
| Additive (new table, column, index, view) | Run it on the test instance, take a backup just before |
| Destructive or irreversible (drop, rename, retype, delete rows) | Rehearse on a restored copy of production, then backup, then explicit approval |

- Approve migrations by name, not "the migrations".
- Never run a `Down` migration on the live database; restore the backup instead.
- One backup immediately before the run is enough; extra full dumps add little.
- A dump of the live database restored into a throwaway database is a safe way to test (see section 5); it must never write back to the live database.

Applies to `OoBDev.DacFx`, the Vectors DB project and any EF-based adapter.

## 3. Restarting and killing processes

Before restarting a service or killing a process in production, state what it is (identifier, command, start time), why, the risk and the exact command. An approval covers only what was named. Restarting what you just deployed on the test instance needs no approval.

## 4. Deployment gates

- A branch may be deployed without being merged to `main`; merging is a separate decision.
- Build and prove every change on an isolated test instance (own database, own port) before it reaches production.
- A deploy script recreates only the services that changed and rolls back on failure.
- Switching on a feature in production is approved by name, with a UI or API surface, tests, a design document and a runbook for anything that has to be operated.

## 5. Testing against production-shaped data

Empty databases hide data-loss and performance problems. Restore a sanitized dump into a throwaway database in the Docker test stack, apply the migration and run the Integration suite against it. The test must refuse to run when its connection string does not point at a test instance (see `.claude/rules/testing.md`).

---

[↑ How-tos](./README.md)
