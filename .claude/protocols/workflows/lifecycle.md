# Development Lifecycle Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Complete development workflow from work item to pull request

---

## Trigger Phrases

- "show lifecycle"
- "show workflow"
- "what's the development process"
- "development workflow"
- "how do I develop a feature"

---

## Overview

This protocol documents the complete development lifecycle for features, from initial work item to pull request. Following this workflow ensures comprehensive documentation, thorough code review, and complete test coverage.

---

## Complete Development Workflow

```
1. start work on {work item}  <- Create branch and foundation docs
   |
2. [Implement changes]        <- Write code, update docs as you go
   |
3. review my changes          <- Comprehensive review, adds Change Review Summary
   |
4. create test plan           <- Generate test plan, links from docs
   |
5. [Implement tests]          <- Write automated and manual tests
   |
6. [Execute tests]            <- Run tests, document results
   |
7. [Create pull request]      <- Submit for team review
```

---

## Lifecycle Phases

### Phase 1: Start Work

**Trigger:** `start work on {work item number}`

**Protocol:** `start-work.md`

**What Happens:**
1. Fetches work item from tracking system (if configured)
2. Creates feature branch from main branch
3. Calls "create documentation" protocol
4. Analyzes codebase for similar implementations
5. Suggests implementation approach
6. Provides next steps

**Output:**
- New feature branch: `features/{workitem}-{description}`
- Foundation documentation: `docs/notes/{Component}-{Feature}.md`
- Implementation suggestions based on codebase patterns

**Next Step:** Implement the feature/change

---

### Phase 2: Implement Changes

**Manual Work**

**What To Do:**
1. Write code following implementation plan
2. Update documentation as you work
3. Fill in TODO sections in feature documentation
4. Document decisions and questions
5. Commit changes regularly with descriptive messages

**Tips:**
- Keep documentation current, don't wait until the end
- Mark completed implementation plan steps
- Document any deviations from original plan
- Track questions and get them answered
- Follow security and coding best practices

**Next Step:** Run "review my changes"

---

### Phase 3: Code Review

**Trigger:** `review my changes`

**Protocol:** `change-review.md`

**Prerequisites:**
- Feature documentation must exist in `docs/notes/`

**What Happens:**
1. Verifies documentation exists (fails if not found)
2. Analyzes git status and changed files
3. Extracts and validates branch name
4. Reviews all code changes for quality and security
5. Searches entire codebase for missed changes
6. Checks for test results
7. Reviews dependencies and build configuration
8. Updates documentation with Change Review Summary

**Output:**
- Comprehensive Change Review Summary added to feature documentation
- Security vulnerabilities identified
- Missed changes found
- Test status reviewed
- Merge readiness assessment

**Next Step:** Run "create test plan"

---

### Phase 4: Create Test Plan

**Trigger:** `create test plan`

**Protocol:** `create-test-plan.md`

**Prerequisites:**
- Change review completed (Change Review Summary must exist)
- Feature documentation must exist in `docs/notes/`

**What Happens:**
1. Verifies change review completed (fails if not found)
2. Extracts change information from Change Review Summary
3. Analyzes end-to-end application flow
4. Creates comprehensive test scenarios and test cases
5. Defines automated test specifications
6. Creates manual test procedures
7. Includes security/performance testing if applicable
8. Generates test plan in `docs/testing/`
9. Updates feature documentation with test plan link

**Output:**
- Comprehensive test plan: `docs/testing/{Component}-{Feature}.md`
- Test scenarios and test cases
- Automated test specifications
- Manual test procedures
- Feature documentation updated with test plan reference

**Next Step:** Implement tests

---

### Phase 5: Implement Tests

**Manual Work**

**What To Do:**
1. Read test plan: `docs/testing/{Component}-{Feature}.md`
2. Implement automated tests (unit, integration)
3. Follow automated test specifications in test plan
4. Add tests to appropriate test projects
5. Update test plan with implementation status
6. Mark test cases as implemented

