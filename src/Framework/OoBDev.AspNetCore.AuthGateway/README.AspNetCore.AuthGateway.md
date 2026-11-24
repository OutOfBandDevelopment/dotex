# OoBDev.AspNetCore.AuthGateway

Authentication translation for a YARP reverse proxy: callers present an API key or a client certificate, the target receives a JWT bearer token. `services.TryAddAuthGateway(configuration)`, then `app.MapReverseProxy(p => p.UseAuthGateway())`, and name the target in each route's metadata (`AuthGatewayTarget`). A route without a configured target is refused (403), a request without an accepted credential gets 401, a failing token endpoint gives 502. Tokens come from client credentials or RFC 8693 token exchange and are cached until 30 seconds before expiry. Configuration section `OoBDev:AuthGateway`; design and security notes: [AuthGateway](../../../docs/design/AuthGateway/README.md).

Tests: `OoBDev.AspNetCore.AuthGateway.Tests` (`Unit`).
