# Temp directories from ITempFileFactory

**Date:** 2026-11-04

`ITempFileFactory` only handed out files, so tests that need a folder built GUID paths under `Path.GetTempPath()` by hand, against the owner rule that temporary files and folders come from the factory. It now has `GetTempDirectory()`, returning an `ITempDirectory` (`DirectoryPath`, `IDisposable`). `TempDirectoryHandle` creates a uniquely named folder and deletes it with its contents on dispose; a file still held open or a read-only file is tolerated rather than failing the caller.

Tests: `TempDirectoryHandleTests` (nested delete, double dispose, held-open file, distinct directories from the factory); `OoBDev.System.Tests` temp tests 6 of 6 pass.

Callers moved so far: the project files in `OoBDev.DevTerm.Web.Tests` (`WebHostTests`, `WebLayoutReviewTests`) use `TempFileHandle`, and the folders in `ConnectionManagerStoreTests` and `WebManifestEditorTests` use `TempDirectoryHandle` (6 of 6 pass), and the certificate files in `WebRolesTests` use `TempFileHandle` (2 of 2 pass); the Web.Tests suite ran 105 passed, 0 failed, 1 skipped before the 5-minute run timeout aborted the run (about 4 m 48 s). The remaining callers are tracked in `BACKLOG.md`, section Test infrastructure.
