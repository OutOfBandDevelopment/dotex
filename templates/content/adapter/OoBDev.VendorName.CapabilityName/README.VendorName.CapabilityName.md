# OoBDev.VendorName.CapabilityName

VendorName provider for CapabilityName.

## Registration

```csharp
services.TryAddVendorNameCapabilityNameServices(configuration);
```

Registers nothing unless `VendorNameOptions:AccountId` is configured.

## Configuration

| Key | Purpose |
|-----|---------|
| `VendorNameOptions:AccountId` | Enables the adapter |
| `OoBDev:CapabilityName:Provider` = `vendor-name` | Selects this provider when several are registered |

Document new keys in `CONFIGURATION_SETTINGS.md`.
