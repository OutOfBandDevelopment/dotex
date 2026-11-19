# OoBDev.Amazon.S3

AWS S3 blob container provider for the Documents framework: `IBlobContainerProvider` over one bucket, chosen like the Azure provider through `IBlobContainerProviderFactory` (design: [blob containers request](../../../../docs/features/request/2026-11-10-blob-container-providers.md), [PersistenceService](../../../../docs/design/DocumentServices/PersistenceService)).

> **Tested against emulators only.** The Integration tests run against the Moto container in the test stack; behavior against real S3 (permissions, versioning, large uploads) is not verified.

## What it provides

| Type | Role |
|------|------|
| `S3BlobContainerProvider` | One container = one bucket, a path = an object key. Get, store (refuses an existing key unless `overwrite`), delete, metadata read and replace, and `QueryContent` (a paged listing with one metadata request per object, filtered in memory). User metadata round-trips as `x-amz-meta-*`. |
| `S3BlobContainerProviderFactory` | Offers the provider while `Region` or `ServiceUrl` is set, else returns null so another provider's factory is used. |
| `S3ClientFactory` / `S3BlobOptions` | Client from options; `ServiceUrl` points it at Moto (path-style addressing). |

## Configuration

```json
{
  "S3BlobOptions": {
    "Region": "us-east-1",
    "AccessKeyId": "",
    "SecretAccessKey": "",
    "ServiceUrl": "http://127.0.0.1:4566",
    "EnsureContainerExists": true
  }
}
```

Register with `services.TryAddAwsS3BlobServices(configuration)`. Empty keys use the AWS credential chain.
