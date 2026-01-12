# Start Work Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Begin work on a new feature or bug fix with proper setup

---

## Trigger Phrases

- "start work on {work item number}"
- "begin work on {work item number}"
- "start {work item number}"
- "work on {work item number}"

---

## Overview

This protocol helps you start work on a new feature or bug fix by:
1. Fetching work item details (if tracking system configured)
2. Creating an appropriate branch from the latest code
3. Analyzing requirements and locating impacted code
4. Creating initial documentation
5. Suggesting initial changes

---

## Protocol Steps

### Step 1: Extract Work Item Number

**Actions:**
- [ ] Parse user input to extract work item number
- [ ] Validate work item number is numeric

**Pattern Matching:**
```
"start work on 115163" -> 115163
"begin work on #115163" -> 115163
"work on 115163" -> 115163
```

**If Invalid:**
```
I need a valid work item number to start work.

Example: "start work on 115163"
```

---

### Step 2: Fetch Work Item Details (Optional)

If work item tracking is configured in project-config.json:

**Extract:**
- Work item title
- Work item type (Feature, Bug, Task, User Story, etc.)
- Description and acceptance criteria
- Related work items
- Priority and severity
- Assigned to
- Tags
- Comments with implementation guidance

**Output:** Display work item summary

---

### Step 3: Determine Base Branch

**Actions:**
- [ ] Check if repository uses `master` or `main`
- [ ] Fetch latest changes from base branch
- [ ] Verify base branch is up to date

**Commands:**
```bash
# Check which base branch exists
git branch -r | grep -E "origin/(master|main)$"

# Fetch latest changes
git fetch origin

# Get latest commit on base branch
git log origin/main -1 --oneline
```

---

### Step 4: Determine Branch Name

**Actions:**
- [ ] Determine branch prefix based on work item type
- [ ] Create descriptive branch name from work item title
- [ ] Validate branch name follows conventions

**Branch Naming Rules:**

| Type         | Pattern                              | Example                              |
|--------------|--------------------------------------|--------------------------------------|
| Feature      | `features/{workitem}-{description}`  | `features/123-user-auth`             |
| Bug          | `bugs/{workitem}-{description}`      | `bugs/456-login-fix`                 |
| Task         | `tasks/{workitem}-{description}`     | `tasks/789-refactor`                 |
| User Story   | `features/{workitem}-{description}`  | `features/101-checkout-flow`         |

**Description Generation:**
- Take work item title
- Convert to kebab-case
- Limit to 3-5 key words
- Remove articles (a, an, the)
- Remove special characters

---

### Step 5: Create Branch

**Actions:**
- [ ] Ensure working directory is clean
- [ ] Switch to base branch
- [ ] Pull latest changes
- [ ] Create new branch from base
- [ ] Push new branch to remote (optional)

**Commands:**
```bash
# Check for uncommitted changes
git status --short

# If clean, proceed
git checkout {base-branch}
git pull origin {base-branch}
git checkout -b {new-branch-name}

# Optionally push to remote
git push -u origin {new-branch-name}
```

**If Working Directory Not Clean:**
```
Working directory has uncommitted changes.

Options:
1. Commit current changes first
2. Stash changes: git stash
3. Discard changes: git reset --hard (CAREFUL!)

What would you like to do?
```

---

### Step 6: Analyze Requirements and Locate Impacted Code

**Actions:**
- [ ] Parse acceptance criteria and description
- [ ] Identify key components mentioned
- [ ] Search for potentially impacted files
- [ ] Identify similar existing features
- [ ] Locate relevant code patterns

**Analysis Questions:**
1. What components are mentioned?
2. What functionality is affected?
3. Are there similar features?
4. What patterns should be used?
5. What needs to be modified?

---

### Step 7: Create Initial Feature Documentation

**Actions:**
- [ ] Create documentation file in `docs/notes/`
- [ ] Use appropriate naming convention
- [ ] Include all required sections
- [ ] Mark sections as TODO where details aren't known yet

**Documentation File Naming:**
```
{Component}-{Feature}.md
{WorkItem}-{ShortDescription}.md

Examples:
- KeyDatabase-PasswordSecurityFix.md
- 115163-PlaintextPasswordFix.md
```

**Required Sections:**
1. Overview
2. Branch Information
3. Problem Statement
4. Solution
5. Implementation Plan
6. Testing Strategy
7. Placeholder for Change Review Summary
8. Placeholder for Test Plan reference

---

### Step 8: Suggest Initial Changes

**Actions:**
- [ ] Based on work item and code analysis, suggest implementation approach
- [ ] Identify files to modify
- [ ] Suggest code patterns to follow
- [ ] Recommend similar implementations to reference

---

### Step 9: Provide Work Summary and Next Steps

**Summary Template:**

```markdown
WORK STARTED ON #{work-item-id}

## Summary

**Work Item:** #{number} - {title}
**Branch:** `{branch-name}` (created from {base-branch})
**Type:** {Feature|Bug|Task}
**Priority:** {priority}

## Files Created

**Feature Documentation**
- `docs/notes/{filename}.md`
  - Work item details
  - Problem statement (TODO)
  - Potentially impacted code identified
  - Implementation plan (TODO)

## Code Analysis Results

**Potentially Impacted Components:**
{list}

**Files to Review:**
{list}

**Similar Implementations:**
{list}

## Next Steps

1. **Review the documentation:**
   - `docs/notes/{filename}.md`

2. **Start implementation:**
   - Modify identified files
   - Follow similar patterns in codebase
   - Write tests alongside code

3. **Update documentation as you work:**
   - Remove TODOs
   - Document actual changes

4. **When ready for review:**
   - Say "review my changes" to run the code review protocol
```

---

## Error Handling

| Error                     | Cause                          | Resolution                       |
|---------------------------|--------------------------------|----------------------------------|
| Work Item Not Found       | Invalid ID or no access        | Verify work item number          |
| Branch Already Exists     | Previous work on same item     | Switch to existing or rename     |
| Cannot Determine Base     | No master/main branch          | Specify branch explicitly        |
| Uncommitted Changes       | Dirty working directory        | Commit, stash, or discard        |

---

## Integration with Other Protocols

### After Starting Work

When implementation is complete:
1. **Run Code Review:** "review my changes" - See `change-review.md`
2. **Create Test Plan:** "create test plan" - See `create-test-plan.md`

### Workflow

```
start work on {number}
    |
[Implement feature]
    |
review my changes
    |
create test plan
    |
[Run tests]
    |
[Create pull request]
```

---

## Related Protocols

- [lifecycle.md](./lifecycle.md) - Development workflow
- [change-review.md](./change-review.md) - Code review
- [create-test-plan.md](./create-test-plan.md) - Test planning
- [task-management.md](./task-management.md) - Task tracking

---

## Change Log

- 2026-01-03 v1.0: Generalized for any project
