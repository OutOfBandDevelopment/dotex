# Protocol Validation Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Ensure all protocols meet quality standards before use

---

## Overview

This protocol guides the validation of new and existing protocols to ensure they meet quality standards, are complete, and can be successfully executed.

## When to Use

- After creating a new protocol
- After completing a task using a protocol
- When a protocol seems inadequate during use
- During periodic protocol reviews

---

## Procedure

### Step 1: Structural Validation

Verify the protocol has all required sections.

**Required Sections:**

| Section | Purpose | Required |
|---------|---------|----------|
| Title | Protocol name | Yes |
| Metadata | Version, date, purpose | Yes |
| Overview | What the protocol does | Yes |
| When to Use | Trigger conditions | Yes |
| Prerequisites | What's needed before starting | Yes |
| Procedure | Step-by-step instructions | Yes |
| Validation Checklist | How to verify completion | Yes |
| Common Issues | Troubleshooting guide | Yes |
| Related Protocols | Cross-references | Yes |
| Change Log | Version history | Yes |

**Validation:**
- [ ] All required sections present
- [ ] Sections in logical order
- [ ] Metadata complete (version, date, purpose)

---

### Step 2: Content Quality Validation

**Clarity Check:**
- [ ] Instructions are unambiguous
- [ ] Technical terms are defined or commonly understood
- [ ] Steps are actionable (start with verbs)
- [ ] Examples provided for complex steps

**Completeness Check:**
- [ ] All steps necessary to complete the task are included
- [ ] No assumed knowledge without prerequisites
- [ ] Edge cases addressed
- [ ] Error handling documented

**Accuracy Check:**
- [ ] File paths are correct
- [ ] Commands are valid
- [ ] Code examples compile/run
- [ ] Cross-references are valid

---

### Step 3: Usage Validation

**Execute Protocol:**
- [ ] Follow the protocol exactly as written
- [ ] Note any ambiguities or missing steps
- [ ] Verify all validation checkpoints
- [ ] Document any deviations required

**Test All Paths:**
- [ ] Happy path works
- [ ] Error handling works
- [ ] Edge cases handled
- [ ] Prerequisites are sufficient

---

### Step 4: Style Compliance

**Formatting:**
- [ ] Markdown renders correctly
- [ ] Code blocks have language identifiers
- [ ] Tables are properly formatted
- [ ] Headers use proper hierarchy

**Standards:**
- [ ] Follows documentation style guide
- [ ] Consistent terminology
- [ ] Consistent formatting
- [ ] No typos or grammatical errors

**Metadata:**
- [ ] Version follows semantic versioning
- [ ] Date in YYYY-MM-DD format
- [ ] Purpose is one clear sentence
- [ ] Change log is current

---

### Step 5: Cross-Reference Validation

**Links:**
- [ ] All internal links work
- [ ] Related protocols exist
- [ ] No broken references
- [ ] Circular dependencies documented

**Dependencies:**
- [ ] Prerequisite protocols listed
- [ ] Dependent protocols listed
- [ ] Order of execution clear

---

### Step 6: Version Update

**When to Update Version:**

| Change Type | Version Update | Example |
|-------------|----------------|---------|
| Major workflow changes | X.0 | 1.0 -> 2.0 |
| New sections/steps | X.Y | 1.0 -> 1.1 |
| Clarifications/typos | X.Y | 1.0 -> 1.1 |
| Bug fixes | X.Y | 1.0 -> 1.1 |

**Update Checklist:**
- [ ] Version number incremented
- [ ] Last Updated date changed
- [ ] Change log entry added
- [ ] Breaking changes documented

---

### Step 7: Registry Update

If this is a new protocol or significant update:

- [ ] Update protocols README.md
- [ ] Add to protocol registry table
- [ ] Update any dependent protocols
- [ ] Notify team of changes

---

## Validation Checklist

**Structure:**
- [ ] All required sections present
- [ ] Logical section order
- [ ] Complete metadata

**Content:**
- [ ] Clear, unambiguous instructions
- [ ] Complete step coverage
- [ ] Accurate examples and references

**Usability:**
- [ ] Successfully executed end-to-end
- [ ] All paths tested
- [ ] Prerequisites sufficient

**Style:**
- [ ] Markdown renders correctly
- [ ] Consistent formatting
- [ ] No errors

**Integration:**
- [ ] All links work
- [ ] Registry updated
- [ ] Related protocols updated

---

## Common Issues

| Issue | Cause | Resolution |
|-------|-------|------------|
| Missing steps | Assumed knowledge | Add explicit instructions |
| Broken links | File moved/renamed | Update references |
| Outdated examples | Code changed | Update code samples |
| Unclear instructions | Jargon or ambiguity | Simplify language |
| Missing prerequisites | Undocumented dependencies | Add prerequisites |

---

## Review Schedule

| Type | Frequency | Trigger |
|------|-----------|---------|
| New protocols | Immediately | Creation |
| Active protocols | Quarterly | Calendar |
| After usage issues | As needed | Problem report |
| All protocols | Annually | Comprehensive review |

---

## Related Protocols

- [Architectural Analysis](./architectural-analysis.md)
- [Documentation Style Guide](../documentation/documentation-style-guide.md)
- [Comprehensive Analysis Checklist](./comprehensive-analysis-checklist.md)

---

## Change Log

- 2026-01-03 v1.0: Generalized for any project
- 2026-01-02 v1.0: Initial protocol validation protocol
