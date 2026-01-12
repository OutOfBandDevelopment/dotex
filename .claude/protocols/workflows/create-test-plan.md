# Create Test Plan Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Generate comprehensive test plan based on code changes

---

## Trigger Phrases

- "create test plan"
- "generate test plan"
- "make test plan"
- "test planning"

---

## Overview

This protocol generates a comprehensive test plan based on the Change Review Summary, ensuring all changes are properly tested before merge.

---

## Prerequisites

- [ ] Change Review completed (Change Review Summary must exist in docs)
- [ ] Feature documentation must exist in `docs/notes/`

**If Prerequisites Not Met:**
```
PREREQUISITE MISSING

Change Review Summary not found in feature documentation.

Please run "review my changes" first to:
1. Complete code review
2. Generate Change Review Summary
3. Add summary to feature documentation

Cannot create test plan without completed change review.
```

---

## Protocol Steps

### Step 1: Verify Prerequisites

**Actions:**
- [ ] Find feature documentation in `docs/notes/`
- [ ] Verify Change Review Summary section exists
- [ ] Extract changed files and impact analysis

---

### Step 2: Analyze Changes for Test Scenarios

**For Each Changed Component:**

1. **Identify testable functionality:**
   - What does this code do?
   - What inputs does it accept?
   - What outputs does it produce?
   - What side effects does it have?

2. **Identify test types needed:**
   - Unit tests (isolated logic)
   - Integration tests (component interaction)
   - System tests (end-to-end)
   - Manual tests (UI, user experience)

3. **Identify test data needs:**
   - Valid input data
   - Invalid input data
   - Boundary conditions
   - Edge cases

---

### Step 3: Create Test Plan Document

**File Location:**
```
docs/testing/{Component}-{Feature}.md

Example:
docs/testing/Authentication-PasswordSecurity.md
```

**Test Plan Template:**

```markdown
# Test Plan: {Feature Name}

**Work Item:** #{work-item-id}
**Branch:** `{branch-name}`
**Status:** Draft
**Created:** YYYY-MM-DD

**Feature Documentation:** [Link](../notes/{matching-filename}.md)

---

## Test Objectives

Based on the Change Review Summary, this test plan validates:

1. {Objective from acceptance criteria}
2. {Objective from acceptance criteria}
3. {Objective from acceptance criteria}

---

## Scope

### In Scope

- {Component/feature 1}
- {Component/feature 2}

### Out of Scope

- {Component/feature not being tested}

---

## Test Strategy

### Test Levels

| Level           | Focus                     | Location                     |
|-----------------|---------------------------|------------------------------|
| Unit Tests      | Individual methods/classes| `tests/unit/`                |
| Integration     | Component interaction     | `tests/integration/`         |
| System Tests    | End-to-end flows          | `tests/e2e/`                 |
| Manual Tests    | User experience           | This document                |

---

## Test Environment

**Requirements:**
- Operating System: {OS}
- Database: {DB}
- Configuration: {special config}
- Test Data: {data requirements}

---

## Test Scenarios

### Scenario 1: {Happy Path Scenario}

**Objective:** {What this scenario tests}

**Preconditions:**
- {Precondition 1}
- {Precondition 2}

**Test Cases:**

#### TC-1.1: {Test Case Name}

| Field           | Value                                  |
|-----------------|----------------------------------------|
| Priority        | High                                   |
| Type            | Functional                             |
| Automated       | Yes/No                                 |

**Steps:**
1. {Step 1}
2. {Step 2}
3. {Step 3}

**Expected Result:** {expected outcome}

**Status:** Not Started

---

### Scenario 2: {Error Handling Scenario}

**Objective:** {What this scenario tests}

**Test Cases:**

#### TC-2.1: {Invalid Input Test}

| Field           | Value                                  |
|-----------------|----------------------------------------|
| Priority        | High                                   |
| Type            | Negative                               |
| Automated       | Yes                                    |

**Steps:**
1. {Step 1 with invalid input}
2. {Step 2}

**Expected Result:** {expected error handling}

**Status:** Not Started

---

### Scenario 3: {Security Scenario}

**Objective:** Verify security controls

**Test Cases:**

#### TC-3.1: {Authorization Check}

| Field           | Value                                  |
|-----------------|----------------------------------------|
| Priority        | Critical                               |
| Type            | Security                               |
| Automated       | Yes                                    |

**Steps:**
1. {Step 1}
2. {Step 2}

**Expected Result:** {expected security behavior}

**Status:** Not Started

---

## Automated Test Specifications

### Unit Tests

**File:** `tests/unit/{TestClassName}.{ext}`

**Tests to Create:**

| Test Name                                    | Purpose                    |
|----------------------------------------------|----------------------------|
| `test_{method}_{scenario}_returns_{expected}`| {Purpose}                  |
| `test_{method}_{error}_throws_{exception}`   | {Purpose}                  |

### Integration Tests

**File:** `tests/integration/{TestClassName}.{ext}`

**Tests to Create:**

| Test Name                                    | Purpose                    |
|----------------------------------------------|----------------------------|
| `test_{flow}_{scenario}`                     | {Purpose}                  |

---

## Manual Test Procedures

### Procedure 1: {UI Verification}

**Purpose:** {What this validates}

**Steps:**
1. {Step 1}
2. {Step 2}
3. {Step 3}

**Verification:**
- [ ] {Check 1}
- [ ] {Check 2}

---

## Regression Testing

**Areas to Verify:**
- {Existing functionality 1}
- {Existing functionality 2}

**Regression Suite:**
- [ ] Run existing unit tests
- [ ] Run existing integration tests
- [ ] Manual smoke test of {features}

---

## Pass/Fail Criteria

**Feature Acceptance:**
- [ ] All acceptance criteria met
- [ ] All unit tests passing
- [ ] All integration tests passing
- [ ] No critical or high-severity bugs
- [ ] Manual test procedures completed successfully
- [ ] Security validation passed
- [ ] Regression tests passed

---

## Test Results

### Execution Summary

| Test Type         | Total | Passed | Failed | Blocked |
|-------------------|-------|--------|--------|---------|
| Unit Tests        |       |        |        |         |
| Integration Tests |       |        |        |         |
| Manual Tests      |       |        |        |         |
| Regression        |       |        |        |         |

### Defects Found

| ID   | Severity | Description             | Status   |
|------|----------|-------------------------|----------|
|      |          |                         |          |

---

## Sign-off

- [ ] All tests executed
- [ ] All critical bugs resolved
- [ ] Test results reviewed
- [ ] Ready for pull request

---

**Last Updated:** YYYY-MM-DD
**Test Author:** {name}
```

