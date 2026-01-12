# Init Setup Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Initialize a new project with protocol-based workflows by gathering project configuration

---

## Trigger Phrases

- "init"
- "setup project"
- "initialize protocols"
- "configure project"

## Overview

This protocol guides the initial setup of a new project by collecting essential configuration values that are used throughout other protocols. It creates a project configuration file that stores these values for consistent use.

## When to Use

- Starting work on a new project
- Cloning a repository without existing configuration
- When `.claude/project-config.json` does not exist
- When project configuration needs to be updated

## Protocol Steps

### Step 1: Check for Existing Configuration

**Actions:**
- [ ] Check if `.claude/project-config.json` exists
- [ ] If exists, offer to update or use existing values

**Commands:**
```bash
# Check for existing config
ls -la .claude/project-config.json 2>/dev/null
```

**If Configuration Exists:**
```
Found existing project configuration:
- Project Name: {name}
- Project Type: {type}
- Last Updated: {date}

Would you like to:
1. Use existing configuration
2. Update configuration
3. Create new configuration (overwrites existing)
```

---

### Step 2: Gather Project Information

**Questions to Ask:**

#### 2.1 Project Name
```
What is the project name?
(Used for documentation headers, branch naming, etc.)

Examples: "MyApp", "CustomerPortal", "DataService"
```

#### 2.2 Project Type
```
What type of project is this?

1. Web Application (React, Angular, Vue, etc.)
2. API/Backend Service (.NET, Node.js, Python, etc.)
3. Desktop Application (WPF, Electron, etc.)
4. Library/Framework
5. Mobile Application
6. Full Stack (Frontend + Backend)
7. Documentation/Analysis Only
8. Other (specify)
```

#### 2.3 Primary Technology Stack
```
What is the primary technology stack?

Examples:
- ".NET 8 / C# / Entity Framework"
- "React / TypeScript / Node.js"
- "Python / FastAPI / PostgreSQL"
- "WPF / .NET Framework 4.8 / SQLite"
```

#### 2.4 Source Control
```
What source control system is used?

1. Git (default)
2. Azure DevOps Git
3. GitHub
4. GitLab
5. Other
```

#### 2.5 Work Item Tracking (Optional)
```
What work item tracking system is used? (Optional)

1. Azure DevOps Boards
2. GitHub Issues
3. Jira
4. Linear
5. None
6. Other
```

#### 2.6 Branch Naming Convention
```
What branch naming convention should be used?

1. features/{id}-{description}, bugs/{id}-{description} (default)
2. feature/{description}, bugfix/{description}
3. {type}/{ticket}-{description}
4. Custom (specify pattern)
```

#### 2.7 Documentation Location
```
Where should documentation be stored?

1. ./docs/ (default)
2. ./documentation/
3. ./.claude/docs/
4. Custom path
```

#### 2.8 Test Location (Optional)
```
Where are tests located? (Optional)

Examples:
- "Tests/" or "tests/"
- "src/__tests__/"
- "{project}.Tests/"
- "test/"
```

---

### Step 3: Create Configuration File

**Actions:**
- [ ] Create `.claude/` directory if not exists
- [ ] Write `project-config.json` with collected values

**Configuration Template:**

```json
{
  "version": "1.0",
  "lastUpdated": "YYYY-MM-DD",
  "project": {
    "name": "{projectName}",
    "type": "{projectType}",
    "technologyStack": "{technologyStack}"
  },
  "sourceControl": {
    "system": "{git|azure-devops|github|gitlab}",
    "mainBranch": "{main|master}",
    "branchPattern": {
      "feature": "features/{id}-{description}",
      "bug": "bugs/{id}-{description}",
      "task": "tasks/{id}-{description}",
      "hotfix": "hotfix/{id}-{description}"
    }
  },
  "workItems": {
    "system": "{azure-devops|github|jira|none}",
    "project": "{projectName}",
    "linkPattern": "#{id}"
  },
  "paths": {
    "documentation": "./docs/",
    "protocols": "./.claude/protocols/",
    "tests": "{testPath}",
    "source": "{sourcePath}"
  },
  "documentation": {
    "rootFiles": ["README.md", "CLAUDE.md", "TODO.md"],
    "namingConvention": "kebab-case"
  }
}
```

---

### Step 4: Create Directory Structure

**Actions:**
- [ ] Create documentation directory
- [ ] Create protocols directory
- [ ] Create initial README files

