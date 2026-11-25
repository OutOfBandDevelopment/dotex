# OoBDev.SpatialServices.Abstractions

Vendor-neutral geocoding contract: `ILocationServices` (`GeocodeAsync`, `ReverseGeocodeAsync`), `AddressQuery`, `GlobalPosition` (range-checked), `GeocodeResult` and `ResultQuality`. A vendor failure is `GeocodeResult.NoMatch` (quality `None`), never an exception. Adapters: [Census](../../ExternalServices/Census/OoBDev.Census.Geocoding/README.Census.Geocoding.md). Design: [SpatialServices](../../../docs/design/SpatialServices/README.md).
