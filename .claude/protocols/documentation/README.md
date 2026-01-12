# Documentation Protocols

**Last Updated:** 2026-01-03

---

## Overview

Protocols for documentation standards, content guidelines, directory structure, and file organization.

---

## Protocols

| Protocol                        | Version | Purpose                                   |
|---------------------------------|---------|-------------------------------------------|
| [documentation-style-guide.md]  | 2.1     | Technical documentation content standards |
| [documentation-standards.md]    | 1.2     | File organization and naming conventions  |
| [documentation-structure.md]    | 1.0     | Directory structure and templates         |

---

## Quick Reference

### When to Use Each Protocol

| Situation                             | Protocol                     |
|---------------------------------------|------------------------------|
| Writing technical documentation       | documentation-style-guide.md |
| Organizing project documentation      | documentation-standards.md   |
| Creating feature documentation        | Both protocols apply         |
| Setting up docs folder structure      | documentation-standards.md   |
| Creating directory structure          | documentation-structure.md   |
| Documenting architecture              | documentation-style-guide.md |
| Creating action/journey templates     | documentation-structure.md   |

---

## Key Concepts

### Documentation Style Guide

- **Focus:** What content to include and how to write it
- **Key Rules:**
  - Document only what exists in code
  - Every claim must have a code reference
  - No hypothetical content or estimates
  - Full stack traces for features

### Documentation Standards

- **Focus:** Where to put files and how to name them
- **Key Rules:**
  - Only README.md, CLAUDE.md, TODO.md at root
  - All docs under `./docs/`
  - Use kebab-case for filenames
  - Organize by epic/feature

### Documentation Structure

- **Focus:** Directory organization and templates
- **Key Rules:**
  - Standard directory hierarchy
  - Action document template
  - User journey template
  - Index document template
  - Cross-reference patterns

---

## Related Protocols

- [Architectural Analysis](../software/architectural-analysis.md) - Uses these standards
- [Protocol Validation](../software/protocol-validation.md) - Validates compliance

---

[documentation-style-guide.md]: ./documentation-style-guide.md
[documentation-standards.md]: ./documentation-standards.md
[documentation-structure.md]: ./documentation-structure.md
