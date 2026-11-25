# Pattern 21 — Blackboard (worked example)

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 20 — `IConfigureOptions<>` classes for third-party options](./20-configure-options-classes.md) · [Index →](./README.md)
<!-- nav -->

A blackboard is shared state that independent knowledge sources watch. A source reacts to a change by writing derived facts back; no source calls another. This is a small worked example from the owner's `design-patterns/BehaviorBlackboard` sample (read-only reference, see [harvest candidates](../../harvest-candidates.md)); it is not a framework library.

## Shape

- `IBlackboard`: `Set(sender, key, value)`, an indexer, a read-only `State` snapshot and a `DataChanged` event. `Set` raises the event only when the value actually changed (added, replaced, or removed by `null`); keys are case-insensitive.
- `IKnowledgeSource`: `HandleChange(blackboard, data)`. The source filters on `data.Key` and posts derived values.
- `IControl`: registers sources, posts input, and takes snapshots. It does not forward a change to the source that wrote it (`if (sender == source) return`), so a source cannot trigger itself.

## Example

Two sources: one derives `email` once `first` and `last` exist, the other sets `ready` when `email` appears. Posting `first` changes nothing visible; posting `last` ends with `first`, `last`, `email` and `ready` on the board.

```csharp
var control = new Control(new Blackboard())
    .Add(new KnowledgeSourceHasFullName())
    .Add(new KnowledgeSourceHasEmail())
    .Post("first", "Matt")
    .Post("last", "Whited");
```

## Where it fits

Use it when the order of inputs is not known and derived values should appear as soon as their inputs do (form completion, enrichment, sensor fusion). Compare [strategy collections](./14-strategy-collections.md) (a fixed pipeline picks one provider) and the [message context object](./12-message-context-object.md) (state travels with one message). Cautions: no ordering between sources, a cycle between two sources loops until values stop changing, and the sample's `Set` is not atomic across the check and the event.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 20 — `IConfigureOptions<>` classes for third-party options](./20-configure-options-classes.md) · [Index →](./README.md)
<!-- nav -->
