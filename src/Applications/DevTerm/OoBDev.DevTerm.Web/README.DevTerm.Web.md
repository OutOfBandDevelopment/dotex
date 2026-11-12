# OoBDev.DevTerm.Web

ASP.NET Core and Blazor front end for DevTerm, ported from dev-term `DevTerm.Web` onto `OoBDev.DevTerm.Core`.

## Description

A host that opens one device session (`AddDevTermFrontEnd` selects the transport from the command line) and serves it to browsers: a SignalR session hub, a REST API described by OpenAPI (Scalar reference), an AsyncAPI document, and Blazor pages for the terminal, playback and generated device panels (`PanelApi`, `PanelHost`). Roles are mapped to rights as in the other API surfaces.

## Verified

`OoBDev.DevTerm.Web.Tests`: 49 Unit tests pass (2026-11-12), none ignored. The Playwright user-flow and screenshot tests are `DevLocal` and were not run.

## Not done

See the `DevTerm 100% parity` item in `BACKLOG.md`: `BundleDevicePlugins`, the k8055 and busylight panels, merging the local AsyncAPI, OpenAPI and Scalar code with `OoBDev.AsyncApi` and its helpers.
