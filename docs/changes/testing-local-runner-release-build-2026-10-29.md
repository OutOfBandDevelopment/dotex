# Local runner verified for Unit and Simulate (2026-10-29)

`scripts/ci/run-tests.ps1` (restore, Release build of `src/`, Unit and Simulate tests, trx to `artifacts/test-results`) ran end to end: every test project passed, none failed.

It found a Release-only build break: `OoBDev.Common.Hosting` imported `OoBDev.MailKit.Hosting` under `#if DEBUG` but called `TryAddMailKitHosting` in every configuration. The `using` is now unconditional.

The `-Category Integration -Configuration Debug` path also ran end to end (about 8 minutes): the runner started the Docker stack, built, ran every Integration project with 0 failures, and stopped the stack with `--clean`.
