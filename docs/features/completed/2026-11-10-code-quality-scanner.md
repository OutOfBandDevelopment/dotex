# Feature request: a code quality scanner (new SonarQube or similar)

- **Status:** done (2026-11-16, [change document](../../changes/2026-11-16.md#sonarqube-community-in-the-integration-stack)); owner decision 2026-11-16: SonarQube Community in the integration stack with `dotnet-sonarscanner`, token in `SONAR_TOKEN`, `scripts/ci/sonar.ps1`, switchable gate
- **Requested by:** owner (2026-11-10)
- **Date:** 2026-11-10
- **Area:** build, `scripts/ci`, `.config/dotnet-tools.json`

## Need

The earlier SonarQube instance is gone and its script (`src/run-sonar.bat`) was deleted. Nothing scans the code for bugs, smells, security hotspots, duplication and coverage trends over time. The analyzers in `src/.editorconfig` give build-time findings only, with no history or dashboard.

## Proposed behavior

Stand up a scanner the owner can run locally (CI is manual only, see `scripts/ci`): SonarQube Community in a Docker container, or a lighter alternative, fed by `dotnet test` coverage and the build. Results are viewable in a dashboard, and a script in `scripts/ci` runs the scan end to end.

Candidates to compare in a short evaluation:

- SonarQube Community Build (container; the previous choice; needs a database).
- SonarAnalyzer.CSharp only (already in the Roslyn family; no server, no history).
- Qodana or Semgrep (container or CLI; different rule sets).
- The existing Aspire dashboard stays for telemetry and is not a candidate.

## Acceptance criteria

- Given a clean checkout and Docker, when the scan script runs, then a report with findings and coverage is produced and viewable locally.
- Given a scan with a new bug-level finding, then the script reports it and exits non-zero (a gate that can be switched on).
- Given the token or password, then it comes from an environment variable or user secret and is never written to a file in the repository.

## Existing code checked

- `.config/dotnet-tools.json` still pins `dotnet-sonarscanner` 11.0.0 and `.gitignore` ignores `.sonarqube/`; no script or container uses them.
- `containers/testing` holds the Docker service pattern (see the `add-integration-service` skill); `scripts/ci` holds the manual CI scripts.
- The roadmap lists SonarQube at 25% ("had working locally, requires a deployed instance").

## Security and operations

- Token handling: a token was once committed in `src/run-sonar.bat`. The instance is gone, so it is moot, but the new setup must read secrets from the environment only.
- The server is local and bound to loopback unless the owner decides otherwise.
- Disk and memory cost of a SonarQube container (it needs a database and several GB of RAM).

## Priority

- **Impact:** quality history and security hotspots beyond build analyzers.
- **Needed by:** no date
- **Workaround today:** analyzers at build time and `dotnet test` coverage reports.

## Open questions

- SonarQube again (known, heavy) or a lighter scanner? Recommended: evaluate SonarQube Community in a container first, since the tool and coverage format are already in place.
- Keep `dotnet-sonarscanner` in the tool manifest until this is decided?

## Triage notes

Replaces the stale Sonar script and the owner question about its token.

2026-11-11: triaged. Accepted as tooling work through deterministic scripts or MSTest entry points ([GeneratedGuides](../../design/GeneratedGuides/README.md)); scheduled behind the active work. Backlog: BACKLOG.md, Tooling requests.
