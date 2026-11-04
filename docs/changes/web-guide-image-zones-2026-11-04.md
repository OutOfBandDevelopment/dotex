# Web guide images stop churning

**Date:** 2026-11-04

Every run of `OoBDev.DevTerm.Web.Tests` rewrote five `web-blazor-*` guide images because of volatile text: temp folders with GUIDs (plugins, monitor, manifest), the loopback port and capture time (monitor), and the open-connection ids and row order (connections). `zones.json` now masks those regions for `web-blazor-plugins`, `-monitor`, `-manifest`, `-connections` and `-connections-project`, so `image-mask sync` leaves the committed images alone.

Verified: the plugins, monitor, manifest and connections tests were run two to three times each after the change with no image changes in git.
