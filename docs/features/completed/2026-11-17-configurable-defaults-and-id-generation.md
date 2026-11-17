# Feature request: configurable defaults and an id generation service

- **Status:** done (built 2026-11-17, see [change](../../changes/2026-11-17.md#configurable-defaults-and-id-generation-closed))
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

## Outcome

1. `EmbeddingSentenceTransformerQueueReaderOptions.RestartDelay` (default 10 s) drives the host restart delay; stopping the host during that delay no longer throws `TaskCanceledException`.
2. `StreamDevice` takes `int? minimumTransmissionDelay`: explicit value, else the new optional `IDeviceDefinitionTransmissionDelay` of the device definition, else 1000 ms.
3. `InProcessMessageProvider` takes an optional `IGuidProvider` (existing platform-style service) for generated correlation ids, falling back to `Guid.NewGuid()`.
4. `BsonTypeInfoResolver` has a constructor taking extra id attribute type names.

Tests: new `OoBDev.Data.Vectors.Hosting.Tests`, `StreamDeviceTests`, `BsonSerializerTests`, `InProcessMessageProviderTests`.
