# Applications

Documentation that belongs to one application: requirements, architecture, API design and testing strategy as living documents (kept current in the same change as the code; no decision records), plus its screen specs, user guide and bench reports. Reusable capability documents stay in `docs/design/<Feature>/`, protocol references in `docs/protocols/`.

**Table 1 — Applications and their documents**

| Application | Projects | Documents |
|-------------|----------|-----------|
| DevTerm | `src/Applications/DevTerm/OoBDev.DevTerm.*` | [devterm](devterm/README.md) |
| ser2net-win | `src/Applications/Ser2NetWin/OoBDev.Ser2NetWin.Cli` (bridge library `src/Framework/OoBDev.System.IO.Ports.Bridge`) | [ser2netwin](ser2netwin/README.md) |
| Example application | `src/Examples/OoBDev.Example.Web`, `OoBDev.Example.WebApi`, `OoBDev.Example.AppHost` | [example](example/README.md) |
| Command-line tools | `src/Tools` | [tools](tools/README.md) (generated guides, not edited by hand) |

Migrated applications keep their imported history under their own folder until the final documentation cleanup (see [the migration plan](../migration/README.md)).
