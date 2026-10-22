# Example web: view-model tests for the remaining pages

**Date:** 2026-10-22 · **Design:** [example-ui](../design/example-ui/README.md)

- New `Communications.test.ts`, `CurrentUser.test.ts`, `Queues.test.ts` and `Templates.test.ts` in `src/Examples/OoBDev.Example.Web/src/pages`, in the style of `Cache.test.ts`: `canExecute` rules, request path and body, and the result shown.
- Verified: `npx vitest run` 19 files, 62 tests pass. No browser run.
