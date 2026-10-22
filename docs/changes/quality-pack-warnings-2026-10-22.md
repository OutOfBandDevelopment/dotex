# Pack warnings NU5118 and NU5128 removed

**Date:** 2026-10-22

- `Retro/Apple2/Notes.txt` renamed `Apple2Notes.txt`: the examples packing flattens file names, so it clashed with `Codecs/Notes.txt` (NU5118).
- `OoBDev.Analyzers` sets `SuppressDependenciesWhenPacking` (an analyzer-only package has no lib assembly, NU5128).
- `OoBDev.Data.Vectors.DB` sets `NU5118` in `NoWarn`: the SQL project SDK adds the dacpac to the package itself, so the second add is expected.
- Tried `PackagePath="\examples\%(RelativeDir)"` for the examples; it duplicated directories elsewhere and was reverted.
- Verified: building `OoBDev.System`, `OoBDev.Analyzers` and `OoBDev.Data.Vectors.DB` with `--no-incremental` shows no NU warnings. `NU1902` (MessagePack, via Aspire) remains in BACKLOG.
