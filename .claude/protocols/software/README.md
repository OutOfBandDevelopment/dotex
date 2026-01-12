# Software Analysis Protocols

**Last Updated:** 2026-01-03

---

## Overview

Protocols for software architecture analysis, documentation completeness validation, security auditing, code generation, and component standards.

---

## Protocols

### Architecture & Analysis

| Protocol                              | Version | Purpose                               |
|---------------------------------------|---------|---------------------------------------|
| [architectural-analysis.md]           | 1.0     | Systematic architecture documentation |
| [comprehensive-analysis-checklist.md] | 1.0     | Documentation completeness checklist  |
| [incoming-codebase-comparison.md]     | 1.0     | Compare incoming code with main codebase |
| [protocol-validation.md]              | 1.0     | Protocol quality assurance            |
| [security-audit.md]                   | 1.0     | API security audit                    |

### Documentation Generation

| Protocol                              | Version | Purpose                               |
|---------------------------------------|---------|---------------------------------------|
| [compare-documentation-to-project.md] | 1.0     | Gap analysis between docs and code    |
| [generate-rewrite-documentation.md]   | 1.0     | Generate rewrite documentation        |

### Code Generation

| Protocol                              | Version | Purpose                               |
|---------------------------------------|---------|---------------------------------------|
| [template-development.md]             | 1.0     | Template-based code generation        |
| [template-swagger-documentation.md]   | 1.0     | Template/OpenAPI doc maintenance      |

### Component Standards

| Protocol                              | Version | Purpose                               |
|---------------------------------------|---------|---------------------------------------|
| [schema-integration.md]               | 1.0     | Schema framework integration          |
| [datagrid-style-guide.md]             | 1.0     | DataGrid component standards          |

---

## Quick Reference

### When to Use Each Protocol

| Situation                                  | Protocol                            |
|--------------------------------------------|-------------------------------------|
| Analyzing a new codebase                   | architectural-analysis.md           |
| Comparing incoming code for migration      | incoming-codebase-comparison.md     |
| Documenting project architecture           | architectural-analysis.md           |
| Verifying documentation completeness       | comprehensive-analysis-checklist.md |
| Creating or updating a protocol            | protocol-validation.md              |
| Conducting security audit                  | security-audit.md                   |
| Comparing docs to implementation           | compare-documentation-to-project.md |
| Creating rewrite documentation             | generate-rewrite-documentation.md   |
| Developing code generation templates       | template-development.md             |
| Maintaining template documentation         | template-swagger-documentation.md   |
| Integrating schema validation frameworks   | schema-integration.md               |
| Implementing data grids                    | datagrid-style-guide.md             |

---

## Workflow

### Architecture Documentation

```
1. architectural-analysis.md
   |
   v  (creates architecture documentation)
   |
2. comprehensive-analysis-checklist.md
   |
   v  (validates completeness)
   |
3. protocol-validation.md
   |
   v  (validates any new protocols created)
```

### Rewrite Planning

```
1. generate-rewrite-documentation.md
   |
   v  (creates baseline documentation)
   |
2. compare-documentation-to-project.md
   |
   v  (identifies gaps)
   |
3. [Implementation Planning]
```

### Code Generation

```
1. template-development.md
   |
   v  (develop templates)
   |
2. template-swagger-documentation.md
   |
   v  (maintain documentation)
   |
3. schema-integration.md
   |
   v  (integrate with components)
```

---

## Key Concepts

### Architectural Analysis

- **Purpose:** Create comprehensive architecture documentation
- **Outputs:**
  - Architecture documentation
  - Patterns documentation
  - C4 diagrams (Context, Container)

### Security Audit

- **Purpose:** Ensure API security
- **Validates:**
  - Secure-by-default configuration
  - Authorization attributes
  - Data scoping
  - Sensitive data protection

### Schema Integration

- **Purpose:** Leverage schema validation for UI configuration
- **Benefits:**
  - Auto-generated column definitions
  - Type safety
  - Reduced boilerplate

### DataGrid Standards

- **Purpose:** Consistent data grid implementation
- **Covers:**
  - Column definitions
  - Filtering patterns
  - Styling standards
  - TypeScript patterns

---

## Related Protocols

- [Documentation Style Guide](../documentation/documentation-style-guide.md) - Content standards
- [Documentation Standards](../documentation/documentation-standards.md) - File organization

---

[architectural-analysis.md]: ./architectural-analysis.md
[comprehensive-analysis-checklist.md]: ./comprehensive-analysis-checklist.md
[incoming-codebase-comparison.md]: ./incoming-codebase-comparison.md
[protocol-validation.md]: ./protocol-validation.md
[security-audit.md]: ./security-audit.md
[compare-documentation-to-project.md]: ./compare-documentation-to-project.md
[generate-rewrite-documentation.md]: ./generate-rewrite-documentation.md
[template-development.md]: ./template-development.md
[template-swagger-documentation.md]: ./template-swagger-documentation.md
[schema-integration.md]: ./schema-integration.md
[datagrid-style-guide.md]: ./datagrid-style-guide.md
