# Feature request: HP-GL vector scene to a stereo audio signal for an X-Y oscilloscope

- **Status:** triaged
- **Requested by:** owner (notes repository project mapping, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.Documents.Hpgl` (or a new project, see open questions)

## Need

`OoBDev.Documents.Hpgl` parses plotter output into a scene of polylines (`VectorScene`, `VectorPolyline`, `VectorPoint`) and writes SVG. SVG covers viewing on a screen. A scope in X-Y mode draws a scene when the left channel drives X and the right channel drives Y (osci-render does this), so a captured plot can be drawn on a scope from a stereo line output. Nothing in dotex produces that signal. (Owner, 2026-11-17: no osci-render TCP sink; generate the audio signal directly.)

## Proposed behavior

Resample a `VectorScene` along its polylines into a stereo float signal: left is X, right is Y, scaled to fit -1 to 1 keeping the aspect ratio. The first target is a deterministic PCM WAV writer (sample rate and points per second as options, for example 48 kHz or 96 kHz; the scene repeats for a set duration). Live playback through a sound device is a later option. Example: `hpgl-capture.plt` becomes a scene, then a WAV that is played into the scope.

Known limits, to be documented in the readme:

- There is no Z (blanking) channel, so jumps between polylines show as faint lines unless they are made fast or the scope has a Z input.
- Many sound cards and line outputs are AC-coupled and lose the DC offset; DC-coupled or USB audio devices draw steadier.
- The scope must be in X-Y mode and the level may need attenuating.

## Audio hardware notes (2026-11-17)

- Output is stereo X-Y by default. An optional third Z (blanking) track goes into a multi-channel WAV for devices with more channels, with the channel layout (for example 5.1: Z on the center channel, never on the low-pass filtered LFE) set in options.
- The sample rate is an option. 48 kHz is the baseline: about 1,900 points per frame at 25 frames a second. A device with a faster converter (listings claim up to 384 kHz; unverified, check with a loopback test) allows denser scenes and makes fast jumps almost invisible without Z.
- Candidate devices: a C-Media based 7.1 USB adapter (StarTech ICUSBAUDIO7D: eight discrete analog channels, 16-bit, 44.1 or 48 kHz), the onboard Realtek output when Windows exposes 5.1 or 7.1, or a 2-in/2-out recording interface for X-Y only.
- Not suitable: an S/PDIF decoder box. S/PDIF carries only two PCM channels, so a third discrete channel means a lossy AC-3 or DTS bitstream with filtered bandwidth and channel delay, which smears the Z edges and misaligns Z against X and Y.
- AC-coupled outputs lose the DC offset, so the image can drift; DC-coupled interfaces draw steadier.

## Acceptance criteria

- Given a scene with one polyline, when rendered, then left and right samples follow its points in order.
- Given coordinates outside the range, when rendered, then they are scaled to fit, keeping the aspect ratio.
- Given an empty scene, when rendered, then nothing is produced and no error is raised.
- Given the WAV output, when read back, then it is valid PCM with the configured sample rate and two channels.

## Existing code checked

- `OoBDev.Documents.Hpgl` and its README: the scene model exists; only SVG output.
- The earlier osci-render TCP idea (notes repository prototype `proving-grounds`, `examples/OsciRenderClient`) is dropped; its wire format was never confirmed.

## Security and operations

Writes a local audio file; no network. Set the playback level low first.

## Priority

- **Impact:** a use of existing parsing; niche.
- **Needed by:** no date
- **Workaround today:** none.

## Open questions

- Own project, or an output format of the Hpgl library?
- Which sample rate, and which blanking approach (fast jumps, or an optional Z channel on a third track)?

## Triage notes

Origin: notes repository `projects/osci-render-client` and `OoBDev.Documents.Hpgl`. Low priority.

2026-11-17: rewritten from an osci-render TCP sink to a stereo audio signal (owner). 2026-11-19: the WAV writer is built; only the scope bench check is open.

2026-11-11: triaged. Accepted as a backlog idea with no design yet. Backlog: BACKLOG.md, Devices ideas.

2026-11-19: proposed design written: [HpglAudioSink](../../design/HpglAudioSink/README.md).
