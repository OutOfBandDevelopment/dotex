# DevTerm WPF: routing rule editing and history (2026-10-30)

The routing window now edits the rule list: add, remove, move up and down, one row per rule (direction, topic, match, payload) with its hit count. A sample box tries a device line against the rules (one result line per rule), and Refresh updates hit counts and the routed-message history. `RoutingRuleRow` wraps the core `RoutingRule`; the six commands are real `ICommand` types over `RoutingDialogViewModel`.

Verification: WPF Unit suite 126 passed (6 new in `RoutingRuleEditingTests`). The window was not viewed running.
