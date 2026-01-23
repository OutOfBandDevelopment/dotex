# Change Documentation Archival Protocol

**Version:** 1.0
**Last Updated:** 2026-01-20
**Purpose:** Archive completed work from active TODO files and CLAUDE.md to reduce context overhead while preserving historical detail

---

## When to Use This Protocol

### Trigger Phrases
- "clean up your task list"
- "archive completed work"
- "reduce context overhead"
- "move completed work to change documents"
- "clean up TODO files"

### When to Archive

**Archive when:**
- ✅ Work is marked as COMPLETE in TODO files
- ✅ Work has been verified (build passes, tests pass, etc.)
- ✅ Detailed implementation notes exist in TODO files or CLAUDE.md
- ✅ Work is no longer actively being modified
- ✅ TODO files are becoming too long (>400 lines is a good threshold)

**Do NOT archive when:**
- ❌ Work is still in progress
- ❌ Work is pending verification
- ❌ Work might need immediate reference (wait 24-48 hours after completion)
- ❌ Work is trivial (simple one-line fixes don't need change documents)

---

## Step-by-Step Process

### Step 1: Identify Completed Work to Archive

**Scan TODO files for:**
- Sections marked with ✅ COMPLETE or ✅ VERIFIED COMPLETE
- Sections in "Completed Work" or "Recently Completed Work" sections
- Work older than 24-48 hours (check dates)
- Detailed implementation notes, code examples, file lists

**Common patterns to look for:**
```markdown
### ✅ Feature Name (COMPLETED - YYYY-MM-DD)
**What was done:**
- Detailed point 1
- Detailed point 2
[... lots of detail ...]
```

**Files to check:**
- `TODO.md` - "Completed Work Summary" section
- `TODO-*.md` - All child TODO files (bug-fixes, migrations, testing, etc.)
- `CLAUDE.md` - "Recently Completed Work" or "Previous Completed Work" sections

### Step 2: Create Change Document

**Naming Convention:**
```
docs/changes/{epic}-{feature}-{YYYY-MM-DD}.md
```

**Epic values:**
- `bug-fixes` - Bug fixes and technical debt
- `testing` - Testing infrastructure and test migrations
- `migration` - Migration work (Incomming projects, BinaryDataDecoders, etc.)
- `feature` - New features
- `refactor` - Refactoring work
- `infrastructure` - CI/CD, build system, tooling
- `documentation` - Documentation updates

**Feature values:**
- Use kebab-case (lowercase with hyphens)
- Be specific: `swashbuckle-dotnet10` not just `fixes`
- Keep under 30 characters
- Examples: `docker-infrastructure`, `mstest-expected-exception`, `phase0-critical`

**Date format:**
- Use completion date: `YYYY-MM-DD`
- Example: `2026-01-20`

**Example filenames:**
```
bug-fixes-swashbuckle-dotnet10-2026-01-20.md
testing-docker-infrastructure-2026-01-19.md
migration-sharedframework-phase0-2026-01-15.md
feature-search-query-pagination-2026-02-01.md
```

### Step 3: Write Change Document

**Required sections:**

```markdown
# {Epic Title} - {Feature Title}

**Date:** YYYY-MM-DD
**Epic:** {Epic Name}
**Status:** ✅ COMPLETE [AND VERIFIED]
**Impact:** {Brief summary of files/projects affected}

---

## Summary

{2-3 sentence overview of what was done and why}

**Results:**
- ✅ Key outcome 1
- ✅ Key outcome 2
- ✅ Key outcome 3

---

## Detailed Changes

{Comprehensive details of all changes made}

### Subsection 1

{Details, code examples, before/after}

### Subsection 2

{More details}

---

## Verification

**Build Verification:**
```bash
{Commands run}
```
- ✅ Result 1
- ✅ Result 2

**Test Verification:**
```bash
{Commands run}
```
- ✅ Result 1
- ✅ Result 2

---

## Key Patterns (Optional)

{Reusable patterns for future reference}

---

## Impact Summary (Optional)

{Tables, statistics, file counts}

---

## References (Optional)

{Links to external documentation}

---

## Files Modified

{List of all files changed}

---

**Related Documentation:**
- [TODO.md](../../TODO.md) - Main project tracking
- [TODO-{epic}.md](../../TODO-{epic}.md) - Epic tracking
```

**Content guidelines:**
- **Include ALL details** from TODO files - code examples, commands, verification steps
- **Use markdown formatting** - code blocks, tables, lists, headings
- **Add context** - explain why changes were made, not just what
- **Show before/after** - code examples showing the problem and solution
- **Include commands** - exact build, test, verification commands used
- **Link related work** - reference other change documents if applicable

### Step 4: Update TODO Files

**For each section archived:**

**Replace detailed content with summary + link:**

```markdown
### ✅ {Feature Name} (YYYY-MM-DD)

**Summary:** {1-2 sentence summary}

**Impact:**
- Key point 1
- Key point 2
- Key point 3

**Details:** [docs/changes/{filename}.md](docs/changes/{filename}.md)
```

**Example:**
```markdown
### ✅ Swashbuckle 10.1.0 & .NET 10.0 Breaking Changes (2026-01-20)

**Summary:** Fixed all breaking changes from Swashbuckle 10.1.0 upgrade and .NET 10.0 migration + enabled XML documentation generation globally.

**Impact:**
- 5 files fixed (4 Swashbuckle, 1 .NET 10.0)
- 3 files for XML documentation
- All 65 projects build successfully
- Swagger Summary/Description properties now appear

**Details:** [docs/changes/bug-fixes-swashbuckle-dotnet10-2026-01-20.md](docs/changes/bug-fixes-swashbuckle-dotnet10-2026-01-20.md)
```

**What to keep in TODO files:**
- ✅ High-level summary (1-2 sentences)
- ✅ Key impact points (3-5 bullet points)
- ✅ Link to change document
- ✅ Status markers (✅ COMPLETE)
- ✅ Dates

**What to remove from TODO files:**
- ❌ Detailed code examples
- ❌ Line-by-line changes
- ❌ Build output
- ❌ Lengthy explanations
- ❌ Breaking change reference guides (move to change doc)

### Step 5: Update CLAUDE.md

**Move from "Current Work Context" to "Recently Completed Work":**

```markdown
## Recently Completed Work

### ✅ {Feature Name} (YYYY-MM-DD)

{1 sentence summary}

**Details:** [docs/changes/{filename}.md](docs/changes/{filename}.md)

---
```

**What to keep in CLAUDE.md:**
- ✅ Work completed in last 30 days (for context)
- ✅ One-sentence summary
- ✅ Link to change document

**What to remove from CLAUDE.md:**
- ❌ All detailed "What was done" sections
- ❌ Code examples
- ❌ File lists
- ❌ "Next Steps When Resuming" sections (work is complete)
- ❌ Work older than 30 days (keep in change docs only)

**Age-based cleanup:**
- **< 7 days old:** Keep in "Recently Completed Work"
- **7-30 days old:** Move to bottom of "Recently Completed Work"
- **> 30 days old:** Remove from CLAUDE.md entirely (only in change docs)

### Step 6: Update Change Document Index

**Update `docs/changes/README.md`:**

Add entry to "Current Change Documents" section under appropriate epic:

```markdown
### {Epic Name}

**[{filename}]({filename})**
- {One sentence summary}
- {Key impact point}
- Status: ✅ Complete [and verified]
```

**Sort by date:** Most recent first within each epic

### Step 7: Update TODO.md Reference Section

**Update "Change History" section in TODO.md:**

```markdown
### Change History
- `docs/changes/README.md` - Archived completed work (reduces context overhead)
- `docs/changes/{new-file}.md` - {Brief description}
- `docs/changes/{existing-file}.md` - {Brief description}
```

**Keep newest 5-10 entries:** Full list is in `docs/changes/README.md`

### Step 8: Verification

**After archiving, verify:**

- [ ] Change document created with all required sections
- [ ] Change document includes ALL details from TODO files
- [ ] TODO files updated with summary + link
- [ ] CLAUDE.md updated with summary + link
- [ ] `docs/changes/README.md` updated with new entry
- [ ] TODO.md Reference section updated
- [ ] All internal links work correctly
- [ ] Relative paths are correct (`../../TODO.md` from change docs)
- [ ] No broken references in archived sections

**Test links:**
```bash
# From change document, verify these work:
- [TODO.md](../../TODO.md)
- [TODO-{epic}.md](../../TODO-{epic}.md)

# From TODO files, verify these work:
- [docs/changes/{file}.md](docs/changes/{file}.md)
```

---

## Examples

### Example 1: Archiving Bug Fixes

**Before (TODO-bug-fixes.md):**
```markdown
### OoBDev.AspNetCore.Mvc - Swashbuckle 10.1.0 Breaking Changes (COMPLETED)

**Issue:** Assembly updates introduced breaking changes in Swashbuckle 10.1.0 API

**Changes Made (2026-01-15):**

**File: FormFileOperationFilter.cs**
- [x] Fixed CS0200: `IOpenApiSchema.Properties` is read-only
  - Changed from: `schema.Properties = new Dictionary<...>()`
  - Changed to: Loop through fileParams and add to `schema.Properties[propertyName]`
  - Location: Lines 34-43
[... 200+ more lines of detailed changes ...]

**Verification (2026-01-20):**
- [x] Build verified: All 65 projects built successfully
[... more verification details ...]
```

**After (TODO-bug-fixes.md):**
```markdown
### ✅ Swashbuckle 10.1.0 & .NET 10.0 Breaking Changes (2026-01-20)

**Summary:** Fixed all breaking changes from Swashbuckle 10.1.0 upgrade and .NET 10.0 migration + enabled XML documentation generation globally.

**Impact:**
- 5 files fixed (4 Swashbuckle, 1 .NET 10.0)
- 3 files for XML documentation
- All 65 projects build successfully
- Swagger Summary/Description properties now appear

**Details:** [docs/changes/bug-fixes-swashbuckle-dotnet10-2026-01-20.md](docs/changes/bug-fixes-swashbuckle-dotnet10-2026-01-20.md)
```

### Example 2: Archiving Testing Infrastructure

**Before (CLAUDE.md):**
```markdown
## Current Work Context (2026-01-19)

### Task: Docker-Based Integration Testing Infrastructure

**Status:** ✅ WEEK 1 & 2 COMPLETE - ⏳ AWAITING LOCAL DOCKER VALIDATION

**What's Completed:**

**Week 1 - Docker Infrastructure (✅ COMPLETE):**
- ✅ Complete Docker infrastructure in `/containers/testing/`
- ✅ 11 services configured with health checks
[... 100+ more lines ...]
```

**After (CLAUDE.md):**
```markdown
## Recently Completed Work

### ✅ Docker-Based Integration Testing Infrastructure (2026-01-19)

Implemented complete Docker-based testing infrastructure with 11 services, CI/CD pipeline, and migrated 19 tests from DevLocal to Integration category.

**Details:** [docs/changes/testing-docker-infrastructure-2026-01-19.md](docs/changes/testing-docker-infrastructure-2026-01-19.md)
```

---

## Context Reduction Metrics

**Target reductions:**
- TODO files: 30-50% reduction in line count
- CLAUDE.md: 20-40% reduction in "Completed Work" sections
- Overall: 200-500 lines moved to change documents

**Example actual results:**
- TODO-bug-fixes.md: 312 → 150 lines (-160 lines, -51%)
- CLAUDE.md: 646 → 550 lines (-96 lines, -15%)
- Total: 256 lines moved to 3 change documents (27KB preserved)

---

## Common Mistakes to Avoid

### ❌ Don't Do This

**1. Archiving too early:**
```markdown
### ✅ Feature (COMPLETED - today)
[Archives immediately]
```
**Why wrong:** Might need immediate reference if issues found

**2. Losing detail:**
```markdown
# Change Document
## Summary
Fixed some bugs.
```
**Why wrong:** No useful information preserved

**3. Breaking links:**
```markdown
[Details](changes/bug-fixes.md)  ❌ Wrong path
```
**Why wrong:** Relative paths must be correct from each file

**4. Incomplete archival:**
- Archived detailed content but forgot to update links
- Created change doc but didn't update README.md
- Updated TODO files but left detailed content in CLAUDE.md

**5. Archiving active work:**
```markdown
### Feature (IN PROGRESS)
[Archives it anyway]
```
**Why wrong:** Active work should stay in TODO files

### ✅ Do This Instead

**1. Wait 24-48 hours after completion**
**2. Include ALL details in change document**
**3. Use correct relative paths from each file**
**4. Complete all steps in the protocol**
**5. Only archive truly complete work**

---

## Template: Change Document

```markdown
# {Epic} - {Feature}

**Date:** YYYY-MM-DD
**Epic:** {Epic Name}
**Status:** ✅ COMPLETE [AND VERIFIED]
**Impact:** {Brief summary}

---

## Summary

{2-3 sentences}

**Results:**
- ✅ {Key outcome 1}
- ✅ {Key outcome 2}

---

## Detailed Changes

{Comprehensive details with code examples}

---

## Verification

**Build Verification:**
```bash
{commands}
```
- ✅ {Result}

---

## Files Modified

{List of files}

---

**Related Documentation:**
- [TODO.md](../../TODO.md)
- [TODO-{epic}.md](../../TODO-{epic}.md)
```

---

## Integration with Other Protocols

**Related Protocols:**
- **documentation-standards.md** - File organization and structure
- **documentation-style-guide.md** - Writing style and formatting
- Use change documentation archival AFTER completing work documented by other protocols

**Workflow Integration:**
1. Complete work following relevant protocol
2. Update TODO files with completion status
3. **Wait 24-48 hours**
4. Run this protocol to archive to change document
5. Reduce context overhead in active files

---

## Version History

**v1.0 (2026-01-20):**
- Initial protocol creation
- Based on successful archival of 3 bug fix change documents
- Saved 256 lines across TODO-bug-fixes.md and CLAUDE.md

---

## References

- [docs/changes/README.md](../../../docs/changes/README.md) - Change document index
- [documentation-standards.md](./documentation-standards.md) - Documentation organization
- [documentation-style-guide.md](./documentation-style-guide.md) - Writing style

---

**Protocol Owner:** Development Team
**Review Frequency:** Quarterly
**Next Review:** 2026-04-20
