# Integration suite timing

Measured 2026-10-29 with `scripts/ci/run-tests.ps1 -Category Integration -Configuration Debug` (all green): 7 min 54 s end to end including the stack start and the build. The goal is under 5 minutes.

The no-flaky-tests goal is met: ten consecutive full Integration runs passed on 2026-10-29 (the first plus nine repeats of 6 to 8 minutes each, 20 test projects green every time).

## Where the time goes

- The test projects run in parallel and total about 17 minutes of test time.
- The ONNX model tests lead: DINOv2 batch 102 s; CLIP, ViT, Nomic and MPNet each 30 to 60 s of CPU inference. The Hugging Face cache on the runner has had every model since 2026-10-08, so downloads are not in these numbers; a clean machine adds them.
- The SQL vector tests follow (69 s, 29 s, 23 s).
- The ONNX conformance class alone takes 42 s, so most of the wall time is the projects competing for the same CPU cores.

## Options

- Caching models will not help on a warm runner.
- Trim repeated work, for example the 18-image conformance set run for each preset and each test in `OoBDev.Onnx.ImageEmbeddings.Tests`.
- Give the runner more cores; this helps more than trimming one class.

## Run on 2026-11-10

`scripts/ci/run-tests.ps1 -Category Integration -Configuration Debug` took 4 min 58 s end to end (stack already up, build included), inside the 5 minute goal. Four tests failed under that load and passed when their project was rerun alone against the same stack: `RabbitMqIntegrationTests` AMQPS and STOMP-over-TLS (5 to 6 s timeouts in `OoBDev.System.IO.Brokers.Tests`), `RealRabbitMq_Stomp_RoutesBothWays` (`OoBDev.DevTerm.Core.Tests`, 11 s wait) and one SQL vector test whose initializer failed to load `Microsoft.Identity.Client` (`OoBDev.Data.Vectors.DB.Tests`). The first three were timeouts too tight for a loaded machine (the test adapters now use 30 s, the DevTerm wait 30 s); the fourth is a DacFx assembly-load problem that did not reproduce alone and stays in `BACKLOG.md`.

## Run on 2026-11-11

`dotnet test OoBDev.slnx --settings .runsettings --filter TestCategory=Integration` (stack already up, Keycloak and the Aspire dashboard on overridden ports, build included) took 6 min 51 s and all 34 test projects passed with no failure. The suite has grown since 2026-11-10 (event sourcing, Azure blob provider tests), so the 5 minute goal is not met; the earlier trimming options (the repeated ONNX conformance set, more cores) still apply.

## Audit 2026-11-12

The stack was not running and the goal is CPU-bound with host load, so no change was made. The `Microsoft.Identity.Client` load failure in `OoBDev.Data.Vectors.DB.Tests` did not recur in the 2026-11-11 run. That run passed once the host ports another project held (8081, 8080, 18888) were overridden with `TEST_PORT_KEYCLOAK`, `TEST_PORT_NGINX` and `TEST_PORT_OTEL_DASHBOARD` plus matching `-- TestRunParameters` URLs. Measure the suite again the next time the stack is up.

## Warm-up script (2026-11-19)

The 2026-11-19 per-test timing showed the longest tests were the first test of a project waiting for its container (pgvector 2 min 23 s, SQL Server 2 min 22 s). `scripts/ci/warm-up-integration.ps1` pulls the images and runs `docker compose up -d` (no `--wait`) for the slow services before the build, so they initialise while the solution compiles; the harness then finds them healthy and the tests do not stop them. `scripts/ci/run-tests.ps1 -Category Integration` already starts the whole stack with `--wait` before the build, so it needs no warm-up. The script is verified with `-DryRun` (service names checked against the compose file); measured 2026-11-19: 5 min 3 s for the full run with the warm-up against about 7 min 50 s without; the Service Bus emulator is left out because it fails against a SQL Server volume that an earlier emulator run already initialised (`Cannot create file SbGatewayDatabase.mdf`): remove `oobd-test-sqlserver-data` to reset it.

## Repeat run (2026-11-19)

A second full Integration run with the stack already warm (`--no-build`, after the warm-up script) took 3 min 41 s. Two tests failed, `RealRabbitMq_Stomp_RoutesBothWays` (DevTerm.Core.Tests, "The STOMP broker closed the connection") and `Recognize_RealDoclingServe_ReturnsTheText` (Docling.Tests), each in under a second; both pass when their project runs alone, so the failure is tied to the parallel full run (probably their containers, RabbitMQ and Docling, are not up when the test starts), not to the code.

Cause and fix: both containers pass their health check before the service answers its first request (the STOMP listener of RabbitMQ, the first Docling call), and the harness had started them on demand a moment earlier. `RabbitMQ` and `Docling` are now in the warm-up defaults; the full Integration run after the warm-up had no failing test. A test started against a container the harness has just brought up can still hit this window (tracked in BACKLOG).
