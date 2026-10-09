# Testing - Live Integration Testing (completed work)

**Date:** 2026-10-09
**Epic:** Testing
**Status:** ✅ COMPLETE (for the items below)
**Impact:** Archived from `TODO-testing-live-integration.md`

---

## Summary

The `LiveIntegration` category exists and is documented. The cloud services it was meant for changed since: Azure B2C was dropped and Application Insights was replaced by OpenTelemetry (see [the OpenTelemetry change](migration-opentelemetry-2026-10-09.md)), leaving Groq.

---

## Archived Sections

### Completed Work

### Test Categories Enhancement (COMPLETED - 2026-01-19)

- [x] Added `LiveIntegration` category to `src/Framework/OoBDev.TestUtilities/TestCategories.cs`
- [x] Updated XML documentation clearly explaining:
  - LiveIntegration is for cloud services that cannot be emulated
  - Requires valid cloud credentials and active service subscriptions
  - Manual execution only, NOT run in CI/CD pipelines
  - Examples: Groq Cloud
- [x] Clear distinction from Integration category (Docker-based, runs in CI/CD)

---
