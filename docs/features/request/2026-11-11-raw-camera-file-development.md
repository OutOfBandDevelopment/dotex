# Feature request: Develop camera RAW files (NEF and others) to JPEG or PNG

- **Status:** triaged
- **Requested by:** owner (filed from the notes repository, `projects/applications/second-shooter-photo-library.md`; design concepts only, its code is not reused)
- **Date:** 2026-11-11
- **Area:** new, near `OoBDev.Onnx.ImageEmbeddings.Skia` and the file inventory request

## Need

The owner's photo collection includes Nikon `.nef` files. The photo library design had an "Image Conversion" tool and a type-detect then convert step before any analysis. Two throwaway
attempts existed: a C# loop using ImageMagick (quality 90, hard-coded paths) and a Python notebook using `rawpy` with per-channel scale factors. Neither is a library. Image embeddings,
histograms and thumbnails all need a decoded RGB image, and RAW files cannot be fed to them directly.

## Proposed behavior

1. **Develop:** an interface that takes a RAW file or stream and returns an RGB bitmap or writes JPEG or PNG, with options for quality, 8 or 16 bit output and white-balance channel scales.
2. **Type detection:** content-based detection of RAW formats so a pipeline can decide whether to develop first.
3. **Originals untouched:** output goes to a separate location or stream; the source is never modified.
4. **Provider choice:** a native decoder (LibRaw through a wrapper, or ImageMagick) behind the interface, so a host that cannot ship native code can leave it out.

## Acceptance criteria

- Given a `.nef` file, when developed, then a JPEG of the full sensor size is produced and the source is byte-identical afterward.
- Given a non-RAW file, when type detection runs, then it is reported as not RAW and passed through.
- Given white-balance scales of 1, 1, 1 and a changed scale, when developed, then the output differs only in the scaled channel.
- Given a host without the native provider, when the feature is requested, then it fails with a clear unsupported message.

## Existing code checked

- Search of `src` project files found no LibRaw, rawpy equivalent or ImageMagick dependency; Skia is not known to decode NEF (not tested).
- The old examples (`SecondShooter.Cli` and `notebooks/RawConverter.ipynb`) were read and are only a few lines each; not reused. Neither was run.
- The NAS storage catalog handles HEIC conversion; whether it handles camera RAW was not checked.

## Security and operations

Native decoders parse untrusted binary formats, so they should run in a worker with limits on size and time. CPU cost is high for large files.

## Priority

- **Impact:** unblocks embeddings, thumbnails and color analysis for RAW photos.
- **Needed by:** no date
- **Workaround today:** a hand-written loop with ImageMagick or a Python notebook.

## Open questions

- LibRaw wrapper or ImageMagick as the default provider (licence and native packaging)?
- Is embedded-JPEG-preview extraction enough for cataloguing, with full development only on demand?
- Should sidecar files (XMP) from Lightroom be read here or in the metadata request?

## Triage notes

2026-11-12: design written: [RawDevelopment](../../design/RawDevelopment/README.md); provider is ImageMagick (owner, 2026-11-12).

Origin: notes repository `projects/applications/second-shooter-photo-library.md`. Maintainers: accept, schedule or decline.

2026-11-11: triaged. Accepted as a backlog idea from the storage and photo-library review; the code named above was not read in full, so the first step is the superset comparison before any design. Face and pet matching also needs an owner decision on personal data. Backlog: BACKLOG.md, Consolidation requests.
