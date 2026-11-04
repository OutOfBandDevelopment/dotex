# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open

### Q1 - 2026-11-04 - Generated guides - where does the generic API guide renderer live?

`ApiGuideRenderer` (OpenAPI/AsyncAPI document to a markdown guide) sits in `src/Applications/DevTerm/OoBDev.DevTerm.TestUtilities`. The Example WebApi guide and any other application guide need it too, and it has no DevTerm-specific names. The generated command-line tool guides (`src/Tools`, BACKLOG section 1) will need a sibling helper (run a tool, capture command lines and output, write a guide), so the answer places both.

**Options**
- A (recommended): move it to a Framework test-utility library with a generic name (for example `OoBDev.Documents.Guides`, namespace `OoBDev.Documents.Guides`), and DevTerm, Example and later apps reference it. Rule `layering-and-migration-plans`: reusable means Framework. Cost: a new Framework project with readme and tests.
- B: keep it in DevTerm.TestUtilities and let Example reference DevTerm test code. Cheapest, but a generic capability sits under an application and couples applications.
- C: copy it into the Example test project. No coupling, but two copies drift.

**Default:** none; the Example WebApi guide waits. Other items continue.

**Your answer:**

### Q2 - 2026-11-04 - DevTerm - where does the generic external converter (Ghostscript, GhostPCL, HP-GL to SVG) live?

The BACKLOG item asks to make the DevTerm conversion for PCL and PostScript generic and part of the document conversion framework. Today `StreamCaptureConverter` and `StreamConvertToolOptions` sit in `OoBDev.DevTerm.Core/Configuration`; the framework already has `IDocumentConversion` in `OoBDev.Documents.Abstractions` and `Conversion` in `OoBDev.Documents`.

**Options**
- A (recommended): a Framework `IDocumentConversion` provider that runs registered external tools (name, path, argument template, formats, output extension, dpi), plus an HP-GL to SVG converter; DevTerm keeps only its tool-list store and editor and calls the framework. Reusable means Framework (rule `layering-and-migration-plans`). Cost: a migration plan table first, a Documents project change, DevTerm refactor.
- B: a new Framework project `OoBDev.Documents.ExternalTools` so `OoBDev.Documents` stays free of process execution. More projects, clearer dependencies.
- C: leave it in DevTerm and add HP-GL to SVG there. No layering work, not reusable.

**Default:** none; the item waits and I take the next one.

**Your answer:**

### Q3 - 2026-11-04 - Generated guides - move the Example web guides from `docs/example` to `docs/applications/example`?

The Example web app is already an `.esproj` project with `npm test` and generated journey guides, specs and run books, but they are written to `docs/example` (`e2e/guide.ts`, `scripts/examples/verify-example-web.ps1`, `future-example.md`). The `work-tracking` rule and the GeneratedGuides design put application docs in `docs/applications/{app-name}/`.

**Options**
- A (recommended): move the folder to `docs/applications/example/` and change the generator default, the verify script, the links and the docs map in one commit. Matches the rule and DevTerm; the docs tests cover the links.
- B: keep `docs/example` and note it as the exception in the GeneratedGuides design. No churn, but two layouts.

**Default:** B until you answer (nothing is moved).

**Your answer:**

### Q4 - 2026-11-04 - Example AppHost - how do the live Keycloak journeys run?

The Example AppHost starts the API with `IDENTITY_PROVIDER=None`, so the Keycloak sign-in and token-claims live journeys fail ([details](docs/design/example-ui/apphost-live-journeys.md)). The telemetry journey also fails live (OTLP from the host-run API does not reach the dashboard); that needs a live host run to diagnose and I take it after this answer.

**Options**
- A (recommended): wire Keycloak into the host (`IDENTITY_PROVIDER=Keycloak`, authority from the Keycloak endpoint) so the journeys run for real, as in the compose stack. Cost: the web client needs the host's Keycloak address, and sign-in becomes required for every other live journey (they already sign in through `signInIfAsked`).
- B: keep the host anonymous and mark those journeys `liveOnly` plus `needsIdentity`, skipped when the API is anonymous. No wiring, but the host never proves sign-in.

**Default:** B's behaviour (nothing changes) until you answer.

**Your answer:**

### Q5 - 2026-11-04 - Example apps - do the device projects need Example-framework coverage?

BACKLOG section 1 lists example apps that use the UiDefinitions language beyond DevTerm, with the open spike question whether the device projects need coverage in the example framework at all, since DevTerm already covers them ([ui-language-later.md](docs/design/DevTermParity/ui-language-later.md)).

**Options**
- A (recommended): no. DevTerm is the reference application for devices; the Example apps cover non-device capabilities only. Smallest scope; the item becomes the list of Example screens in `missing-screens.md`.
- B: yes, one Example panel per device family. Shows the language on a second host, at the cost of duplicating DevTerm's device UI.

**Default:** A.

**Your answer:**
