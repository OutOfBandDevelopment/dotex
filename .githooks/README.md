# Git hooks

Versioned hooks for this repository. Install them once per clone:

```powershell
pwsh .githooks/setup.ps1            # sets core.hooksPath to .githooks
pwsh .githooks/setup.ps1 -Remove    # back to the default .git/hooks
```

| Hook | When it runs | What it does |
|------|--------------|--------------|
| `pre-merge-commit` | before a clean merge on `main` creates its merge commit | Runs `scripts/ci/run-tests.ps1` (restore, build, Unit and Simulate tests) on the merge result. A failure stops the merge commit: the merge stays staged, so fix the cause and `git commit`, or `git merge --abort`. Skip it once with `git merge --no-verify`. |
| `post-merge` | after `git merge` or `git pull` completes | If the current branch is `main` and `HEAD` is a merge commit, runs `scripts/ci/pack.ps1`: `dotnet pack` of `OoBDev.slnx` into `G:\nuget\` and `npm pack` of publishable npm projects into `G:\npm\`, all with the GitVersion version. Other branches and fast-forwards do nothing. |

Notes:
- Git ignores the exit code of `post-merge`, so a failed pack does not undo the merge; check the output.
- `pre-merge-commit` does not run for a fast-forward or for `--no-commit`; merges into `main` are always `--no-ff` merge commits, so it covers them. Git has no `pre-merge` hook.
- Packing runs after the tests, so a merge that fails them is never packed.
- A `git pull` that brings in a merge commit on `main` also packs.
- Hook files use LF line endings (`.gitattributes`) because git runs them with `sh`.
