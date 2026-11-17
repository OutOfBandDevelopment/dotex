# Git hooks

Versioned hooks for this repository. Install them once per clone:

```powershell
pwsh .githooks/setup.ps1            # sets core.hooksPath to .githooks
pwsh .githooks/setup.ps1 -Remove    # back to the default .git/hooks
```

| Hook | When it runs | What it does |
|------|--------------|--------------|
| `post-merge` | after `git merge` or `git pull` completes | If the current branch is `main` and `HEAD` is a merge commit, runs `scripts/ci/pack.ps1`: `dotnet pack` of `OoBDev.slnx` into `G:\nuget\` and `npm pack` of publishable npm projects into `G:\npm\`, all with the GitVersion version. Other branches and fast-forwards do nothing. |

Notes:
- Git ignores the exit code of `post-merge`, so a failed pack does not undo the merge; check the output.
- Packing runs after the tests, so a merge that fails them is never packed.
- A `git pull` that brings in a merge commit on `main` also packs.
- Hook files use LF line endings (`.gitattributes`) because git runs them with `sh`.
