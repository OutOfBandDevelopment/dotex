# Task Management Protocol

**Version:** 1.1
**Last Updated:** 2026-01-03
**Purpose:** Standard workflow for managing feature requests, bug fixes, and enhancements

---

## Overview

Every task follows this lifecycle:

```
Request -> Track -> Review -> Propose -> Plan -> Implement -> Document -> Complete -> Cleanup
```

---

## Phase 1: Add to Tracking

### 1.1 Receive Request

When a new feature request or bug report is received:

1. **Clarify requirements** - Ask questions if unclear
2. **Assess scope** - Is this a quick fix or multi-phase feature?
3. **Identify dependencies** - What existing code/systems are affected?

### 1.2 Add to TODO.md

Add new item to `TODO.md` under "Active Work" section:

```markdown
### N. [Feature Name]

**Status:** `PROPOSED`
**Priority:** [High/Medium/Low]
**Document:** [docs/proposals/{feature}.md](./docs/proposals/{feature}.md)

[Brief 1-2 sentence summary]

**Key Deliverables:**
- [ ] Deliverable 1
- [ ] Deliverable 2
- [ ] Deliverable 3
```

### 1.3 Priority Levels

| Priority | Criteria |
|----------|----------|
| **CRITICAL** | Security, data loss, blocking issues |
| **HIGH** | Core functionality, user-facing bugs |
| **MEDIUM** | Enhancements, quality improvements |
| **LOW** | Nice-to-have, future considerations |

---

## Phase 2: Review Feature Request

### 2.1 Understand the Problem

- What problem does this solve?
- Who is affected?
- What is the current workaround (if any)?
- What is the business impact?

### 2.2 Research Existing Code

Search for related code and patterns in the codebase.

### 2.3 Identify Affected Areas

Document:
- Which layers are affected?
- Which files need modification?
- Are there database changes needed?
- Are there frontend changes needed?

### 2.4 Assess Complexity

| Complexity | Criteria | Approach |
|------------|----------|----------|
| **Simple** | Single file, < 50 lines | Direct implementation |
| **Medium** | 2-5 files, single layer | Brief proposal |
| **Complex** | Multiple layers, new patterns | Full proposal document |
| **Major** | Architecture change, breaking changes | ADR + detailed proposal |

---

## Phase 3: Propose Changes

### 3.1 Create Proposal Document

For Medium+ complexity, create `docs/proposals/{feature}.md`:

```markdown
# Proposal: [Feature Name]

**Status:** Planning
**Priority:** [HIGH/MEDIUM/LOW]
**Created:** YYYY-MM-DD

---

## Problem Statement

[Describe the problem being solved]

---

## Proposed Solution

[High-level solution description]

### Option A: [Approach Name]
- Pros: ...
- Cons: ...

### Option B: [Alternative] (if applicable)
- Pros: ...
- Cons: ...

### Recommended: [Option X]

[Rationale for recommendation]

---

## Implementation Plan

### Phase 1: [Phase Name]
- [ ] Task 1
- [ ] Task 2

### Phase 2: [Phase Name]
- [ ] Task 3
- [ ] Task 4

---

## Files to Modify

| File | Change |
|------|--------|
| path/to/file | Description of change |

---

## Testing Strategy

[How will this be tested?]

---

## Rollback Plan

[How to revert if issues arise]

---

*Created: YYYY-MM-DD*
```

### 3.2 Get Approval

- Present proposal to stakeholders
- Address questions and concerns
- Update proposal based on feedback
- Update status in TODO.md: `PROPOSED` -> `APPROVED`

---

## Phase 4: Create Test Plan

### 4.1 Identify Test Scenarios

| Category | Test Cases |
|----------|------------|
| **Happy Path** | Normal successful operations |
| **Edge Cases** | Boundary conditions, empty inputs |
| **Error Cases** | Invalid inputs, system failures |
| **Security** | Authorization, data scoping |
| **Performance** | Load, concurrency (if applicable) |

### 4.2 Document Test Plan

Add to proposal or create separate test document.

### 4.3 Define Acceptance Criteria

- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Manual testing scenarios verified
- [ ] No regression in existing functionality

---

## Phase 5: Implement Feature

