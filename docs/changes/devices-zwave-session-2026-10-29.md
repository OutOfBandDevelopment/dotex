# Z-Wave session (2026-10-29)

`ZWaveSession` wraps the framework's `IStreamDevice<ZWaveFrame>` (the existing `StreamDevice<T>` over a device adapter is the production one, so no new session observer was needed). It subscribes to `MessageReceived`, queues the answer `ZWaveAckResponder.Respond` picks, applies the frame to its `ZWaveNodeTable` and raises `FrameReceived`; `SendAsync(frame)` queues a request. `AnswerFailed` reports an answer the device refused or could not send. `Dispose` unsubscribes.

Build the stream device with `minimumTransmissionDelay: 0`: the default of 1000 ms is a gap after every transmission, so an acknowledgement could wait behind it close to the stick's timeout. The session cannot change that, so the type documentation says so.

Tests (Unit, 8 added, 50 pass in `OoBDev.ZWave.Tests`) use a fake `IStreamDevice<ZWaveFrame>`: data frame acknowledged and relayed, lone ACK relayed without an answer, refused answer, a throwing transmit, `SendAsync`, `Dispose`, null arguments. Not run: a Hardware pass with a real stick through `StreamDevice<T>`, and the `ZWaveStick` test helper still uses its own loop.
