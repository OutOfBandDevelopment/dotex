# Feature request: HP-GL vector scene to an oscilloscope display (osci-render)

- **Status:** new
- **Requested by:** owner (notes repository project mapping, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.Documents.Hpgl` and a new sink project

## Need

`OoBDev.Documents.Hpgl` parses plotter output into a scene of polylines (`VectorScene`, `VectorPolyline`, `VectorPoint`) and writes SVG. The notes
repository has an osci-render client prototype that sends vector frames to the osci-render program over TCP so a scope in X-Y mode draws them. The two
share the same data shape, but nothing connects them: a captured plot cannot be drawn on a scope.

## Proposed behavior

A sink that takes a `VectorScene` and sends it as frames to an osci-render TCP endpoint, scaled to the display range, with an option to loop. Example:
`hpgl-capture.plt` is converted to a scene, then drawn repeatedly on the scope.

## Acceptance criteria

- Given a scene with one polyline, when sent, then one frame with the same points in order reaches a test listener.
- Given coordinates outside the display range, when sent, then they are scaled to fit, keeping the aspect ratio.
- Given an empty scene, when sent, then nothing is sent and no error is raised.
- Given the endpoint is not listening, when sent, then a clear connection error is returned.

## Existing code checked

- `OoBDev.Documents.Hpgl` and its README: the scene model exists; only SVG output.
- `docs/applications/devterm/specs/stream-monitor.md`: mentions an oscilloscope stream, as input to DevTerm, not output to a scope.
- The notes prototype is in a separate repository (`proving-grounds`, `examples/OsciRenderClient`); it was not built or run, and the osci-render wire
  format is unconfirmed there. This request depends on confirming that first.

## Security and operations

Sends data to a configured TCP endpoint on the local network; no credentials. Cost is one connection.

## Priority

- **Impact:** a use of existing parsing; niche.
- **Needed by:** no date
- **Workaround today:** none.

## Open questions

- Is a sink in dotex wanted, or should the notes prototype stay a sample? The wire format must be confirmed against the osci-render version in use before any design.
- Own project, or an output format of the Hpgl library?

## Triage notes

Origin: notes repository `projects/osci-render-client` and `OoBDev.Documents.Hpgl`. Low priority and speculative; accept, schedule or decline.