### 5.1 Update Status

In TODO.md, change status:
```markdown
**Status:** `IN_PROGRESS`
```

### 5.2 Implementation Order

1. **Models/Entities** - Data structures first
2. **Repository** - Data access layer
3. **Service** - Business logic
4. **Controller** - API endpoints
5. **Frontend** - UI components (if applicable)

### 5.3 Follow Coding Standards

- Add documentation to public APIs
- Follow existing patterns in codebase
- Apply appropriate security attributes
- Add comments only where logic isn't self-evident

### 5.4 Verify as You Go

Build and test after each significant change.

---

## Phase 6: Update Documents

### 6.1 Architecture Documentation

If feature introduces new patterns:
- Update relevant `docs/architecture/*.md`
- Add diagrams if helpful (PlantUML)

### 6.2 API Documentation

If new endpoints or models:
- Add documentation to code
- Update API documentation
- Verify spec is generated correctly

### 6.3 CLAUDE.md Updates

If feature affects development workflow:
- Add to relevant sections
- Update file counts/locations if changed
- Add any new patterns to remember

---

## Phase 7: Add Change History

### 7.1 Change History vs Documentation

| File Type | Contains | Does NOT Contain |
|-----------|----------|------------------|
| `docs/changes/{feature}.md` | What changed, files modified, decisions made | How to use, examples, guides |
| `docs/architecture/*.md` | How it works, patterns, design | Change history |
| `docs/*.md` guides | How to use, examples, integration | Implementation details |

### 7.2 Create Change History File

Create `docs/changes/{feature}.md`:

```markdown
# Change: [Feature Name]

**Completed:** YYYY-MM-DD
**Category:** [Feature/Bug Fix/Enhancement/Documentation]

---

## Summary

[1-2 sentence description of what was implemented]

## What Changed

### Files Created
- `path/to/new/file`

### Files Modified
- `path/to/existing/file`

## Key Decisions

- [Decision 1 and rationale]
- [Decision 2 and rationale]

## Documentation

For usage and implementation details, see:
- [Architecture Doc](../architecture/relevant.md)
- [Guide](../relevant-guide.md)

---

*Completed: YYYY-MM-DD*
```

---

## Phase 8: Cleanup Tracking

### 8.1 Update TODO.md

1. **Remove from Active Work** section entirely
2. **Remove from Quick Reference** tables
3. **Remove from Pending Proposals** table

**Rationale:** TODO.md should only contain **active work**. Completed work is tracked in:
- `docs/changes/{feature}.md` - Change summary
- `docs/features/{feature}.md` - Feature documentation

### 8.2 Move Proposal to Features Folder

Once implemented and change history created:

```bash
mv docs/proposals/{feature}.md docs/features/{feature}.md
```

Update the status in the moved file:
```markdown
**Status**: Implemented (YYYY-MM-DD)
```

### 8.3 Final Verification

- [ ] TODO.md updated
- [ ] Change history created
- [ ] CHANGELOG.md updated
- [ ] Proposal document moved to features
- [ ] CLAUDE.md updated (if needed)
- [ ] All tests passing
- [ ] Documentation complete

---

## Quick Reference: Task States

```
PROPOSED -> APPROVED -> IN_PROGRESS -> COMPLETED
    |
  BLOCKED (if dependencies)
    |
  REJECTED (if not approved)
```

## Quick Reference: File Locations

| Purpose | Location |
|---------|----------|
| Active tracking | `TODO.md` |
| Active proposals | `docs/proposals/{feature}.md` |
| Completed features | `docs/features/{feature}.md` |
| Change summaries | `docs/changes/{feature}.md` |
| Architecture | `docs/architecture/*.md` |
| Protocols | `.claude/protocols/*.md` |

---

## Related Protocols

- [lifecycle.md](./lifecycle.md) - Development workflow
- [start-work.md](./start-work.md) - Begin work on item
- [change-review.md](./change-review.md) - Code review
- [documentation-standards.md](../documentation/documentation-standards.md) - Doc organization

---

## Change Log

- 2026-01-03 v1.1: Generalized for any project, added features folder workflow
- 2025-12-31 v1.0: Initial protocol
