# CI restore, vector functions and Moto AWS emulator

**Date:** 2026-10-08 · **Epic:** Testing and build health · **Status:** Complete (CI run on GitHub not yet observed)

## Summary

Fixed the GitHub Actions restore failure, corrected and null-hardened the SQL CLR vector functions (with SQL Server Integration tests), and replaced LocalStack with the Moto server so the AWS emulator needs no licence token. Repairing the SQS Integration tests on the way exposed two product bugs.

## Contents

- [CI pipeline](#ci-pipeline)
- [Vector functions](#vector-functions)
- [Moto replaces LocalStack](#moto-replaces-localstack)
- [SQS fixes](#sqs-fixes)
- [Verification](#verification)
- [Related](#related)

## CI pipeline

**Table 1 — CI changes**

| Change | Reason |
|--------|--------|
| `ManagePackageVersionsCentrally` off for the `AllMiniLML6v2Sharp` fork (its `OoBDev.*` projects excepted) | Fork has inline versions; restore failed with NU1008 |
| NU1605 suppressed for non-central projects | Downgrade warning treated as error |
| Workflow `permissions` (contents, checks, pull-requests write) | Tag push and test report publishing |
| GitVersion path filter `GitVersion.yml`; `RunSettings` points at `src/.runsettings`; test step passes `--settings` | Wrong paths |
| Build tag step skips an existing remote tag | Re-runs of one commit failed on the duplicate tag |

## Vector functions

**Table 2 — Vector behaviour**

| Function | Before | After |
|----------|--------|-------|
| `Angle`, `AngleF` | `acos` of the wrong quantity (`sqrt(dot)`), never above 90 degrees | `acos(clamp(dot / (\|a\|\|b\|)))`, range 0 to pi |
| `Angle`, `AngleF` undefined (zero vector, NaN, length mismatch) | Misleading number or exception | NULL |
| `Distance`, `Midpoint`, `UniformV` on length mismatch; `Distance` with unknown metric | Exception (fails the whole SQL batch) | NULL |

NULL policy: undefined results are NULL, never an exception and never a plausible number. SQL Server sorts NULL first in ascending order, so callers filter with `WHERE ... IS NOT NULL`. This is documented in the XML docs and `README.Data.Vectors.md`. Still throwing on purpose: `Parse`, matrix `Element` out of range, vector constructor with an invalid size.

The metric names are `cosine`, `similarity`, `euclidean`, `dot`, `manhattan` (case-insensitive); the earlier XML docs listed names the code never accepted.

`SqlServerDeploymentTests` (category `Integration`) deploys the `OoBDev.Data.Vectors.DB` dacpac with `IncludeCompositeObjects`, enables CLR for its run and checks distances, angle and NULL results inside SQL Server. It needs a disposable server because it changes the CLR settings.

## Moto replaces LocalStack

LocalStack's free image now requires a licence token (the compose file had been pinned to `4.4` to avoid it). Moto (`motoserver/moto`, Apache-2.0) needs none.

- Compose service `moto` on host port 4566 (container port 5000), so `SQS_ENDPOINT` is unchanged.
- One-shot `moto-init` service creates `integration-test-queue` after Moto is healthy (Moto has no init hook directory).
- `LOCALSTACK_*` settings renamed `MOTO_*`; scripts, nginx route (`/moto/`), dashboard and documentation updated.
- Only SQS is used by the repository; the S3, SNS, DynamoDB, Lambda and Secrets Manager services of the old service list were dropped.

## SQS fixes

**Table 3 — SQS fixes**

| Item | Problem | Fix |
|------|---------|-----|
| `SqsClientFactory` | The `ServiceUrl` setting was ignored, so an emulator could not be targeted | Honours `ServiceUrl` |
| `AmazonSqsMessageProvider` | The AWS SDK 4 leaves `MessageAttributes` null; adding headers would throw | Initialises the dictionary first |
| `AmazonSqsIntegrationTests` | Did not compile; resolved the last registered `IMessageSenderProvider` (the in-process one, which returns an id without sending) | Uses current registration APIs and resolves the keyed SQS provider |

## Verification

- Vector tests: 55 passed, including 2 SQL Server Integration tests against a SQL Server 2022 container.
- SQS tests: 4 passed against the Moto container.
- Unit and Simulate set: all assemblies passed before the Moto change; the Moto change touches only container, script and documentation files plus the SQS code above.
- Not verified: a GitHub Actions run of the CI fixes.

## Related

- [TODO.md](../../TODO.md)
- [Docker testing infrastructure](testing-docker-infrastructure-2026-01-19.md)
- [containers/testing/README.md](../../containers/testing/README.md)

[↑ Change index](README.md)
