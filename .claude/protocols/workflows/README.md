# Workflow Protocols

**Last Updated:** 2026-01-03

---

## Overview

Protocols for the complete software development lifecycle, from project initialization through pull request creation.

---

## Protocols

| Protocol                   | Version | Trigger Phrase               | Purpose                   |
|----------------------------|---------|------------------------------|---------------------------|
| [init-setup.md]            | 1.0     | `init`, `setup project`      | Project initialization    |
| [lifecycle.md]             | 1.0     | `show lifecycle`             | Development workflow      |
| [start-work.md]            | 1.0     | `start work on {id}`         | Begin work on item        |
| [change-review.md]         | 1.0     | `review my changes`          | Code review               |
| [create-test-plan.md]      | 1.0     | `create test plan`           | Test planning             |
| [task-management.md]       | 1.1     | -                            | Task lifecycle management |
| [review-branch-updates.md] | 1.0     | `pulled latest`              | Branch update review      |

---

## Development Workflow

```
1. init                      <- Project initialization (first time only)
   |
2. start work on {id}        <- Create branch and docs
   |
3. [Implement changes]       <- Write code
   |
4. review my changes         <- Code review
   |
5. create test plan          <- Test planning
   |
6. [Implement & run tests]   <- Execute tests
   |
7. [Create pull request]     <- Submit for review
```

---

## Protocol Dependencies

```
init-setup.md (first time only)
      |
      v
start-work.md -----> creates feature documentation
      |
      v
change-review.md --> requires feature documentation
      |                adds Change Review Summary
      v
create-test-plan.md -> requires Change Review Summary
      |                 creates test plan
      v
[Pull Request]
```

---

## Trigger Phrases

| Phrase                       | Protocol                   |
|------------------------------|----------------------------|
| `init`                       | init-setup.md              |
| `setup project`              | init-setup.md              |
| `configure project`          | init-setup.md              |
| `start work on {number}`     | start-work.md              |
| `begin work on {number}`     | start-work.md              |
| `work on {number}`           | start-work.md              |
| `review my changes`          | change-review.md           |
| `code review`                | change-review.md           |
| `create test plan`           | create-test-plan.md        |
| `generate test plan`         | create-test-plan.md        |
| `show lifecycle`             | lifecycle.md               |
| `show workflow`              | lifecycle.md               |
| `development workflow`       | lifecycle.md               |
| `pulled latest`              | review-branch-updates.md   |
| `pulled in latest`           | review-branch-updates.md   |
| `review the changes`         | review-branch-updates.md   |

---

## Quick Reference

### When to Use Each Protocol

| Situation                                  | Protocol                   |
|--------------------------------------------|----------------------------|
| Setting up a new project                   | init-setup.md              |
| Starting a new feature or bug fix          | start-work.md              |
| Completed implementation, need review      | change-review.md           |
| Change review done, need test plan         | create-test-plan.md        |
| Need to understand the development process | lifecycle.md               |
| Tracking tasks and priorities              | task-management.md         |
| After pulling latest changes               | review-branch-updates.md   |

---

## Key Outputs

| Protocol                   | Creates                                        |
|----------------------------|------------------------------------------------|
| init-setup.md              | `.claude/project-config.json`, `CLAUDE.md`     |
| start-work.md              | Branch, `docs/notes/{feature}.md`              |
| change-review.md           | Change Review Summary in feature docs          |
| create-test-plan.md        | `docs/testing/{feature}.md`, test plan link    |
| review-branch-updates.md   | Updated coverage docs, implemented features    |

---

## Related Protocols

- [Documentation Style Guide](../documentation/documentation-style-guide.md) - Content standards
- [Documentation Standards](../documentation/documentation-standards.md) - File organization
- [Compare Documentation to Project](../software/compare-documentation-to-project.md) - Gap analysis

---

[init-setup.md]: ./init-setup.md
[lifecycle.md]: ./lifecycle.md
[start-work.md]: ./start-work.md
[change-review.md]: ./change-review.md
[create-test-plan.md]: ./create-test-plan.md
[task-management.md]: ./task-management.md
[review-branch-updates.md]: ./review-branch-updates.md
