# Protocol Registry

**Version:** 1.1
**Last Updated:** 2026-01-03

---

## Overview

This directory contains reusable protocols for software development workflows. These protocols are project-agnostic and can be used across any codebase.

---

## Protocol Categories

| Category      | Path                 | Purpose                             |
|---------------|----------------------|-------------------------------------|
| Documentation | `./documentation/`   | Documentation standards and guides  |
| Software      | `./software/`        | Analysis and validation protocols   |
| Workflows     | `./workflows/`       | Development lifecycle protocols     |

---

## Active Protocols

### Documentation Protocols

| Protocol                      | Version | Purpose                                  |
|-------------------------------|---------|------------------------------------------|
| [documentation-style-guide.md] | 2.1     | Technical documentation content standards |
| [documentation-standards.md]  | 1.2     | File organization and naming conventions |
| [documentation-structure.md]  | 1.0     | Directory structure and templates        |

### Software Analysis Protocols

| Protocol                             | Version | Purpose                               |
|--------------------------------------|---------|---------------------------------------|
| [architectural-analysis.md]          | 1.0     | Systematic architecture documentation |
| [comprehensive-analysis-checklist.md] | 1.0    | Documentation completeness checklist  |
| [protocol-validation.md]             | 1.0     | Protocol quality assurance            |
| [security-audit.md]                  | 1.0     | API security audit                    |
| [compare-documentation-to-project.md] | 1.0    | Gap analysis                          |
| [generate-rewrite-documentation.md]  | 1.0     | Rewrite documentation generation      |
| [template-development.md]            | 1.0     | Code generation template workflow     |
| [schema-integration.md]              | 1.0     | Schema framework integration          |
| [datagrid-style-guide.md]            | 1.0     | DataGrid component standards          |
| [template-swagger-documentation.md]  | 1.0     | Template/OpenAPI documentation        |

### Workflow Protocols

| Protocol              | Version | Trigger Phrase               | Purpose                   |
|-----------------------|---------|------------------------------|---------------------------|
| [init-setup.md]       | 1.0     | `init`, `setup project`      | Project initialization    |
| [lifecycle.md]        | 1.0     | `show lifecycle`             | Development workflow      |
| [start-work.md]       | 1.0     | `start work on {id}`         | Begin work on item        |
| [change-review.md]    | 1.0     | `review my changes`          | Code review               |
| [create-test-plan.md] | 1.0     | `create test plan`           | Test planning             |
| [task-management.md]  | 1.1     | -                            | Task lifecycle management |
| [review-branch-updates.md] | 1.0 | `pulled latest`             | Branch update review      |

---

## Quick Reference

### Development Workflow

```
1. init                       <- Initialize project (first time only)
   |
2. start work on {id}         <- Create branch and docs
   |
3. [Implement changes]        <- Write code
   |
4. review my changes          <- Code review
   |
5. create test plan           <- Test planning
   |
6. [Implement & run tests]    <- Execute tests
   |
7. [Create pull request]      <- Submit for review
```

### Trigger Phrases

| Phrase                       | Protocol                   |
|------------------------------|----------------------------|
| `init`                       | init-setup.md              |
| `setup project`              | init-setup.md              |
| `start work on {number}`     | start-work.md              |
| `review my changes`          | change-review.md           |
| `create test plan`           | create-test-plan.md        |
| `show lifecycle`             | lifecycle.md               |
| `show workflow`              | lifecycle.md               |
| `pulled latest`              | review-branch-updates.md   |

---

## Protocol Dependencies

```
init-setup.md
    |
    v
start-work.md --> (creates documentation)
    |
    v
change-review.md --> (requires documentation)
    |
    v
create-test-plan.md --> (requires change review)
```

---

## Adding New Protocols

1. **Choose category:** documentation, software, or workflows
2. **Use kebab-case naming:** `my-new-protocol.md`
3. **Follow template:** Include all required sections
4. **Validate:** Use protocol-validation.md
5. **Update registry:** Add to this README.md

### Required Protocol Sections

- Title and metadata (version, date, purpose)
- Overview
- When to Use
- Prerequisites
- Procedure (step-by-step)
- Validation Checklist
- Common Issues
- Related Protocols
- Change Log

---

## Naming Conventions

| Type             | Convention     | Example                        |
|------------------|----------------|--------------------------------|
| Protocol files   | kebab-case     | `documentation-style-guide.md` |
| Category folders | lowercase      | `documentation/`               |
| Versions         | Semantic       | `1.0`, `1.1`, `2.0`            |

---

## Version History

| Date       | Version | Changes                                    |
|------------|---------|------------------------------------------- |
| 2026-01-03 | 1.1     | Added software and workflow protocols      |
| 2026-01-03 | 1.0     | Initial protocol registry consolidated     |

---

## Links

### Documentation Protocols
- [Documentation Style Guide](./documentation/documentation-style-guide.md)
- [Documentation Standards](./documentation/documentation-standards.md)
- [Documentation Structure](./documentation/documentation-structure.md)

### Software Analysis Protocols
- [Architectural Analysis](./software/architectural-analysis.md)
- [Comprehensive Analysis Checklist](./software/comprehensive-analysis-checklist.md)
- [Protocol Validation](./software/protocol-validation.md)
- [Security Audit](./software/security-audit.md)
- [Compare Documentation to Project](./software/compare-documentation-to-project.md)
- [Generate Rewrite Documentation](./software/generate-rewrite-documentation.md)
- [Template Development](./software/template-development.md)
- [Schema Integration](./software/schema-integration.md)
- [DataGrid Style Guide](./software/datagrid-style-guide.md)
- [Template and Swagger Documentation](./software/template-swagger-documentation.md)

### Workflow Protocols
- [Init Setup](./workflows/init-setup.md)
- [Development Lifecycle](./workflows/lifecycle.md)
- [Start Work](./workflows/start-work.md)
- [Change Review](./workflows/change-review.md)
- [Create Test Plan](./workflows/create-test-plan.md)
- [Task Management](./workflows/task-management.md)
- [Review Branch Updates](./workflows/review-branch-updates.md)

[documentation-style-guide.md]: ./documentation/documentation-style-guide.md
[documentation-standards.md]: ./documentation/documentation-standards.md
[documentation-structure.md]: ./documentation/documentation-structure.md
[architectural-analysis.md]: ./software/architectural-analysis.md
[comprehensive-analysis-checklist.md]: ./software/comprehensive-analysis-checklist.md
[protocol-validation.md]: ./software/protocol-validation.md
[security-audit.md]: ./software/security-audit.md
[compare-documentation-to-project.md]: ./software/compare-documentation-to-project.md
[generate-rewrite-documentation.md]: ./software/generate-rewrite-documentation.md
[template-development.md]: ./software/template-development.md
[schema-integration.md]: ./software/schema-integration.md
[datagrid-style-guide.md]: ./software/datagrid-style-guide.md
[template-swagger-documentation.md]: ./software/template-swagger-documentation.md
[init-setup.md]: ./workflows/init-setup.md
[lifecycle.md]: ./workflows/lifecycle.md
[start-work.md]: ./workflows/start-work.md
[change-review.md]: ./workflows/change-review.md
[create-test-plan.md]: ./workflows/create-test-plan.md
[task-management.md]: ./workflows/task-management.md
[review-branch-updates.md]: ./workflows/review-branch-updates.md
