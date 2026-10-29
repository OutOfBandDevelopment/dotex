# `[AcceptsQuery]` attribute (Accept-Query header)

**Date:** 2026-10-28
**Status:** Complete

`OoBDev.AspNetCore.Mvc.Filters.AcceptsQueryAttribute` is a result filter that adds `Accept-Query: "application/json"` to the response of a search action, so clients can discover HTTP `QUERY` support (REQ-006 in [HttpQueryVerb](../design/HttpQueryVerb/README.md)). No header is added elsewhere.

Verified: `AcceptsQueryAttributeTests` (Unit) pass.
