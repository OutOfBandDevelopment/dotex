---
name: user-ui-mvvm-preference
description: "UI preference: MVVM with command binding (WPF); for required JS/TS frameworks pick ones that support MVVM-style binding"
metadata:
  node_type: memory
  type: user
  originSessionId: 9ea789d6-f84c-46cc-8a8c-3bc487e0ad56
  modified: 2026-09-30T14:11:23.408Z
---

Commands should be real, retestable command types (named classes implementing `ICommand`/command interfaces with injected dependencies), not just relay commands routed to lambda expressions; the same applies to command-like handlers in JS/TS frameworks (named, testable units rather than inline closures).

Prefers MVVM with command binding for UIs where possible (e.g. WPF: ViewModels, `ICommand`, data binding, no code-behind logic). When a JS/TS framework is required, prefer frameworks that support similar view-model + two-way binding + command-style handlers as closely as possible.

**Why:** stated directly by the user as a standing design preference (like the patterns in [[feedback-docs-plantuml]]: recorded preferences to carry forward, not to be "corrected").

**How to apply:** when designing or scaffolding UI (desktop, web, docs/blueprint recipes, industry-alternatives comparisons), default to MVVM + commands; when weighing JS/TS options, rank binding/view-model fit highly and say where a framework diverges. Also applies to UI mockups (PlantUML Salt).
