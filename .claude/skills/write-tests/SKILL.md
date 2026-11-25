---
name: write-tests
description: Write or extend MSTest tests for a class or project to the repo's testing rules (tier categories, naming, Moq, coverage). Use when asked to "write tests", "generate unit tests", "add tests for {class}", "raise coverage", or when a change lacks tests.
---

# Write tests

Standing rules: `.claude/rules/testing.md` (tiers, categories, configuration, determinism) and `coding-standards`. This skill is the procedure.

1. **Find the home.** Every library has its own test project (`{Project}.Tests`, same layer folder). Create it from a sibling's csproj if missing and add it to `OoBDev.slnx`. Read two existing tests in that project first and copy their layout.
2. **Read the code and its design.** Open the class, its `README.{Project}.md` and the testing strategy in `docs/design/<Feature>/`. List the public behaviour: each method, each branch, each error path, null and empty inputs, cancellation.
3. **Pick the tier** per test: `Unit` (isolated, Moq for collaborators), `Simulate` (full stack, in-memory), `Integration` (Docker services), `Hardware` (ends `Assert.Inconclusive` when the device is absent). A test that touches an external service is never `Unit`.
4. **Write the tests.** MSTest and Moq (`Directory.Packages.props` has the version). Names `Method_Scenario_Expected`. Body in Stage, Mock, Test, Assert, Verify comments. One tier per test via `[TestCategory(TestCategories.Unit)]`. Test the commands and view models through their real `ICommand` types. Use `TimeProvider` fakes, never `DateTime.Now`. Floating point with `NumericAsserts.AreSimilar()`. Configuration from `TestContext.GetRequiredProperty<T>()`. A test written from a bug report also gets `BugRegression`.
   **Log what the test does** (owner, 2026-11-17). Add `public required TestContext TestContext { get; set; }` and call `TestContext.WriteLine` as the test progresses, so the output of a run tells how far it got:
   - one line per phase, for example `Stage: temp directory C:...`, `Test: GET /api/items (page 2)`, `Assert: 3 items, first id 42`;
   - a test that needs a Docker stack service uses `[IntegrationTestMethod("oobd-test-<name>")]` (never plain `[TestMethod]` plus a port probe) and is verified with the stack down;
   - the inputs that matter (endpoint, container, file, parameters), the values the code returned, counts and elapsed time for anything slow;
   - before every `Assert.Inconclusive` or early return, the reason (`Hardware not detected on COM3`, `GROQ_API_KEY not set`); in a loop or retry, log each attempt;
   - for Integration, Simulate and Hardware tests log the service or device used and what was created or cleaned up; Unit tests may keep it to one or two lines;
   - never secrets, tokens or passwords (log `ApiKey: set`, not the key). Keep lines short and deterministic (no timestamps or random ids in files committed to the repository).
   Assert messages say what was expected (`Assert.AreEqual(expected, actual, "item count after delete")`).
5. **Cover the edges**, not only the happy path: invalid arguments, boundaries, failures from dependencies, disposal, concurrency where the type claims thread safety.
6. **Run them.** `dotnet test src/{Path}/{Project}.Tests --filter TestCategory=Unit`; for coverage add `--collect:"XPlat Code Coverage"`. Framework layer needs 80%+. Fix failures in the code or the test, never delete or weaken a test to pass. If something was not run (Docker, device), say so.
7. **Track.** New test properties go in `src/.runsettings` and `TEST_VARIABLES.md`; update the testing strategy document when coverage targets or scenarios change; finish with the `complete-work` skill if this closed a backlog item.
