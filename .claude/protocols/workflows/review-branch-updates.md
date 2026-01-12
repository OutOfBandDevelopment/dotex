# Review Branch Updates Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Review changes after pulling latest from branches and update documentation

---

## Overview

This protocol defines how to review changes after pulling latest from branches, verify implementations, and update documentation.

> **IMPORTANT**: Always check planned features against PR changes and clean up completed/moved files. This step is critical and must not be skipped.

---

## When to Use

- After pulling latest changes from branches
- After merging PRs
- When reviewing what's new in the codebase

---

## Trigger Phrases

- "pulled in latest"
- "pulled latest changes"
- "review the changes"
- "PR {number} was merged"

---

## Inputs Required

1. **Branch/PR Info**: Which branches were updated (API, UI, or both)
2. **PR Numbers** (optional): Specific PRs to review

---

## Procedure

### Phase 1: Identify Changes

#### 1.1 Get Recent Commits
```bash
# For main repository
git log --oneline -10

# For submodule (if applicable)
cd {submodule} && git log --oneline -10
```

#### 1.2 Review Commit Details
For each relevant commit:
```bash
git show <commit-hash> --stat
git show <commit-hash> -- "*.{extension}"
```

#### 1.3 Categorize Changes
Group changes by type:
- **New Controllers**: New API endpoints
- **New Models**: Query/Save models, DTOs
- **Model Extensions**: New fields on existing models
- **New Repositories**: Data access additions
- **UI Pages**: New or updated pages
- **UI Components**: Shared component changes

### Phase 2: Verify Frontend/Backend Alignment

#### 2.1 For Each New Controller
Verify frontend has matching:
- API client
- Schema definitions
- TypeScript models

#### 2.2 For Each Model Extension
Verify:
- Backend property exists
- Frontend schema has matching field
- TypeScript interface includes the field

#### 2.3 Verify Data Mappings
For new models, compare:
```
Backend Field           →  Frontend Schema     →  TypeScript Type
string? Name            →  z.string().optional() →  string | undefined
DateTime? DateCreated   →  z.string().optional() →  string | undefined
int Id                  →  z.number()            →  number
bool IsActive           →  z.boolean()           →  boolean
```

#### 2.4 Check Metadata Attributes
Verify OpenAPI extensions propagated:
- Navigation attributes
- Labels
- Hidden fields

### Phase 3: Update Documentation

#### 3.1 Update coverage.md
- Update version numbers (commit hashes)
- Update coverage statistics
- Mark newly implemented features with ✅
- Update tables with new endpoints

#### 3.2 Update gap-analysis.md
- Update statistics
- Add "Recent Updates" section
- Update feature tables
- Update "Files Changed Summary" section

#### 3.3 Create Implemented Feature Docs
For each new feature, create `docs/features/implemented/{feature}.md`:

```markdown
# Implemented: {Feature Name}

**Status**: Implemented
**PR**: #{number}
**Date**: {date}

## Summary
{Brief description}

## Endpoints Implemented
- `POST /api/{controller}/{action}`

## Models Created
| File       | Purpose    |
|------------|------------|
| `{Model}`  | {purpose}  |

## Frontend Integration
- API Client: `{client}`
- Schema: `{schema}`
- UI Page: `{page}/` (if applicable)

## Verification
{How to verify it works}
```

#### 3.4 Clean Up Planned Features (CRITICAL)

**This step must not be skipped.**

1. **List all planned features**:
```bash
ls -la docs/features/planned/*.md
```

2. **For each planned feature, check against PR changes**:
   - Was the feature implemented in this PR?
   - Was it partially implemented?
   - Are there any broken references?

3. **For fully completed features**:
   - Move file from `planned/` to `implemented/`
   - Update status in the file header
   - Update `planned/README.md` to remove reference
   - Add to `implemented/` section

4. **For partially completed features**:
   - Update status (e.g., "Backend Ready - Frontend Only Needed")
   - Document what was completed and what remains
   - Fix any broken cross-references

5. **Fix broken references** after file moves:
```bash
grep -r "planned/{moved-file}" docs/
```

6. **Verify planned/README.md** reflects current state:
   - Only lists features still pending
   - Links point to correct locations
   - "Recently Implemented" section updated

#### 3.5 Clean Up All Cross-References
Search and update all cross-references:
```bash
grep -r "planned/{moved-file}" docs/
```

### Phase 4: Validate Documentation Structure

#### 4.1 Check File Organization
```
docs/features/
├── README.md
├── coverage.md
├── gap-analysis.md
├── implemented/
│   └── {completed-features}.md
├── planned/
│   ├── api/
│   ├── ui/
│   └── {full-stack}.md
└── decisions/
```

#### 4.2 Verify Links
Check all markdown links resolve:
- Internal doc references
- Cross-folder references
- External references

### Phase 5: Output Summary

#### 5.1 Report Summary
```markdown
## Branch Update Review Complete

### PRs Reviewed
- PR #{number}: {summary}

### New Implementations
- ✅ {feature 1}
- ✅ {feature 2}

### Documentation Updated
- coverage.md - Updated to X% coverage
- gap-analysis.md - Added PR section
- Created X new implemented/*.md files
- Moved X completed planned/*.md to implemented/

### Remaining Gaps
- {feature} (backend ready, UI needed)
- {feature} (priority P1)
```

---

## Validation Checklist

**Frontend/Backend Alignment:**
- [ ] All new controllers have frontend clients
- [ ] All model fields match between backend and frontend
- [ ] Metadata attributes propagated to OpenAPI

**Documentation Updates:**
- [ ] coverage.md updated with new version numbers
- [ ] gap-analysis.md reflects current state
- [ ] New implemented/*.md files created
- [ ] Planned features cleanup complete

**Planned Features Cleanup (CRITICAL):**
- [ ] Listed all planned/*.md files
- [ ] Checked each against PR changes
- [ ] Completed features moved to implemented/
- [ ] Partial features updated with status
- [ ] All broken references fixed
- [ ] planned/README.md current

**Structure Validation:**
- [ ] All markdown links resolve
- [ ] README.md files current
- [ ] No orphaned documentation

---

## Notes

- Always verify data type mappings
- Check for nullable differences
- Metadata attributes are critical for code generation
- Keep implemented/ and planned/ folders in sync
- Update coverage percentages based on actual counts

---

## Related Protocols

- [Start Work](./start-work.md) - Beginning work on items
- [Change Review](./change-review.md) - Reviewing changes
- [Compare Documentation to Project](../software/compare-documentation-to-project.md) - Gap analysis

---

## Change Log

- 2026-01-03 v1.0: Initial generalized version
