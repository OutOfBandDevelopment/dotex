# Git hooks

Versioned hooks for this repository. Install them once per clone:

```powershell
pwsh .githooks/setup.ps1            # sets core.hooksPath to .githooks
pwsh .githooks/setup.ps1 -Remove    # back to the default .git/hooks
```

| Hook | When it runs | What it does |
|------|--------------|--------------|
| `post-merge` | after `git merge` or `git pull` completes | If the current branch is `main` and `HEAD` is a merge commit, runs `scripts/ci/pack.ps1` (`dotnet pack` of `OoBDev.slnx` into `G:\nuget\`, versioned by GitVersion). Other branches and fast-forwards do nothing. |

Notes:
- Git ignores the exit code of `post-merge`, so a failed pack does not undo the merge; check the output.
- The hook runs right after the merge, before the post-merge tests of the `merge-to-main` rule, so packages from a merge that later fails its tests can exist in `G:\nuget\`. The next merge produces a higher version.
- A `git pull` that brings in a merge commit on `main` also packs.
- Hook files use LF line endings (`.gitattributes`) because git runs them with `sh`.
