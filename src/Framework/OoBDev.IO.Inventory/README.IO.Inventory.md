# OoBDev.IO.Inventory

Read-only file inventory and exact duplicate detection. `FileScanner.ScanAsync(root, followLinks, ct)` records path, size, modified time and an XxHash3 content hash into an `IInventoryStore`; a second scan re-reads only new or changed files and reports unreadable files instead of failing. `DuplicateFinder.FindAsync(store, ct)` groups by size and fast hash, then confirms each group with SHA-512, so a fast-hash collision never reports a false duplicate. Nothing is deleted or moved.

```csharp
var store = new InMemoryInventoryStore();
await new FileScanner(store).ScanAsync(root, followLinks: false, ct);
await foreach (var group in new DuplicateFinder().FindAsync(store, ct)) { /* group.Paths */ }
```

Consumers bring their own `IInventoryStore` (for example a Postgres catalog). Symbolic links are skipped unless `followLinks` is set. Not built yet: the perceptual image phase and the `src/Tools` command line. Design: [FileInventory](../../../docs/design/FileInventory/README.md).
