# Open questions

Questions Claude has for the owner. Pick an option (or write your own) under **Your answer**; answering in chat counts the same. Claude keeps working on the **Default** until you answer. Answered questions are removed from this file once the answer is recorded in the feature docs, [BACKLOG.md](BACKLOG.md), [TODO.md](TODO.md) or the rules; history is in `git log`. An answer that raises a new question gets a new entry here.

How this file is kept: see `.claude/rules/open-questions.md` and `.claude/rules/work-tracking.md`.

## Requests

_(none waiting; processed requests are in BACKLOG.md and TODO.md)_

## Open


### Q17 — 2026-11-10 — Security — SonarQube token written into `src/run-sonar.bat`

`src/run-sonar.bat` sets `SONARQUBE_TOKEN` to a literal value, and the file is in git history (commits `0df4a116` and later). Anyone with the repository can use the token against that SonarQube project.

**Options**

- A. Revoke the token in SonarQube now, delete `src/run-sonar.bat`, and re-add Sonar later as a `scripts/ci` step that reads the token from the environment (recommended: a published secret stays valid until revoked, and deleting the file does not remove it from history).
- B. Revoke the token and keep the script, changed to read the token from the environment.
- C. Leave it (only sensible if the token is already dead and the project is gone).

**Default:** nothing changed until you answer; revoking needs your SonarQube access.

**Your answer:**

### Q18 — 2026-11-10 — Build scripts — Legacy scripts at the `src` root

`src` holds `build.bat`, `build.sh`, `package.bat`, `test.bat`, `test.sh`, `deploy-data.bat` and `run-sonar.bat`. The manual CI in `scripts/ci` (`run-tests.ps1`, `run-linux-tests.ps1`) replaced the GitHub pipeline they served. They only work from `src` because `Directory.Build.props` finds `SolutionDir` from the working directory. `package.bat` still names `OoBDev.sln`, which no longer exists.

**Options**

- A. Delete `package.bat` and `deploy-data.bat` (stale), move `build`, `test` and `run-sonar` logic into `scripts/ci` (one entry point per job, `pwsh`), then delete the rest (recommended: one place for scripts, and `scripts/ci` already covers build and test).
- B. Leave them where they are and document them in `src/README.md`.
- C. Delete them all and rely on `scripts/ci` only.

**Default:** leave as is.

**Your answer:**
