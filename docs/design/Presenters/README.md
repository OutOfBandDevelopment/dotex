# Presenters

**Status:** Built in DevTerm (reconciled with `src` 2026-11-25): `IPresenter`, `PresenterCatalog` and the presenters live in `src/Applications/DevTerm/OoBDev.DevTerm.Core` (`Presenters/`); the framework has no separate presenter library.

Text, numeric, protocol-decoding and rendering presenters that turn a transport's bytes into something readable. Moved from the DevTerm design folder on 2026-11-04. Application: [DevTerm](../../applications/devterm/README.md).

- [presenters.md](presenters.md) - presenter kinds, encodings, decoders, rendering and mappable presenters
- [raster-export.md](raster-export.md) - the shared scene and rasterizer behind PNG/JPG export
