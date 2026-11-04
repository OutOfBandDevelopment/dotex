# Temp directories from ITempFileFactory

**Date:** 2026-11-04

`ITempFileFactory` only handed out files, so tests that need a folder built GUID paths under `Path.GetTempPath()` by hand, against the owner rule that temporary files and folders come from the factory. It now has `GetTempDirectory()`, returning an `ITempDirectory` (`DirectoryPath`, `IDisposable`). `TempDirectoryHandle` creates a uniquely named folder and deletes it with its contents on dispose; a file still held open or a read-only file is tolerated rather than failing the caller.

Tests: `TempDirectoryHandleTests` (nested delete, double dispose, held-open file, distinct directories from the factory); `OoBDev.System.Tests` temp tests 6 of 6 pass.

Not done: moving the existing callers over (tracked in `BACKLOG.md`, section Test infrastructure).
