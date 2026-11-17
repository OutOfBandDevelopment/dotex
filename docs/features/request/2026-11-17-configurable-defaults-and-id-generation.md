# Feature request: configurable defaults and an id generation service

- **Status:** triaged
- **Requested by:** owner (source `TODO` comments, collected 2026-11-17)
- **Date:** 2026-11-17
- **Area:** `OoBDev.Data.Vectors.Hosting`, `OoBDev.System.IO.Pipelines`, `OoBDev.MessageQueueing`, `OoBDev.System` (BSON type info)

## Need

Some behavior is hard-coded where an option or an injected service belongs: the retry delay of the embedding queue reader, the minimum transmission delay of a stream device, the correlation id of in-process messages (`Guid.NewGuid()`), and which BSON attributes are recognized.

## Proposed behavior

Options classes (`AddValidatedOptions<T>()`) for the queue-reader delay and the device delay (with the device definition or its attributes able to override the default); an `IIdGenerator`-style service for message correlation ids that tests can replace; an extension point for the BSON attribute names.

## Acceptance criteria

- Given a configured delay, when the queue reader waits after an error, then it waits that long (default unchanged, 10 s).
- Given a device definition with its own minimum delay, then it overrides the default of 1000 ms.
- Given a replacement id generator, then in-process messages carry its ids.

## Existing code checked

`docs/design/Communications/Core/requirements.md` mentions id generation in passing. No request or design defines these options.

Source comments (replaced by a reference to this request):

- `src/Extensions/OoBDev.Data.Vectors.Hosting/EmbeddingSentenceTransformerQueueReaderHost.cs`: "this should be configurable" (10 s delay).
- `src/Framework/OoBDev.System.IO.Pipelines/StreamDevice.cs`: "should this default be overideable from the devicedefinition or it's attributes?" (`minimumTransmissionDelay`).
- `src/Framework/OoBDev.MessageQueueing/Services/InProcessMessageProvider.cs`: "do this better such has having a id generation interface that can be replaced".
- `src/Framework/OoBDev.System/Text/Json/BsonTypeInfoResolver.cs`: "consider making this injectable so it can be extended".

## Security and operations

None; correlation ids stay unguessable by default.

## Priority

- **Impact:** tests and deployments cannot tune these values.
- **Needed by:** no date.
- **Workaround today:** none.

## Open questions

Use `TimeProvider`-style platform primitives where they exist (owner preference) before adding an interface.

## Triage notes

Triaged 2026-11-17 into [BACKLOG.md](../../../BACKLOG.md) section 5 (source TODO comments). Design document to follow when built.
