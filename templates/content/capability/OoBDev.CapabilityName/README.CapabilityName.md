# OoBDev.CapabilityName

Default implementation of CapabilityName.

## Registration

```csharp
services.TryAddCapabilityNameServices(configuration, new CapabilityNameBuilder());
```

## Configuration

| Key | Purpose |
|-----|---------|
| `CapabilityNameOptions` | Options section (override with `CapabilityNameBuilder.OptionsSection`) |
| `OoBDev::ServiceKeys::OoBDev.CapabilityName.ICapabilityNameProvider` | Selects a keyed provider |

## Usage

Inject `ICapabilityNameManager` and call `RunAsync`.
