# OoBDev.FileInventory.Cli

`file-inventory scan --root <folder> [--follow-links true]` hashes every file under a folder with the fast hash, confirms equal-size equal-hash files with SHA-512 and prints the duplicate groups (library: [OoBDev.IO.Inventory](../../Framework/OoBDev.IO.Inventory/README.IO.Inventory.md)).

- Exit code 0 no duplicates, 1 duplicates found, 2 error (bad arguments, missing folder).
- The inventory is in memory, so each run is one scan and report; a persistent store is a consumer adapter.
- `file-inventory images --root <folder> [--max-distance 5]` groups near-duplicate images (dHash, see [OoBDev.IO.Inventory.Images](../../Framework/OoBDev.IO.Inventory.Images/README.IO.Inventory.Images.md)); same exit codes.
- `InventoryCommand.RunAsync` takes the writers, so the command is tested without a process.
