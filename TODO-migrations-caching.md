# Migration TODO - Caching

**Projects:** 4 projects (Abstractions, Common, Redis, Microsoft)
**Source:** Incoming/SharedFramework/
**Status:** ✅ SAFE - Main has ZERO caching, no conflicts
**Priority:** HIGH

---

## Tasks

### Phase 1: Caching.Abstractions (NEW)
- [ ] Create `src/Framework/OoBDev.Caching.Abstractions/`
- [ ] Copy contracts from SF (~60 LOC)
- [ ] Update namespace to `OoBDev.Caching`
- [ ] Add project to solution
- [ ] Build and verify

### Phase 2: Caching.Common (290 LOC - NEW)
- [ ] Create `src/Framework/OoBDev.Caching/`
- [ ] Copy common abstractions, factories, managers
- [ ] Reference Abstractions project
- [ ] Add ServiceCollectionExtensions
- [ ] Create README
- [ ] Add to solution

### Phase 3: Redis.Caching (137 LOC - NEW)
- [ ] Create `src/ExternalServices/Redis/OoBDev.Redis.Caching/`
- [ ] Copy Redis provider implementation
- [ ] Add `StackExchange.Redis` NuGet package
- [ ] Reference Caching.Abstractions
- [ ] Add ServiceCollectionExtensions with TryAddRedisCache()
- [ ] Create README with connection string setup
- [ ] Add to solution

### Phase 4: Microsoft.Caching (97 LOC - NEW)
- [ ] Create `src/ExternalServices/Microsoft/OoBDev.Microsoft.Caching/`
- [ ] Copy Microsoft distributed cache provider
- [ ] Add `Microsoft.Extensions.Caching.Memory` package
- [ ] Add `Microsoft.Extensions.Caching.Distributed` package
- [ ] Reference Caching.Abstractions
- [ ] Add ServiceCollectionExtensions
- [ ] Create README
- [ ] Add to solution

### Phase 5: Testing
- [ ] Migrate Caching.Common.Tests (3 projects total in SF)
- [ ] Migrate Redis.Caching.Tests
- [ ] Add integration tests for Redis (use docker)
- [ ] Test Microsoft caching providers
- [ ] Target 80%+ coverage

### Phase 6: Documentation
- [ ] Create comprehensive caching architecture docs
- [ ] Document provider pattern
- [ ] Add usage examples for each provider
- [ ] Document configuration options
- [ ] Add to architectural patterns catalog

### Phase 7: Integration
- [ ] Add to Docker test infrastructure if needed
- [ ] Update ServiceCollection patterns documentation
- [ ] Build entire solution
- [ ] Run all tests
- [ ] Update TODO.md

---

## Project Structure

```
src/
├── Framework/
│   ├── OoBDev.Caching.Abstractions/     # NEW - Interfaces
│   ├── OoBDev.Caching/                  # NEW - Common implementation
│   └── OoBDev.Caching.Tests/            # NEW - Tests
└── ExternalServices/
    ├── Redis/
    │   ├── OoBDev.Redis.Caching/        # NEW - Redis provider
    │   └── OoBDev.Redis.Caching.Tests/  # NEW - Tests
    └── Microsoft/
        ├── OoBDev.Microsoft.Caching/    # NEW - MS provider
        └── OoBDev.Microsoft.Caching.Tests/ # NEW - Tests
```

---

## Key Features

- ✅ Distributed cache abstraction
- ✅ Redis integration (StackExchange.Redis)
- ✅ Microsoft in-memory and distributed caching
- ✅ Cache factory pattern
- ✅ Configuration options
- ✅ DI integration

---

## LOC Summary

- Abstractions: ~60 LOC
- Common: 290 LOC
- Redis: 137 LOC
- Microsoft: 97 LOC
- **Total:** ~600 LOC

---

**Effort:** 2-3 days
**Risk:** LOW - No conflicts, straightforward migration
