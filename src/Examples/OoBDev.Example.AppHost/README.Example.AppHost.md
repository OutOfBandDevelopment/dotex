# OoBDev.Example.AppHost

Aspire host for the example only: the API, the SPA and the few backing services they use (Redis, RabbitMQ, smtp4dev, Tika, Keycloak, Ollama). It is not the integration test stack.

Run: `dotnet run --project src/Examples/OoBDev.Example.AppHost` (the dashboard link is printed).

Reuses `containers/testing/ollama/Dockerfile`, the Keycloak realm import and the optional `containers/example/.env`.

Compose output: `dotnet run --project src/Examples/OoBDev.Example.AppHost -- --operation publish --publisher default --output-path <dir>` writes a `docker-compose.yaml` generated from this host (the image build step for the API may report an error; the compose file is still written).
