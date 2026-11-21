# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_


## Open


### Q29 · 2026-11-20 · Database tooling · Atlas for the bake-off

The bake-off run ([result](docs/design/DesiredStateDatabase/bake-off-result.md#not-covered)) could not include Atlas: building it from source fails with Go 1.27.1 (a `golang.org/x/tools` constant overflow). May I fetch a binary to a scratch folder outside the repository to run it?

**Options**

- A. Download the vendor's Atlas community binary (Apache 2.0 edition) into a scratch folder and run the scenarios (recommended: the quickest way to finish the comparison, nothing enters the repository, the licence is checked before use).
- B. Build Atlas with an older Go toolchain that Go downloads on demand (same kind of download, slower, keeps the build from source).
- C. Drop Atlas from the comparison and decide between sqldef and pgschema on the recorded results (no download; the buy-or-build verdict then covers two candidates).

**Default:** C until you answer (nothing is downloaded).

**Your answer:**

### Q30 · 2026-11-20 · Example app · Screens for framework areas the plan does not cover

[missing-screens.md](docs/design/example-ui/missing-screens.md#framework-areas-the-plan-does-not-mention) lists framework features with no screen: complex events and schedules, spatial lookup (Census geocoding), data loading, server-driven UI definitions, query over HTTP. The review says each is an owner decision. Which should the example app show?

**Options**

- A. Build the screens for the libraries that exist now: complex events, spatial lookup and data loading (recommended: the example exists to show the framework; each is a small read-mostly page, a screen can be deleted later, and nothing touches live data).
- B. Build only complex events, the one with a hosted service and EF store.
- C. None; the example stays at the current pages and the libraries are shown in their own readmes.

**Default:** A, one screen at a time, starting with complex events.

**Your answer:**

### Q31 · 2026-11-20 · Example app · What "dacpac build and diff" means on the Data access screen

The backlog line asks for a dacpac build and diff half beside the read-only query runner. `OoBDev.DacFx` builds a dacpac for a SQL CLR assembly (`DacPacBuilder`) and validates one (`DacPacValidator`); it has no schema diff. The schema-as-code work has since moved to `OoBDev.Data.Schema.SqlServer` (a SQL project dacpac) and the desired-state bake-off (sqlpackage and other tools).

**Options**

- A. Drop the half: the example shows the query runner only; builds and diffs stay in the CLI tools (`DacPacCompiler`) and the bake-off docs (recommended: the screen would run a build and a schema compare on a server, which the read-only runner deliberately avoids, and the CLI already does both).
- B. Build-only: a page that builds a CLR-assembly dacpac from an uploaded assembly through `DacPacBuilder` and lists its contents (no database access).
- C. Build and diff: option B plus a sqlpackage drift report against a configured test instance (needs sqlpackage in the API image and a write-capable login).

**Default:** A.

**Your answer:**
