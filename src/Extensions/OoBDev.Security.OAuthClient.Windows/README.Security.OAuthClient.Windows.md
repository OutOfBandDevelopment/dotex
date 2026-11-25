# OoBDev.Security.OAuthClient.Windows

`DpapiSecretStore`: an `ISecretStore` that encrypts the signed-in tokens with Windows DPAPI (current user). Register it before `AddOAuthClient` so the file store is not used:

```csharp
services.AddSingleton<ISecretStore, DpapiSecretStore>();
services.AddOAuthClient(configuration);
```

Library: [OoBDev.Security.OAuthClient](../../Framework/OoBDev.Security.OAuthClient/README.Security.OAuthClient.md).