---

### Step 4: Update Feature Documentation

**Add test plan reference to feature documentation:**

```markdown
---

## Test Plan

Test plan created: [Test Plan](../testing/{Component}-{Feature}.md)

**Test Status:** Draft

---
```

---

### Step 5: Provide Summary

**Output:**
```
TEST PLAN CREATED

Created: docs/testing/{Component}-{Feature}.md

Test Plan includes:
- {N} test scenarios
- {N} test cases total
- {N} unit tests specified
- {N} integration tests specified
- {N} manual test procedures

Feature documentation updated with test plan link.

Next Steps:
1. Review test plan for completeness
2. Implement automated tests
3. Execute manual tests
4. Document results in test plan
5. Fix any defects found
6. Create pull request when all tests pass
```

---

## Validation Checklist

- [ ] Change Review Summary found and analyzed
- [ ] Test scenarios cover all changed functionality
- [ ] Unit test specifications created
- [ ] Integration test specifications created
- [ ] Manual test procedures defined
- [ ] Security test cases included (if applicable)
- [ ] Pass/fail criteria defined
- [ ] Test plan file created in `docs/testing/`
- [ ] Feature documentation updated with test plan link

---

## Common Issues

| Issue                          | Cause                          | Resolution                    |
|--------------------------------|--------------------------------|-------------------------------|
| Missing Change Review Summary  | Review not completed           | Run "review my changes" first |
| Incomplete test coverage       | Missing scenarios              | Add scenarios for all changes |
| No automated tests specified   | Complex UI-only changes        | Document manual procedures    |

---

## Related Protocols

- [change-review.md](./change-review.md) - Must complete first
- [start-work.md](./start-work.md) - Begin work on item
- [lifecycle.md](./lifecycle.md) - Development workflow
- [task-management.md](./task-management.md) - Task tracking

---

## Change Log

- 2026-01-03 v1.0: Generalized for any project
