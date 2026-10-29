# CGI symlink test (2026-10-29)

`Get_SymlinkOutOfTheRoot_IsForbidden` (Simulate, REQ-007) builds a temporary script root with a link to a script outside it, once as a file link and once as a directory link, and expects 403. It passes in the Linux SDK container (`scripts/ci/run-linux-tests.ps1`); on Windows it ends Inconclusive when the account lacks the link privilege. No defect found: the resolver already rejects both shapes.
