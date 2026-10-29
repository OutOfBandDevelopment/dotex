# Oobtainium

**Consolidated:** 2026-10-29 · **Status:** not migrated; the January 2026 feature mapping and migration plan are removed (`git log -- docs/migration` has them).

`OoBDev.Oobtainium` is a mocking and proxy framework (about 1,600 LOC, 48 files) built on dynamic interface proxies with call recording.

- **Where it is:** the owner moved it to the proving-grounds repository (`C:\repo\mine\proving-grounds\examples\OoBDev.Oobtainium`, upstream `OutOfBandDevelopment/oobtainium`) on 2026-01-20. It is not in `Incoming/` any more.
- **Recommendation of the investigation:** do not migrate. Mocking is a solved problem, Oobtainium has little that Moq or NSubstitute lack, and it would need modernising; the effort belongs on features unique to this framework.
- **Current state:** listed as "evaluate later" (decision D7) in [BACKLOG.md](../../BACKLOG.md) and [harvest-candidates](../harvest-candidates.md). Nothing is scheduled. If the owner reopens it, start with a fresh comparison against the proving-grounds copy.
