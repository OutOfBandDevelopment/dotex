# Migration TODO - Documents / DocumentCenter

**Projects:** DocumentCenter (911 LOC), DocumentCenter.Abstractions (422 LOC)
**Source:** Incoming/SharedFramework/
**Status:** ⚠️ FEATURE MERGE - Main has 531 LOC partial, SF adds packaging/storage
**Priority:** MEDIUM

---

## Tasks

### Phase 1: Analysis & Comparison
- [ ] Compare main's Documents with SF's DocumentCenter
- [ ] Identify overlapping features (conversion)
- [ ] Identify unique SF features (packaging, resolvers, storage)
- [ ] Identify unique main features (Azure Blob containers)
- [ ] Decide: Merge into Documents or keep DocumentCenter separate?
- [ ] Recommendation: Merge into existing Documents

### Phase 2: Documents.Abstractions Comparison
- [ ] Compare main's Documents.Abstractions (~700 LOC) with SF's DocumentCenter.Abstractions (422 LOC)
- [ ] Identify overlaps and gaps
- [ ] Design merge strategy
- [ ] Determine which abstractions to keep/merge

### Phase 3: Add Packaging Features (NEW from SF)
- [ ] Add Packaging/ folder to main Documents
- [ ] Copy packaging system from SF DocumentCenter
- [ ] Update namespace to OoBDev.Documents
- [ ] Integrate with existing Documents framework
- [ ] Add tests

### Phase 4: Add Resolvers Features (NEW from SF)
- [ ] Add Resolvers/ folder to main Documents
- [ ] Copy document resolvers from SF
- [ ] Update namespace
- [ ] Integrate with Documents framework
- [ ] Add tests

### Phase 5: Storage Abstraction Merge
- [ ] Compare main's Containers/ (Azure Blob specific) with SF's Storage/
- [ ] Evaluate: Replace Azure-specific with generic abstraction?
- [ ] Options:
  - A) Keep both (Azure as one provider, add generic abstraction)
  - B) Migrate to generic storage provider pattern
- [ ] Recommended: Option A - Keep Azure, add generic abstraction
- [ ] Copy SF's Storage/ as generic provider abstraction
- [ ] Adapt existing Azure Blob to new abstraction if chosen

### Phase 6: Conversion Reconciliation
- [ ] Compare Conversion/ implementations
- [ ] Merge unique features from both
- [ ] Keep best implementation
- [ ] Ensure compatibility with Apache Tika, WkHtmlToPdf

### Phase 7: Testing
- [ ] Migrate DocumentCenter tests
- [ ] Update existing Documents tests
- [ ] Test packaging system
- [ ] Test resolvers
- [ ] Test storage providers (Azure + generic)
- [ ] Test conversion features
- [ ] Target 80%+ coverage

### Phase 8: Documentation
- [ ] Document enhanced Documents architecture
- [ ] Document packaging system
- [ ] Document resolver pattern
- [ ] Document storage abstraction
- [ ] Add usage examples for each feature
- [ ] Create migration guide

### Phase 9: Integration
- [ ] Verify compatibility with Apache Tika
- [ ] Verify compatibility with WkHtmlToPdf
- [ ] Verify compatibility with HtmlToOpenXml
- [ ] Build entire solution
- [ ] Run all tests
- [ ] Update TODO.md

---

## Project Structure

```
src/Framework/
└── OoBDev.Documents/                # ENHANCED
    ├── Containers/                  # Keep - Azure Blob (531 LOC existing)
    ├── Conversion/                  # MERGE - Reconcile both implementations
    ├── Packaging/                   # NEW from SF - Document packaging
    ├── Resolvers/                   # NEW from SF - Document resolution
    └── Storage/                     # NEW from SF - Generic storage abstraction
```

---

## Feature Comparison

| Feature | Main Documents | SF DocumentCenter | Action |
|---------|---------------|-------------------|--------|
| **Conversion** | ✅ Yes | ✅ Yes | Merge/reconcile |
| **Azure Blob** | ✅ Containers/ | ❌ None | Keep |
| **Generic Storage** | ❌ None | ✅ Storage/ | Add |
| **Packaging** | ❌ None | ✅ Yes | Add |
| **Resolvers** | ❌ None | ✅ Yes | Add |

---

## Integration Points

**Existing in Main:**
- ✅ OoBDev.Documents.Abstractions (~700 LOC)
- ✅ OoBDev.Documents (531 LOC)
- ✅ OoBDev.Apache.Tika (document conversion)
- ✅ OoBDev.WkHtmlToPdf (PDF generation)
- ✅ OoBDev.HtmlToOpenXml (Word generation)

**Adding from SF:**
- ➕ Document packaging system
- ➕ Document resolvers
- ➕ Generic storage provider abstraction

---

## Decision: Namespace

**Recommendation:** Keep `OoBDev.Documents` namespace
- DocumentCenter → merge into Documents
- Maintains consistency with main
- Clear evolution of existing feature

---

## LOC Summary

- Main Documents: 531 LOC
- Main Documents.Abstractions: ~700 LOC
- SF DocumentCenter: 911 LOC
- SF DocumentCenter.Abstractions: 422 LOC
- **Net Addition:** ~400 LOC (packaging, resolvers, storage abstraction)

---

**Effort:** 3-4 days
**Risk:** MEDIUM - Requires careful feature reconciliation
