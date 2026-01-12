# Architectural Analysis Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Standardized procedure for analyzing and documenting project architecture

---

## Overview

This protocol guides the systematic analysis of a project's architecture, ensuring consistent documentation, pattern identification, and cross-project insights. It produces structured documentation following the style guide while capturing both technical and organizational details.

## When to Use

- Initial architectural analysis of a project
- Periodic architecture reviews (quarterly recommended)
- After major architectural changes
- When onboarding new team members
- When identifying cross-project patterns

## Prerequisites

- [ ] Access to project repository
- [ ] Read access to source code
- [ ] [Documentation Style Guide Protocol](../documentation/documentation-style-guide.md) reviewed
- [ ] PlantUML rendering tools available
- [ ] Git command-line access

---

## Procedure

### Step 1: Capture Project Metadata

Record current state of the project for version tracking.

```bash
# Capture metadata
echo "Project: $(basename $(pwd))"
echo "Branch: $(git rev-parse --abbrev-ref HEAD)"
echo "Commit: $(git rev-parse HEAD)"
echo "Short Hash: $(git rev-parse --short HEAD)"
echo "Last Commit Date: $(git log -1 --format=%cd --date=short)"
```

**Validation:**
- [ ] Branch name captured
- [ ] Full commit hash captured
- [ ] Short hash (7 chars) captured
- [ ] Last commit date captured

---

### Step 2: Create Documentation Directory

Create documentation directory for the project.

```bash
# Create project directory
mkdir -p docs/{ProjectName}

# Create standard documentation files
touch docs/{ProjectName}/architecture.md
touch docs/{ProjectName}/patterns.md
touch docs/{ProjectName}/README.md
```

**Validation:**
- [ ] Project directory created under `docs/`
- [ ] Three standard files created (architecture.md, patterns.md, README.md)

---

### Step 3: Initial Discovery - Project Structure

Explore the project to understand its organization.

**3.1: Identify Build System**

Look for solution files, project files, package management.

**3.2: Identify Key Directories**

Map the top-level structure (src/, test/, docs/, etc.)

**3.3: Identify Documentation**

Find existing README.md, CLAUDE.md, and other documentation.

**Validation:**
- [ ] Build system identified
- [ ] Project count estimated
- [ ] Directory structure mapped
- [ ] Existing documentation cataloged

---

### Step 4: Analyze Architecture

Perform systematic architectural analysis.

**4.1: Technology Stack**

Document in `technologies.md`:

```markdown
# Technologies - [ProjectName]

**Last Updated:** YYYY-MM-DD
**Reference:** `{ProjectName}` @ `{branch}` (`{commit-hash}`)

## Programming Languages
- [Language] [Version]

## Frameworks
- [Framework] [Version]

## Key Libraries
| Library | Version | Purpose |
|---------|---------|---------|
| [Name] | [Ver] | [Purpose] |

## Build Tools
- [Tool] [Version]

## Testing Frameworks
- [Framework] [Version]

## Data Storage
- [Database/Format] [Version]

## UI Framework
- [Framework] [Version]
```

**4.2: System Architecture**

Identify architectural layers and components:
- Presentation layer (UI)
- Application layer (business logic)
- Domain layer (domain models)
- Infrastructure layer (data access, external systems)

**4.3: Key Design Decisions**

Look for:
- Dependency injection setup
- Configuration management
- Error handling strategies
- Logging approach
- Testing strategy

**Validation:**
- [ ] Technology stack documented
- [ ] Architectural layers identified
- [ ] Key libraries cataloged
- [ ] Design decisions noted

---

### Step 5: Create C4 Context Diagram

Add C4 Context diagram directly in the architecture markdown file.

```markdown
## System Context

```plantuml
@startuml
'title: C4 Context Diagram - [ProjectName]
'version: 1.0
'date: YYYY-MM-DD
'reference: {ProjectName} @ {branch} ({commit-hash})

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

LAYOUT_WITH_LEGEND()

'Define primary actors/users
Person(user, "[User Role]", "[Description]")

'Define this system
System(system, "[ProjectName]", "[Purpose]")

'Define external systems
System_Ext(external1, "[External System]", "[Description]")

'Define relationships
Rel(user, system, "[Action]", "[Protocol/Tech]")
Rel(system, external1, "[Action]", "[Protocol/Tech]")

@enduml
```
```

**Validation:**
- [ ] Diagram embedded in architecture file
- [ ] All external dependencies identified
- [ ] User actors defined
- [ ] Relationships labeled with protocols/technologies

---

### Step 6: Create C4 Container Diagram

Add C4 Container diagram showing major containers.

**Validation:**
- [ ] Major containers identified (applications, services, databases)
- [ ] Technology stack labeled for each container
- [ ] Inter-container relationships documented

---

### Step 7: Identify and Document Patterns

