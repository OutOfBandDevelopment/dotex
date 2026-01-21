# Migration TODO - Spatial Services

**Projects:** 5 projects (Abstractions, Common, Census, Google Maps, Bing Maps)
**Source:** Incoming/SharedFramework/
**Status:** ✅ SAFE - Main has ZERO spatial/geocoding capability
**Priority:** HIGH

---

## Tasks

### Phase 1: Spatial Services Abstractions (85 LOC - NEW)
- [ ] Create `src/Framework/OoBDev.SpatialServices.Abstractions/`
- [ ] Copy contracts: ILocationServices, address models
- [ ] Update namespace to `OoBDev.SpatialServices`
- [ ] Add to solution
- [ ] Create README

### Phase 2: Spatial Services Common (21 LOC - NEW)
- [ ] Create `src/Framework/OoBDev.SpatialServices/`
- [ ] Copy utility implementations
- [ ] Reference Abstractions
- [ ] Add ServiceCollectionExtensions
- [ ] Add to solution

### Phase 3: Census Geocoding (420 LOC - NEW)
- [ ] Create `src/ExternalServices/Census/OoBDev.Census.Geocoding/`
- [ ] Copy geocoding implementation
- [ ] Update namespace to `OoBDev.Census.Geocoding`
- [ ] Implement ILocationServices interface
- [ ] Add ServiceCollectionExtensions
- [ ] Create README (note: free, no API key required)
- [ ] Add to solution

### Phase 4: Google Maps (453 LOC - NEW)
- [ ] Create `src/ExternalServices/Google/OoBDev.Google.Maps/`
- [ ] Copy Maps API integration
- [ ] Update namespace to `OoBDev.Google.Maps`
- [ ] Add Google Maps NuGet packages
- [ ] Implement ILocationServices
- [ ] Add ServiceCollectionExtensions
- [ ] Create README with API key setup
- [ ] Add to solution

### Phase 5: Microsoft Bing Maps (257 LOC - NEW)
- [ ] Create `src/ExternalServices/Microsoft/OoBDev.Microsoft.BingMaps/`
- [ ] Copy Bing Maps implementation
- [ ] Update namespace to `OoBDev.Microsoft.BingMaps`
- [ ] Add Bing Maps NuGet packages
- [ ] Implement ILocationServices
- [ ] Add ServiceCollectionExtensions
- [ ] Create README with API key setup
- [ ] Add to solution

### Phase 6: Testing
- [ ] Migrate Census.Geocoding.Tests
- [ ] Migrate Google.Maps.Tests
- [ ] Migrate BingMaps.Tests
- [ ] Add integration tests (Census is free to test)
- [ ] Target 80%+ coverage

### Phase 7: Documentation
- [ ] Create spatial services architecture doc
- [ ] Document ILocationServices contract
- [ ] Add usage examples for each provider
- [ ] Create provider comparison table (features, cost, limits)
- [ ] Document address model standards

### Phase 8: Integration
- [ ] Build entire solution
- [ ] Run all tests
- [ ] Update TODO.md
- [ ] Add to architectural patterns

---

## Project Structure

```
src/
├── Framework/
│   ├── OoBDev.SpatialServices.Abstractions/  # NEW - Interfaces
│   ├── OoBDev.SpatialServices/               # NEW - Common utilities
│   └── OoBDev.SpatialServices.Tests/         # NEW - Tests
└── ExternalServices/
    ├── Census/
    │   ├── OoBDev.Census.Geocoding/          # NEW - Free geocoding
    │   └── OoBDev.Census.Geocoding.Tests/    # NEW - Tests
    ├── Google/
    │   ├── OoBDev.Google.Maps/               # NEW - Google provider
    │   └── OoBDev.Google.Maps.Tests/         # NEW - Tests
    └── Microsoft/
        ├── OoBDev.Microsoft.BingMaps/        # NEW - Bing provider
        └── OoBDev.Microsoft.BingMaps.Tests/  # NEW - Tests
```

---

## Key Features

- ✅ Geocoding (address → coordinates)
- ✅ Reverse geocoding (coordinates → address)
- ✅ Address standardization
- ✅ Multi-provider support with common interface
- ✅ Free option (Census) + commercial options (Google, Bing)

---

## Provider Comparison

| Provider | Cost | Features | Limits |
|----------|------|----------|--------|
| Census | Free | Basic geocoding | US addresses only |
| Google Maps | Paid | Advanced features, global | Rate limits, costs |
| Bing Maps | Paid | Enterprise features | Rate limits, costs |

---

## LOC Summary

- Abstractions: 85 LOC
- Common: 21 LOC
- Census: 420 LOC
- Google Maps: 453 LOC
- Bing Maps: 257 LOC
- **Total:** ~1,200 LOC

---

**Effort:** 2-3 days
**Risk:** LOW - Completely new capability, no conflicts
