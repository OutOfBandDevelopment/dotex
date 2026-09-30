# Architecture § 5 — Composition Root

[↑ Architecture](./README.md) · [← 4. The Common Layer Is an Aggregator (Convention over Reference)](./04-common-layer-aggregator.md) · [6. Runtime Composition Model (How a Call Finds Its Implementation) →](./06-runtime-composition-model.md)

There is exactly one composition root per application. In `Examples/OoBDev.Example.WebApi/Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
services.AddApplicationServices();                     // app-specific registrations (TryAdd…)
services.TryAllCommonExtensions(builder.Configuration,
    systemBuilder:   new() { },
    aspNetBuilder:   new() { RequireAuthenticatedByDefault = identityProvider != None },
    jwtBuilder:      new() { JwtBearerConfigurationSection = authProvider + nameof(JwtBearerOptions) },
    identityBuilder: new() { IdentityProvider = identityProvider },
    externalBuilder: new() { },
    hostingBuilder:  new() { DisableMailKit = true });
...
app.UseAllCommonMiddleware(new() { });
app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
```

Characteristics worth carrying forward:

1. **Environment selects behavior through configuration, not through code branches** (`IDENTITY_PROVIDER`, `SWAGGER_ONLY`; config-section-name prefixes like `Keycloak:JwtBearerOptions`).
2. **The application owns almost no infrastructure code.** Controllers depend on small application-level providers (`IExampleMessageProvider`), which depend on Framework interfaces (`IMessageQueueSender<T>`).
3. **Middleware ordering stays in the app** (`UseAuthentication` / `UseAuthorization` are *not* hidden in the roll-up), while middleware *content* comes from Common.

---

[↑ Architecture](./README.md) · [← 4. The Common Layer Is an Aggregator (Convention over Reference)](./04-common-layer-aggregator.md) · [6. Runtime Composition Model (How a Call Finds Its Implementation) →](./06-runtime-composition-model.md)
