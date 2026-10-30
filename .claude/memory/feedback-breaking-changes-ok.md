---
name: feedback-breaking-changes-ok
description: Framework is unreleased; breaking public API changes are fine, no obsolete forwarders needed
metadata:
  type: feedback
---

Owner said (2026-10-09): "dont worry about public breaking changes... this is an unreleased framework".

**Why:** nothing is published or consumed yet, so compatibility shims (aliases, `[Obsolete]` forwarders) are just clutter.

**How to apply:** rename or remove outright instead of keeping legacy keys or forwarders; update docs and tests in the same change. Supersedes the "No breaking changes" line in CLAUDE.md. See [[user-rejected-third-party-deps]] for other owner decisions.
