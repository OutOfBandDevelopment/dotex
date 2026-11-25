# Bake-off containers

A separate, throwaway stack for the desired-state database bake-off ([test plan](../../docs/design/DesiredStateDatabase/bake-off-test-plan.md), [result](../../docs/design/DesiredStateDatabase/bake-off-result.md), [spike](../../docs/patterns-discovery/08-spikes/03-schema-bake-off.md)). It shares nothing with [`containers/testing`](../testing/README.md): own compose project (`oobd-bakeoff`), network, container names and host ports, and no volumes, so `down` leaves nothing behind.

## Contents

| Service | Container | Host port | Purpose |
|---------|-----------|-----------|---------|
| `postgres` | `oobd-bakeoff-postgres` | 5440 (`BAKEOFF_PORT_POSTGRES`) | PostgreSQL 17.7, the target database |
| `postgres-dev` | `oobd-bakeoff-postgres-dev` | 5441 (`BAKEOFF_PORT_POSTGRES_DEV`) | Scratch database for tools that plan on a second one (pgschema `--plan-host`, Atlas `--dev-url`) |
| `sql-server` | `oobd-bakeoff-sqlserver` | 1440 (`BAKEOFF_PORT_SQLSERVER`) | SQL Server 2022 Developer |
| `tools` | `oobd-bakeoff-tools` | none | sqldef 3.11.30 (`sqlite3def`, `psqldef`, `mssqldef`, MIT) and pgschema 1.14.1 (Apache 2.0), built from pinned tags; `SHA256SUMS` in `/usr/local/bin` |
| `atlas` | run on demand (profile `run`) | none | Atlas community edition image; not started with the stack because the image has no shell |

The password is `BAKEOFF_PASSWORD` (default `BakeOff123!`, a throwaway value for a throwaway stack).

## Use

```bash
./scripts/bakeoff-up.sh        # build the tools image, start and wait, print versions and checksums
echo "create table t (id int primary key);" | docker exec -i -e PGPASSWORD='BakeOff123!' oobd-bakeoff-tools psqldef -h postgres -U postgres --dry-run <database>
docker compose -f docker-compose.bakeoff.yml run --rm atlas version
./scripts/bakeoff-down.sh
```

Inside the network the tools reach the engines by service name (`postgres`, `postgres-dev`, `sql-server`). From the host, use the ports above.

## Notes

- Atlas here is the community build (`v1.4.1` canary), not the vendor binary the earlier run used, so its S3, S9 and S10 results need a fresh run before the licence finding is revisited.
- The Go toolchain is pinned in `Dockerfile.tools` because sqldef 3.11 needs Go 1.26 or later.
