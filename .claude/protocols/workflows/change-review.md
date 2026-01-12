# Change Review Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Comprehensive code review before creating test plan or pull request

---

## Trigger Phrases

- "review my changes"
- "code review"
- "review this change"
- "check my work"

---

## Overview

This protocol performs a comprehensive review of code changes, ensuring quality, security, and completeness before proceeding to test planning or pull request creation.

---

## Prerequisites

- [ ] Feature documentation must exist in `docs/notes/`
- [ ] Changes committed or staged in git
- [ ] On a feature/bug branch (not main/master)

**If Prerequisites Not Met:**
```
PREREQUISITE MISSING

Feature documentation not found in docs/notes/

Please create documentation first using:
- "create documentation" or
- "start work on {work item number}"

Cannot proceed with code review without documentation.
```

---

## Protocol Steps

### Step 1: Verify Documentation Exists

**Actions:**
- [ ] Search for feature documentation in `docs/notes/`
- [ ] Verify documentation matches current branch/feature
- [ ] Fail if documentation not found

---

### Step 2: Analyze Git Status

**Actions:**
- [ ] Get current branch name
- [ ] List all changed files (staged and unstaged)
- [ ] Identify commit history on branch
- [ ] Note any untracked files

**Commands:**
```bash
# Current branch
git rev-parse --abbrev-ref HEAD

# Changed files
git status --short

# Recent commits on branch
git log --oneline -10

# Diff summary
git diff --stat HEAD~{n}
```

---

### Step 3: Extract Work Item Information

**Actions:**
- [ ] Extract work item number from branch name
- [ ] Fetch work item details (if tracking configured)
- [ ] Compare implementation to acceptance criteria

---

### Step 4: Review Code Changes

**For Each Changed File:**

1. **Read the file** - Understand the changes
2. **Check code quality:**
   - [ ] Follows existing patterns
   - [ ] Consistent naming conventions
   - [ ] Appropriate comments
   - [ ] No debug code left in
   - [ ] No hardcoded values

3. **Check security:**
   - [ ] No secrets in code
   - [ ] Input validation present
   - [ ] Authorization checks in place
   - [ ] SQL injection prevention
   - [ ] XSS prevention

4. **Check completeness:**
   - [ ] All acceptance criteria addressed
   - [ ] Error handling complete
   - [ ] Edge cases handled
   - [ ] Unit tests added/updated

---

### Step 5: Search for Missed Changes

**Actions:**
- [ ] Search codebase for similar patterns that might need same fix
- [ ] Check for copy-pasted code that needs updating
- [ ] Verify all components affected are updated
- [ ] Check configuration files if needed

**Search Patterns:**
```bash
# Find similar patterns
grep -r "pattern" --include="*.{cs,ts,js,py}"

# Find related files
find . -name "*Related*" -type f
```

---

### Step 6: Check Test Coverage

**Actions:**
- [ ] Identify existing tests for modified code
- [ ] Verify tests still pass
- [ ] Note areas lacking test coverage
- [ ] Check for integration test needs

---

### Step 7: Review Dependencies

**Actions:**
- [ ] Check for new dependencies added
- [ ] Verify version compatibility
- [ ] Check for security vulnerabilities in dependencies
- [ ] Note license implications

---

### Step 8: Generate Change Review Summary

**Add to Feature Documentation:**

```markdown
---

## Change Review Summary

**Review Date:** YYYY-MM-DD
**Branch:** `{branch-name}`
**Reviewer:** Claude Code

### Files Changed

| File                        | Change Type | Lines Changed |
|-----------------------------|-------------|---------------|
| `path/to/file.cs`           | Modified    | +45, -12      |
| `path/to/new-file.cs`       | Added       | +120          |
| `path/to/deleted-file.cs`   | Deleted     | -50           |

### Code Quality Assessment

| Aspect              | Status | Notes                            |
|---------------------|--------|----------------------------------|
| Coding Standards    | PASS   | Follows existing patterns        |
| Security            | PASS   | No vulnerabilities found         |
| Error Handling      | PASS   | Appropriate error handling       |
| Documentation       | PASS   | Code comments adequate           |
| Test Coverage       | WARN   | New code needs unit tests        |

### Security Review

| Check                        | Status | Notes                     |
|------------------------------|--------|---------------------------|
| No secrets in code           | PASS   |                           |
| Input validation             | PASS   |                           |
| Authorization                | PASS   |                           |
| SQL Injection prevention     | N/A    | No database queries       |
| XSS prevention               | N/A    | No user-facing output     |

### Codebase Search Results

**Similar patterns found:**
- `path/to/similar/file.cs` - Uses same pattern, no update needed
- `path/to/other/file.cs` - May need similar update

### Missed Changes

| Location                     | Issue                          | Severity |
|------------------------------|--------------------------------|----------|
| (None found)                 |                                |          |

### Test Status

| Test Type         | Status   | Notes                         |
|-------------------|----------|-------------------------------|
| Unit Tests        | PENDING  | Need to create                |
| Integration Tests | PENDING  | Need to create                |
| Manual Tests      | PENDING  | Need to define                |

### Recommendations

1. **MUST DO:**
   - Create unit tests for new methods
   - Add integration test for end-to-end flow

2. **SHOULD DO:**
   - Add inline comments for complex logic
   - Consider adding logging

3. **NICE TO HAVE:**
   - Refactor duplicate code

### Merge Readiness

| Criteria                     | Status   |
|------------------------------|----------|
| Code quality acceptable      | PASS     |
| Security review passed       | PASS     |
| All components updated       | PASS     |
| Tests passing                | PENDING  |
| Documentation complete       | PASS     |

**Overall Status:** READY FOR TEST PLAN

---
```

---

### Step 9: Provide Next Steps

**Output:**
```
CHANGE REVIEW COMPLETE

Summary:
- Files changed: {count}
- Security issues: {count}
- Missed changes: {count}
- Test coverage: {status}

Change Review Summary added to:
  docs/notes/{feature-name}.md

Next Steps:
1. Address any MUST DO recommendations
2. Run "create test plan" to generate test plan
3. Implement and execute tests
4. Create pull request when ready
```

---

## Validation Checklist

- [ ] Documentation exists and matches feature
- [ ] Git status analyzed
- [ ] All changed files reviewed
- [ ] Code quality assessed
- [ ] Security review completed
- [ ] Codebase searched for missed changes
- [ ] Test coverage evaluated
- [ ] Change Review Summary added to documentation
- [ ] Next steps provided

---

## Common Issues

| Issue                          | Cause                          | Resolution                    |
|--------------------------------|--------------------------------|-------------------------------|
| Documentation not found        | Skipped start work protocol    | Run "start work on {id}"      |
| Security vulnerability found   | Missing validation/auth        | Fix before proceeding         |
| Similar code not updated       | Incomplete search              | Update all related locations  |
| Tests failing                  | Breaking change                | Fix tests or update code      |

---

## Related Protocols

- [start-work.md](./start-work.md) - Begin work on item
- [create-test-plan.md](./create-test-plan.md) - Test planning
- [lifecycle.md](./lifecycle.md) - Development workflow
- [task-management.md](./task-management.md) - Task tracking

---

## Change Log

- 2026-01-03 v1.0: Generalized for any project
