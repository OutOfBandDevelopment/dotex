# CgiMiddleware design reconciled with code

**Date:** 2026-10-22 · **Compared:** `docs/design/CgiMiddleware` against `src/Extensions/OoBDev.AspNetCore.Cgi` and its tests

## Result

- Requirements REQ-001 to REQ-017 match the code (options, limits, rights check, 401/403/413/502/503/504 behavior); no requirement was removed.
- The testing strategy listed tests that do not exist. Table 4 now names the real test for each requirement and marks the rest Planned; the missing tests are one BACKLOG item (path split, echo and header fixtures, shell metacharacters, symlink, child kill, stderr log, static root, environment clearing).
- The rights check is still inside the middleware; moving it onto `IApplicationRightChecker` is in BACKLOG ([application rights](apis-application-rights-2026-10-22.md)).

## Not done

No code or tests were run for this change; the other designs under `docs/design/` are still to be compared (BACKLOG).