Analyze code for patterns and practices.

**7.1: Code Organization Patterns**

Look for common folder structures (Models, ViewModels, Views, Services, etc.)

**7.2: Design Patterns**

Look for evidence of:
- Repository pattern
- Factory pattern
- Strategy pattern
- Observer/Event patterns
- MVVM pattern (for UI apps)
- Dependency Injection

**7.3: Testing Patterns**

Identify testing frameworks and organization.

Document findings in `patterns.md`:

```markdown
# Patterns and Practices - [ProjectName]

**Last Updated:** YYYY-MM-DD
**Reference:** `{ProjectName}` @ `{branch}` (`{commit-hash}`)

## Pattern Name: [Pattern Name]

**Category:** [Architectural | Design | Code Organization | Testing]
**Confidence:** [High | Medium | Low]
**Evidence:** [File references]

### Intent
[What problem does this solve?]

### Implementation
[How is it implemented? Include code references with line numbers]

### Example
```language
[Code snippet with file reference]
```

### Benefits
- [Benefit 1]

### Drawbacks
- [Drawback 1]
```

**Validation:**
- [ ] At least 3 patterns identified and documented
- [ ] Code examples include file references
- [ ] Confidence level assessed

---

### Step 8: Complete Architecture Documentation

Fill out the complete architecture file with all sections.

Required sections:
1. Overview
2. System Context (C4 diagram)
3. Container Architecture (C4 diagram)
4. Component Breakdown
5. Data Architecture
6. Key Design Decisions
7. Integration Points
8. Testing Architecture
9. Build and Deployment
10. Related Documentation

**Validation:**
- [ ] All sections completed
- [ ] Diagrams embedded and rendering
- [ ] Component breakdown includes file references
- [ ] Integration points documented

---

### Step 9: Create Project Summary

Create concise `README.md` for quick reference.

```markdown
# [ProjectName] - Architecture Analysis

**Last Updated:** YYYY-MM-DD
**Reference:** `{ProjectName}` @ `{branch}` (`{commit-hash}`)

## Overview
[One paragraph project description]

## Purpose
[What does this project do? What domain does it serve?]

## Key Technologies
- [Technology 1]
- [Technology 2]

## Architecture
- **Type:** [Desktop App | Service | Library | Web App]
- **UI Framework:** [WPF | React | None]
- **Data Storage:** [SQL Server | SQLite | None]
- **Pattern:** [MVVM | Layered | etc.]

## Integration Points
- [System 1]: [Purpose]
- [System 2]: [Purpose]

## Quick Links
- [Architecture Details](./architecture.md)
- [Patterns and Practices](./patterns.md)
- [Technology Stack](./technologies.md)

## Key Findings
- [Important finding 1]
- [Important finding 2]

## Change Log
- YYYY-MM-DD v1.0: Initial analysis
```

**Validation:**
- [ ] Overview is concise (one paragraph)
- [ ] Key technologies listed
- [ ] Integration points identified
- [ ] Links to detailed documentation work

---

### Step 10: Cross-Reference and Validate

Ensure consistency and completeness.

**Validation Checklist:**
- [ ] README links to all detailed docs
- [ ] Architecture.md links to diagrams
- [ ] Patterns.md references architecture.md where appropriate
- [ ] Reference version consistent across all files
- [ ] All files have **Last Updated** date
- [ ] Follows [Documentation Style Guide](../documentation/documentation-style-guide.md)

---

## Validation Checklist

Complete analysis validation:

- [ ] Project metadata captured (branch, commit, date)
- [ ] Directory structure created
- [ ] Initial discovery completed
- [ ] Technology stack documented
- [ ] C4 Context diagram created and validated
- [ ] C4 Container diagram created and validated
- [ ] Patterns identified and documented (minimum 3)
- [ ] Architecture.md completed
- [ ] README.md created
- [ ] All cross-references validated
- [ ] Diagrams render without errors
- [ ] Metadata consistent across all files
- [ ] Style guide compliance verified

---

## Common Issues

| Issue | Cause | Resolution |
|-------|-------|------------|
| Diagram won't render | PlantUML syntax error | Validate syntax at plantuml.com/plantuml |
| Can't determine architecture | Complex/legacy code | Focus on entry points and public interfaces |
| Too many patterns identified | Over-classification | Focus on significant, repeated patterns |
| Missing documentation in project | No existing docs | Analyze code structure and naming conventions |

---

## Related Protocols

- [Documentation Style Guide](../documentation/documentation-style-guide.md)
- [Comprehensive Analysis Checklist](./comprehensive-analysis-checklist.md)
- [Protocol Validation](./protocol-validation.md)

---

## Change Log

- 2026-01-03 v1.0: Generalized for any project
- 2026-01-02 v1.0: Initial architectural analysis protocol created
