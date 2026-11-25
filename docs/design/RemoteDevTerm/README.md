# Remote DevTerm

**Status:** in progress (slice 1 built 2026-11-25) · **Last Updated:** 2026-11-25 · **Decision:** owner, Q41 A, 2026-11-25

Devices (serial, HID, USBTMC) stay attached to the machine that has them. DevTerm on another machine reaches them through the DevTerm web host, which replaces the raw USB bridge (`OoBDev.System.IO.Usb.Bridge`, unauthenticated, to be removed by [this request](../../features/request/2026-11-25-remove-raw-usb-network-tunnels.md)).

## Authentication

| Client | Presents | Notes |
|--------|----------|-------|
| Tui and WPF remote client | `Authorization: Bearer <session JWT>` | exchanged from the shared key, refreshed before it expires |
| Browser page | cookie `devterm_auth` (shared key) | unchanged; no JWT in a query string |
| First contact | `POST /api/token` with the shared key as Bearer | returns `access_token`, `expires_in`, `expires_at` |

The JWT is HS256, issuer and audience `dev-term-web`, 15 minutes, signed with a key that lives only in the host process (a restart invalidates all tokens). A read-only key yields a read-only token (claim `devterm_readonly`). A session JWT cannot be exchanged for another (403): renewing needs the shared key.

## Slices

| # | Slice | State |
|---|-------|-------|
| 1 | Token endpoint and JWT acceptance in the host (`SessionJwt`, `AccessTokenMiddleware`) | built, unit and host tests |
| 2 | Remote connection type for the shared device layer: a transport that speaks the WebSocket tunnel (`/ws`) with the JWT in the header, covering serial first | not started |
| 3 | HID and USBTMC over the tunnel (frames for report and bulk transfers, per-session device claim) | not started |
| 4 | Tui and WPF: a "Remote DevTerm" connection (URL, key from the credential store), token refresh | not started |
| 5 | Remove the raw USB bridge and its Ser2NetWin wiring; mark the old design superseded | waits for 3 |

Each slice follows the host's existing requirements and testing strategy in `docs/applications/devterm/`.
