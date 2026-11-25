# OoBDev.DesiredState.Cli

`desired-state plan`, `apply` and `pack`: the command line over [`OoBDev.Data.DesiredState`](../../Framework/OoBDev.Data.DesiredState/README.Data.DesiredState.md). A folder of `CREATE` scripts is the desired state; the tool shows or applies the difference for PostgreSQL, SQL Server or SQLite through sqldef (`psqldef`, `mssqldef`, `sqlite3def` on `PATH`).

```bash
desired-state plan  --engine postgres --scripts db/schema --host localhost --port 5432 --database app --user me
desired-state apply --engine sqlite   --scripts db/schema --database app.db
desired-state pack    --source db/package --name app --version 1.0 --out app.zip   # schema/, predeploy/, postdeploy/, refactorlog.json
```

- The password comes from the environment variable named by `--password-env` (default `DESIRED_STATE_PASSWORD`), never from the command line.
- `apply` refuses drops (`--allow-data-loss true`) and column type changes (`--allow-type-change true`) unless you pass the flag; it runs nothing when it refuses.
- Exit codes: 0 done or nothing to do, 1 refused or hazards in the plan, 2 error, 3 plan has pending safe changes.
- Oracle is not supported (rejected by the owner, 2026-11-24).
