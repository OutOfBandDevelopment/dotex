# Git workflow

- The primary repository is the local bare repo `origin` (`G:/oobdev/dotex.git`). GitHub (`upstream`, `OutOfBandDevelopment/dotex`) is no longer primary: never push to it, never use `gh` (no PRs, issues or Actions), and do not wait on GitHub CI. Verify locally (`dotnet test`, the Docker integration stack).
- Commit as you work, in small logical steps, on a working branch, never directly on `main`. If you are on `main`, create a branch first (`dev/<topic>`). Keep related stacked work on that one branch rather than one branch per task. Pushing the working branch to `origin` (the local bare repo) is allowed as needed (owner, 2026-10-22). Merging into `main` still waits for the owner; there are no pull requests.
- Commit messages must never mention Claude or AI: no `Co-Authored-By`, no "generated with" line. A hook (`.claude/hooks/guard-git.mjs`) blocks these.
- Never force-push, amend published commits, or rewrite history without explicit approval in that turn.
- Before committing, run `git status` and stage only the files that belong to the change (name paths explicitly; no `git add -A`).
- Unrelated working-tree changes the owner made (for example an existing edit to `docs/todo.md`) are theirs: do not stage, commit or revert them unless asked.