**Test Quality:**
- Follow AAA pattern (Arrange, Act, Assert)
- Use descriptive test names
- Test happy path and error cases
- Mock external dependencies
- Ensure tests are repeatable

**Next Step:** Execute tests

---

### Phase 6: Execute Tests

**Manual Work**

**What To Do:**
1. Run automated tests
2. Execute manual test procedures from test plan
3. Document test results in test plan
4. Mark test cases as: Passed / Failed / In Progress / Not Started
5. Document any defects found
6. Fix defects and retest
7. Update Test Execution Summary in test plan

**Evidence Collection:**
- Screenshots of manual tests
- Test output logs
- Performance measurements
- Security scan results

**Next Step:** Create pull request (when all tests pass)

---

### Phase 7: Create Pull Request

**Manual Work**

**What To Do:**
1. Ensure all tests pass
2. Review feature documentation is complete
3. Review test plan is complete
4. Commit any final changes
5. Push branch to remote
6. Create pull request
7. Link work item to pull request
8. Add reviewers
9. Reference documentation in PR description

**Pull Request Description Template:**
```markdown
## Summary
{Brief description of change}

## Work Item
#{work-item-number} - {title}

## Documentation
- Feature Documentation: `docs/notes/{Component}-{Feature}.md`
- Test Plan: `docs/testing/{Component}-{Feature}.md`
- Change Review: See "Change Review Summary" in feature docs

## Test Results
- Unit Tests: {count} passed
- Integration Tests: {count} passed
- Manual Tests: {count} passed
- All tests: PASSED

## Checklist
- [x] Documentation complete
- [x] Code review completed
- [x] Test plan created
- [x] All tests passing
- [x] No security vulnerabilities
- [x] No breaking changes (or documented)
```

**Next Step:** Wait for team review and merge

---

## Quality Gates

### Gate 1: Documentation Created (After Phase 1)

**Check:**
- [ ] Feature documentation exists in `docs/notes/`
- [ ] Work item details included
- [ ] Problem statement documented (or TODO)
- [ ] Solution approach documented (or TODO)
- [ ] Implementation plan outlined

**If Failed:** Cannot proceed to implementation

---

### Gate 2: Code Review Complete (After Phase 3)

**Check:**
- [ ] Change Review Summary added to documentation
- [ ] All components reviewed
- [ ] Security vulnerabilities addressed
- [ ] Codebase searched for missed changes
- [ ] Test status checked
- [ ] Merge readiness assessed

**If Failed:** Fix issues before creating test plan

---

### Gate 3: Test Plan Created (After Phase 4)

**Check:**
- [ ] Test plan exists in `docs/testing/`
- [ ] Test scenarios comprehensive
- [ ] Test cases cover all changes
- [ ] Automated test specifications defined
- [ ] Manual test procedures defined
- [ ] Feature documentation updated with test plan link

**If Failed:** Cannot proceed to test implementation

---

### Gate 4: Tests Passing (After Phase 6)

**Check:**
- [ ] All automated tests passing
- [ ] All manual tests passed
- [ ] Test results documented in test plan
- [ ] All defects resolved
- [ ] Test summary complete

**If Failed:** Cannot create pull request

---

## Protocol Reference

### Protocols Used in Lifecycle

| Protocol           | Trigger                      | Purpose                    |
|--------------------|------------------------------|----------------------------|
| start-work.md      | `start work on {number}`     | Begin new work             |
| create-docs.md     | `create documentation`       | Create feature docs        |
| change-review.md   | `review my changes`          | Comprehensive review       |
| create-test-plan.md| `create test plan`           | Generate test plan         |

---

## Related Protocols

- [start-work.md](./start-work.md) - Begin work on item
- [change-review.md](./change-review.md) - Code review
- [create-test-plan.md](./create-test-plan.md) - Test planning
- [task-management.md](./task-management.md) - Task tracking

---

## Change Log

- 2026-01-03 v1.0: Generalized for any project
- 2026-01-02 v1.0: Initial lifecycle protocol
