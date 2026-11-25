# OoBDev.DevTerm.Web

ASP.NET Core and Blazor front end for DevTerm, ported from dev-term `DevTerm.Web` onto `OoBDev.DevTerm.Core`.

## Description

A host that opens one device session (`AddDevTermFrontEnd` selects the transport from the command line) and serves it to browsers: a SignalR session hub, a REST API described by OpenAPI (Scalar reference), an AsyncAPI document, and Blazor pages for the terminal, playback and generated device panels (`PanelApi`, `PanelHostHolder`). Roles are mapped to rights as in the other API surfaces.

## Verified

`OoBDev.DevTerm.Web.Tests`: 49 Unit tests pass (2026-11-12), none ignored. The Playwright user-flow and screenshot tests are `DevLocal` and were not run.

## Not done

See the `DevTerm 100% parity` item in `BACKLOG.md`: `BundleDevicePlugins`, the k8055 and busylight panels, merging the local AsyncAPI, OpenAPI and Scalar code with `OoBDev.AsyncApi` and its helpers.

## Session tokens for remote clients

`POST /api/token` with the shared key (or the read-only key) as `Authorization: Bearer` returns a 15-minute JWT (`access_token`, `expires_in`, `expires_at`). Send it as a Bearer on every other route. The token carries the read-only flag of the key it came from, dies with the host process, and cannot be exchanged for another one. See [Remote DevTerm](../../../../docs/design/RemoteDevTerm/README.md).

## Device tunnel

`/ws/device/{name}` (binary WebSocket) opens the device of a saved connection of this host and relays its bytes: a frame received is written to the device, a frame sent is what the device returned. A read-only token gets 403, an unknown name 404, a device that will not open 502. The client side is `RemoteDeviceAdapter` in `OoBDev.DevTerm.Core` (`OoBDev.DevTerm.Remote`): it exchanges the shared key for a session token and opens the tunnel. See [Remote DevTerm](../../../../docs/design/RemoteDevTerm/README.md).
