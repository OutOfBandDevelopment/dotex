# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open


### Q1 (2026-10-29, DevTerm plugins) Where does the Sample plugin live?

`OoBDev.DevTerm.Plugins.Sample` must stay a separate assembly, because the plugin-loader tests load it from `plugins/sample` the way a real plugin loads. Today it sits beside the applications and reads like a shipped plugin, although it is only a loader-test fixture.

**Options**
- A. Rename it to a test fixture project beside the tests, `OoBDev.DevTerm.Core.TestPlugin` (recommended): the name says what it is, and nothing can mistake it for a product plugin. Costs one rename (project, namespace, loader-test paths).
- B. Keep the name and add a readme line that says it is a fixture. No churn, but the name still suggests a product.
- C. Compile a plugin assembly at test time (Roslyn). No fixture project at all, but slower and more fragile tests.

**Default:** A when answered; until then it stays as is.

**Your answer:**

### Q2 (2026-10-29, DevTerm tests) Where does the shared user-flow test base live?

`UserFlowTestsBase` and `IFrontEndDriver` are identical copies in `OoBDev.DevTerm.Web.Tests` and `OoBDev.DevTerm.Tui.Tests` (only the namespace and a few usings differ). Layering rule: a new project needs a confirmed migration plan first.

| Project | Current | Target | Namespace | Unit | References that change |
|---|---|---|---|---|---|
| `UserFlowTestsBase`, `IFrontEndDriver` | copied in `Web.Tests` and `Tui.Tests` | new `src/Applications/OoBDev.DevTerm.TestUtilities` | `OoBDev.DevTerm.TestUtilities` | test library, not packed | `Web.Tests` and `Tui.Tests` add a ProjectReference; the copies are deleted; `FakeDeviceConfigEditor`, `FakeTransport` and `ManualTimeProvider` (also duplicated in `Core.Tests` and `Tui.Tests`) can move later |

**Options**
- A. New `OoBDev.DevTerm.TestUtilities` project in `src/Applications` as above (recommended): one copy, the application layer stays the only place that knows DevTerm, and it follows the "device libraries share helpers" rule in `testing.md`.
- B. Put them in `OoBDev.Devices.TestUtilities` (the shared device test helpers). One fewer project, but it would then reference DevTerm Core-level concepts, which a HardwareDevices-wide helper must not.
- C. Leave the two copies. No work now, but every flow added must be edited twice.

**Default:** C until answered.

**Your answer:**

### Q3 (2026-10-29, DevTerm Web) Should the web host's AsyncAPI document use `OoBDev.AsyncApi`?

`OoBDev.DevTerm.Web` serves a hand-written AsyncAPI 3.0 document (`/asyncapi.json`: the `/ws` and `/ws/{id}` text WebSockets and the `/api/events` SSE stream) and a dependency-free viewer page (`/asyncapi`). `OoBDev.AsyncApi` plus `MapAsyncApi()` (in `OoBDev.AspNetCore.Mvc`) builds documents from queue adapters' configuration and serves them with a viewer that loads a web component from unpkg. The Framework model has no channel parameters, no operations, no component messages and no non-queue servers, and its viewer needs a CDN, which a bench machine without internet cannot reach.

**Options**
- A. Keep the Web host's own document and viewer, and record why (recommended): it describes streams, not queues; the offline viewer is a requirement on a lab network. No Framework change; the cost is two AsyncAPI code paths.
- B. Extend `OoBDev.AsyncApi` with static channels (parameters, operations, component messages) and a `ViewerScriptUrl` that can point at a bundled script, then have the Web host contribute its channels. One path and a reusable feature, but it grows a Framework library and the ASP.NET Core mvc project for one consumer, and needs a migration plan.
- C. Use the Framework builder only for the document and keep the offline viewer. Smaller than B, but still needs the model extension.

**Default:** A until answered.

**Your answer:**
