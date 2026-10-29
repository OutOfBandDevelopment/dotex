# SCPI reply tracker and auto-detect (2026-10-29)

`ScpiReplyTracker` pairs queries with replies. `QueryAsync(request)` queues the request through a `send` delegate (for example the stream device's `Transmit`) and waits for the oldest open slot's reply; the consumer feeds decoded replies to `OnReply`. A query that times out or is cancelled keeps its place and discards its late reply, so later answers stay in step; a reply with nothing open raises `Unsolicited`. `SendAsync` is for commands without a reply. `ScpiAutoDetect.IdentifyAsync(tracker, catalog, terminator)` sends `*IDN?`, matches the reply with `TryMatchByIdn` and falls back to `ScpiProfileCatalog.Generic`.

The tracker is a separate type, not part of `ScpiDevice`, because SCPI sends `ScpiRequest` and receives `ScpiReply`, which a single `IStreamDevice<T>` cannot carry. Wire it with a transmit-only and a receive-only device, or a pair that share the port.

Tests (Unit, 11 added, 46 pass in `OoBDev.Scpi.Tests`): reply, two queries in order, timeout then late reply, cancel, refused send, non-query, unsolicited reply, command send, null argument, known and unknown `*IDN?`. Not done: the generated control panel (DevTerm merge) and a real instrument.
