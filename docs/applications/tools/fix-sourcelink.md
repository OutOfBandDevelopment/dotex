# fix-sourcelink

Rewrites https links in XML reports to local paths.

## Options

Run the tool with `--help` to list its options.

```console
$ fix-sourcelink --help
fix-sourcelink [--include <glob;glob>] [--exclude <glob;glob>] [--search <folder>] [--target <folder>]
  Rewrites https links in the filename attributes of the matching XML files (for example coverage reports) to local paths under the target folder.
  --include  semicolon-separated globs of the files to fix (nothing is changed without it)
  --exclude  semicolon-separated globs to skip
  --search   folder searched for the files (default: the current folder)
  --target   folder the rewritten paths point into (default: the search folder)
```
