# DevTerm message router

Date: 2026-10-29. Branch `dev/example-ui-tests`.

- `OoBDev.DevTerm.Core` gains `MessageRouter` (an originating presenter that publishes matching device lines to an `IMessageSink` and turns broker messages into device writes, with confirm/drop/always), `RoutingRule` (validated regex rules with `${group}` templates, 1 s regex timeout), `RoutingRuleSet` (JSON) and `RoutedMessage`.
- 5 ported tests added; `OoBDev.DevTerm.Core.Tests` 57 passing, build without warnings.
- Not done: router tests for `MessageRouter` itself (dev-term had none), broker transports, plugin loader, servers, stream content.