**Commands:**
```bash
# Create directories
mkdir -p {documentationPath}
mkdir -p .claude/protocols

# Create index files
touch {documentationPath}/README.md
```

**Initial docs/README.md:**
```markdown
# {Project Name} Documentation

## Overview

Documentation for {Project Name}.

## Contents

| Document | Description |
|----------|-------------|
| [Architecture](./architecture/) | System architecture and design |
| [Features](./features/) | Feature documentation |
| [Changes](./changes/) | Change history |

## Quick Links

- [CLAUDE.md](../CLAUDE.md) - AI assistant configuration
- [TODO.md](../TODO.md) - Work tracking
```

---

### Step 5: Create CLAUDE.md Template

**Actions:**
- [ ] Check if CLAUDE.md exists
- [ ] Create or update with project configuration

**CLAUDE.md Template:**

```markdown
# {Project Name} - Claude Code Configuration

## Project Overview

**Name:** {projectName}
**Type:** {projectType}
**Stack:** {technologyStack}

## Directory Structure

```
{projectName}/
├── {sourcePath}/           # Source code
├── {testPath}/             # Tests
├── {documentationPath}/    # Documentation
├── .claude/
│   ├── protocols/          # Development protocols
│   └── project-config.json # Project configuration
└── CLAUDE.md              # This file
```

## Protocols

See `.claude/protocols/` for available development protocols:

| Protocol | Trigger | Purpose |
|----------|---------|---------|
| init-setup | "init" | Project initialization |
| start-work | "start work on {id}" | Begin new work item |
| change-review | "review my changes" | Code review |
| create-test-plan | "create test plan" | Generate test plan |
| lifecycle | "show lifecycle" | View development workflow |

## Branch Naming

| Type | Pattern | Example |
|------|---------|---------|
| Feature | `{featurePattern}` | `features/123-user-auth` |
| Bug | `{bugPattern}` | `bugs/456-login-fix` |
| Task | `{taskPattern}` | `tasks/789-refactor` |

## Work Item Integration

**System:** {workItemSystem}
**Link Format:** {linkPattern}

## Documentation Standards

- All docs under `{documentationPath}/`
- Protocols in `.claude/protocols/`
- Root files: README.md, CLAUDE.md, TODO.md only

---

*Generated by init-setup protocol v1.0*
*Last Updated: {date}*
```

---

### Step 6: Provide Summary

**Output:**
```markdown
# Project Initialized

**Project:** {projectName}
**Type:** {projectType}
**Stack:** {technologyStack}

## Created Files

- `.claude/project-config.json` - Project configuration
- `{documentationPath}/README.md` - Documentation index
- `CLAUDE.md` - AI assistant configuration (if created/updated)

## Created Directories

- `.claude/protocols/` - Development protocols
- `{documentationPath}/` - Documentation root

## Next Steps

1. **Review CLAUDE.md** - Verify project configuration
2. **Copy protocols** - Add desired protocols to `.claude/protocols/`
3. **Start working** - Use "start work on {id}" to begin

## Available Protocols

After copying protocols, you can use:

| Command | Protocol | Purpose |
|---------|----------|---------|
| `start work on {id}` | start-work.md | Begin new work |
| `review my changes` | change-review.md | Code review |
| `create test plan` | create-test-plan.md | Test planning |
| `create documentation` | create-documentation.md | Feature docs |
| `show lifecycle` | lifecycle.md | View workflow |

## Configuration Location

Project configuration stored in:
`.claude/project-config.json`

To update configuration, run "init" or "setup project" again.
```

---

## Validation Checklist

- [ ] Project name collected
- [ ] Project type identified
- [ ] Technology stack documented
- [ ] Branch naming configured
- [ ] Documentation path set
- [ ] `.claude/project-config.json` created
- [ ] Directory structure created
- [ ] CLAUDE.md updated/created
- [ ] Summary provided

## Common Issues

| Issue | Cause | Resolution |
|-------|-------|------------|
| Permission denied | No write access | Check file permissions |
| Directory exists | Previous init | Offer to update or overwrite |
| Git not initialized | New project | Run `git init` first |

## Related Protocols

- [start-work.md](./start-work.md) - Begin work on item
- [lifecycle.md](./lifecycle.md) - Development workflow
- [documentation-standards.md](./documentation-standards.md) - Doc organization

---

## Change Log

- 2026-01-03 v1.0: Initial protocol created
