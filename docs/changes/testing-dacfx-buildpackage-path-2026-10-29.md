# DacFx BuildPackageTest path

**Date:** 2026-10-29

`DacPacBuilderTests.BuildPackageTest` no longer uses a hard-coded `C:epomerge-em` path. It walks up from the test output to `src/Extensions/OoBDev.Data.Vectors.Net481/bin/Debug/net48/OoBDev.Data.Vectors.dll`, builds into a temporary dacpac, asserts the file exists and is not empty, and deletes it. When the CLR project is not built it ends `Assert.Inconclusive`. Still `DevLocal`.

**Verified:** run with the CLR project built: 1 passed.
