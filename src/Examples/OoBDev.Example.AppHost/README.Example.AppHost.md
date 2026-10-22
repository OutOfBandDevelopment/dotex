# OoBDev.Example.AppHost

Aspire host for the example only: the API, the SPA and the few backing services they use (Redis, RabbitMQ, SQL Server 2019 with the vector types, smtp4dev, Tika, Keycloak, Ollama). It is not the integration test stack.

Run: `dotnet run --project src/Examples/OoBDev.Example.AppHost` (the dashboard link is printed).

SQL Server runs the `2019-latest` image (the version `OoBDev.Data.Vectors.DB` targets, `Sql150`) with a data volume. When the `VectorDb` database is ready the host allows SQL CLR (`clr enabled`, `clr strict security` off; a development server only) and publishes `OoBDev.Data.Vectors.DB.dacpac` to it with DacFx, then the API gets the `VectorDb` connection string. The dacpac is built by the project reference; the host reads the Debug build output. The same deployment is covered by the `SqlServerDeploymentTests` Integration tests.

Reuses `containers/testing/ollama/Dockerfile`, the Keycloak realm import and the optional `containers/example/.env`.

Compose output: `dotnet run --project src/Examples/OoBDev.Example.AppHost -- --operation publish --publisher default --output-path <dir>` writes a `docker-compose.yaml` generated from this host (the image build step for the API may report an error; the compose file is still written).
