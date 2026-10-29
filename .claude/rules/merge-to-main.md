# Merging into main

Trigger: the owner says to merge into `main`. That instruction is the yes for the merge and for deleting the merged branches; it is not a yes for anything else. Without it, never merge into `main` (`git-workflow` rule).

1. **Prepare.** Commit pending work on the current branch with the tracking files current (`work-tracking` rule), tests green, and nothing answered but unprocessed in `OPEN-QUESTIONS.md`. `git fetch --prune`.
2. **Merge.** Switch to `main`, fast-forward it from `origin/main`, merge the branch with `git merge --no-ff` (always a merge commit, never a fast-forward: the merge commit is what bumps the version, see `GitVersion.yml`; owner, 2026-10-29), build and run the relevant tests on the result, then push `main` to `origin`. If the merge conflicts or tests fail, stop on the branch and fix or report; never force-push.
3. **Clean up.** Delete remote branches fully merged into `main` (`git branch -r --merged origin/main`, excluding `main`) with `git push origin --delete <branch>`, and the matching local ones with `git branch -d` (never `-D`). List unmerged branches to the owner instead of deleting them. Finish with `git remote prune origin`.
4. **Continue.** Keep working on a new `dev/<topic>` branch created from the updated `main`.
5. **Report** in one or two lines: what merged, which branches were deleted, which remain and why.
