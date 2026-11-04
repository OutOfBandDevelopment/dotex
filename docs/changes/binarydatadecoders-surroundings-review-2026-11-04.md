# BinaryDataDecoders surroundings review (2026-11-04)

Reviewed everything around the BinaryDataDecoders code (README, bug reports, generated docs, XSLT report templates, GitHub workflow and dependabot, `act` scripts, dotnet tools, build scripts) and recorded each item as migrated or rejected with its reason: [the review](../migration/binarydatadecoders-surroundings-review-2026-11-04.md).

- Migrated: the four bug reports and the review status, as history in `docs/applications/binary-data-decoders/bugs/`.
- Rejected: pipeline-generated docs, XSLT templates, GitHub workflow, `act` scripts, badges (GitHub is no longer primary; the guides here are test-generated).
- Verified: links in the new files checked by reading; the docs validator reports no problem in the new files (it still fails only on imported DevTerm history).
- Harvest projects (`proving-grounds`, `embedded-bakery`, `coding-challenges`, `mycloud-monitor`, `my-znet`): no review due, since nothing in them is migrated; the rule is recorded in `docs/harvest-candidates.md` and the backlog item is closed.
