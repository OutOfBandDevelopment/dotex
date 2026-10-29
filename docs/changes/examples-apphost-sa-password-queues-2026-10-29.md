# Example AppHost: fixed SQL password and queue/mail wiring (2026-10-29)

Running the example under Visual Studio (Aspire AppHost) failed on the SQL login and then on the email and queue journeys.

- **SQL login.** The `sa` password was generated, and the persisted data volume kept the first one, so a later run (new user secrets) got `Login failed for user 'sa'`. The host now uses a fixed `sql-password` parameter, the `SQL_SA_PASSWORD` default the compose stacks use (`IntegrationTest123!`; override with the environment variable). The data volume is renamed `oobd-aspire-sqlserver-fixed-sa` because the old one holds the old password; the old volume is untouched and can be removed with `docker volume rm oobd-aspire-sqlserver`.
- **Mail and queues.** The API read its SMTP and RabbitMQ host, port and credentials from its own sections, which only the compose file set. The host now sets `MailKitSmtpClientOptions__Host/Port` and `MessageQueue__<name>__Config__HostName/Port/UserName/Password` from the Aspire endpoints.
- **Verified live** (`E2E_BASE_URL` against the host's SPA): 15 of 20 journeys passed before the wiring and the email and queue journeys pass after it. Still failing live: the two Keycloak sign-in journeys (the host runs the API with `IDENTITY_PROVIDER=None`) and the telemetry journey (the host sets no dashboard API URL or key); both are in `BACKLOG.md`.
- The dacpac build (`DacpacTool`) crashed once with an access violation (-1073741819) and passed on the retry; not reproduced.
