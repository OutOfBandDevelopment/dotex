# Tui guide determinism (2026-11-04)

A full Tui Unit run rewrote `tui-manifest-editor*` and `tui-plugin-approval*` guide files on every run. Both had content that changes per run.

## Causes and fixes

| Image | Cause | Fix |
|-------|-------|-----|
| `tui-manifest-editor*` (PNG and TXT) | The editor title showed a random `Path.GetRandomFileName()` folder | `ManifestEditorModeTests` uses a fixed leaf name under a unique parent |
| `tui-plugin-approval.png` | The dialog prints the plugin folder path (temp folder with a Guid, user name) | Masked zone `plugin-approval-temp-path` in `images/zones.json`; test root and home are separate folders |
| `tui-plugin-approvals.png` | The picker prints the approval time (wall clock) | Masked zone `plugin-approvals-time` |

## Verification

- `Tui.Tests` Unit: 366 passed.
- Two consecutive runs of `ManifestEditorModeTests` and `PluginApprovalDialogTests` leave the hashes of all `tui-manifest-editor*` and `tui-plugin-approval*` files identical.
- A second full Unit run was aborted by the shell timeout, so a full back-to-back comparison was not completed.
- The committed manifest-editor images and `tui-plugin-approval.png` change once, because they were captured with the random values.

## Found while doing this (in `BACKLOG.md`)

- Restore of the whole `OoBDev.slnx` fails with NU1201.
- The docs Unit tests fail on the imported DevTerm history.
