# Protocol Trigger Reference

**Last Updated:** 2026-01-21

This document provides a quick reference for all protocol trigger phrases. When the user says these phrases, Claude should automatically run the corresponding protocol.

---

## Documentation Protocols

### Configuration Documentation
**Protocol:** `.claude/protocols/documentation/configuration-documentation.md`
**Triggers:**
- "find all my configurations"
- "document configurations"
- "create configuration reference"

**Output:** `CONFIGURATION_SETTINGS.md`

**What it does:** Discovers and documents all IOptions classes, IConfiguration keys, environment variables, and connection strings across the framework.

---

### Change Documentation Archival
**Protocol:** `.claude/protocols/documentation/change-documentation-archival.md`
**Triggers:**
- "clean up your task list"
- "archive completed work"
- When TODO files exceed 400 lines

**Output:** `docs/changes/{feature-name}-{date}.md`

**What it does:** Archives completed work from TODO files into change documentation, reducing context overhead by 30-50%.

---

## Testing Protocols

### Integration Test Maintenance
**Protocol:** `.claude/protocols/software/integration-test-maintenance.md`
**Triggers:**
- "add new container"
- "add integration test service"
- "setup {service} for integration testing"

**Updates:**
- `docker-compose.integration-tests.yml`
- `nginx/nginx.conf` (if web UI)
- `nginx/html/index.html` (dashboard)
- `src/.runsettings` (test parameters)
- `TEST_VARIABLES.md`
- `containers/testing/README.md`

**What it does:** Complete checklist for adding new Docker services to integration test infrastructure, ensuring all related files are updated consistently.

---

## Software Analysis Protocols

### Incoming Codebase Comparison
**Protocol:** `.claude/protocols/software/incoming-codebase-comparison.md`
**Triggers:**
- "compare {incoming-project} with main"
- "analyze incoming code"
- "migration analysis for {project}"

**Output:** Phase-by-phase comparison and migration plan

**What it does:** Systematic comparison of incoming code with main codebase, identifying matches, gaps, and migration requirements.

---

### Architectural Analysis
**Protocol:** `.claude/protocols/software/architectural-analysis.md`
**Triggers:**
- "analyze architecture of {component}"
- "document architecture"

**Output:** Comprehensive architecture documentation

**What it does:** Systematic analysis and documentation of component architecture, patterns, and design decisions.

---

## Code Generation Protocols

### Template Development
**Protocol:** `.claude/protocols/software/template-development.md`
**Triggers:**
- "create template for {feature}"
- "generate code template"

**Output:** Reusable code templates

**What it does:** Creates parameterized code templates for common patterns and features.

---

## Quick Reference Table

| Trigger Phrase | Protocol | Output |
|----------------|----------|--------|
| "find all my configurations" | configuration-documentation.md | CONFIGURATION_SETTINGS.md |
| "clean up your task list" | change-documentation-archival.md | docs/changes/*.md |
| "add new container" | integration-test-maintenance.md | Multiple file updates |
| "compare X with main" | incoming-codebase-comparison.md | Comparison analysis |
| "analyze architecture of X" | architectural-analysis.md | Architecture docs |
| "create template for X" | template-development.md | Code template |

---

## Protocol Discovery

### Finding All Available Protocols

```bash
# List all protocol files
find .claude/protocols -name "*.md" -type f

# Search for protocol triggers
grep -r "Trigger" .claude/protocols --include="*.md"

# Find protocol descriptions
grep -A 3 "^## Purpose" .claude/protocols/**/*.md
```

### Protocol Directory Structure

```
.claude/protocols/
├── PROTOCOL_TRIGGERS.md (this file)
├── software/
│   ├── architectural-analysis.md
│   ├── incoming-codebase-comparison.md
│   ├── integration-test-maintenance.md
│   ├── security-audit.md
│   ├── protocol-validation.md
│   └── template-development.md
├── documentation/
│   ├── documentation-style-guide.md
│   ├── documentation-standards.md
│   ├── change-documentation-archival.md
│   ├── configuration-documentation.md
│   └── template-swagger-documentation.md
└── component/
    ├── schema-integration.md
    └── datagrid-style-guide.md
```

---

## Adding New Protocols

When creating a new protocol:

1. **Add trigger section** to protocol file:
   ```markdown
   **Trigger Phrases:** "phrase 1", "phrase 2", "phrase 3"
   ```

2. **Update this file** with new trigger

3. **Update CLAUDE.md** in the Available Protocols section

4. **Test the trigger** by using the phrase in conversation

---

## Protocol Best Practices

### For Protocol Authors

- **Be Specific:** Use unique, descriptive trigger phrases
- **Be Consistent:** Follow existing protocol format
- **Be Clear:** Document expected output and side effects
- **Be Complete:** Include examples and troubleshooting

### For Protocol Users (Claude)

- **Match Exactly:** Look for trigger phrases, not just similar words
- **Run Completely:** Follow all steps in the protocol
- **Update Documentation:** Keep CLAUDE.md and this file current
- **Validate Output:** Check that all expected files are created/updated

---

## Related Documentation

- [CLAUDE.md](../../CLAUDE.md) - Main development guide
- [Protocol Validation](.claude/protocols/software/protocol-validation.md) - Quality assurance for protocols
- [Documentation Standards](.claude/protocols/documentation/documentation-standards.md) - Documentation file organization

---

**Maintained By:** Protocol Development Team
**Review Frequency:** When new protocols are added
