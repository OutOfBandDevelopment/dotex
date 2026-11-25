#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
docker compose -f docker-compose.bakeoff.yml up -d --build --wait
docker exec oobd-bakeoff-tools sh -c 'cat /usr/local/bin/SHA256SUMS; sqlite3def --version; psqldef --version; mssqldef --version; pgschema --version' || true
docker compose -f docker-compose.bakeoff.yml run --rm atlas version || true
