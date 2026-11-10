# Feature request: spatial services (address geocoding with Census, Google Maps and Bing Maps)

- **Status:** new
- **Requested by:** owner (SharedFramework comparison, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `src/Framework` (abstractions) and `src/ExternalServices/<Vendor>` (adapters)

## Need

Turn an address into a global position and back, with a quality rating, through one contract and swappable vendors. dotex has nothing for location services or geocoding.

## Proposed behavior

A `SpatialServices` abstraction (address, address result, global position, result quality, location services) and one adapter per vendor behind it: US Census geocoder, Google Maps and Bing Maps. Each adapter references Abstractions only, follows the provider and factory pattern and is selected by configuration.

## Source projects

Under `C:\repo\mine\proving-grounds\examples\SharedFramework` (read-only reference):

- `OoBDev.SpatialServices.Abstractions` (`IAddress`, `IAddressResult`, `IGlobalPosition`, `ILocationServices`, `ResultQuality`)
- `OoBDev.SpatialServices.Common` (registrar, service collection extensions)
- `OoBDev.Census.Geocoding` (20 files: client, map, models)
- `OoBDev.Google.Maps` (9 files)
- `OoBDev.Microsoft.BingMaps` (11 files, REST client)
- Matching `.Tests` projects.

## Acceptance criteria

- Given an address and a configured vendor, then the service returns a position and a quality rating.
- Given a vendor error or no match, then the result says so and does not throw for a normal miss.
- Given the live vendors, then tests are `LiveIntegration` or `DevLocal`; unit tests use recorded responses.

## Existing code checked

Searched `src` for `ILocationServices`, `IGlobalPosition`, geocoding and the vendor URLs: no matches. The idea appears only as "Epic 3: Spatial Services" in [SharedFramework](../../design/SharedFramework/EPIC_REVIEW.md); no design set exists.

## Security and operations

API keys come from configuration or user secrets, never from files in the repository. Addresses are personal data: log without them. Vendor terms limit caching and rate; respect them.

## Priority

- **Impact:** a reusable capability for any application that handles addresses.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Which vendors are still wanted (Bing Maps is being retired by Microsoft)? Is there a consumer, or does this wait as a design (the SharedFramework epics wait for one, Q1 of 2026-11-06)?

## Triage notes

Origin: SharedFramework (the application framework dotex grew from). No design documents yet, so the design comes first (`docs/patterns-discovery/06-design-document-standard.md`).
