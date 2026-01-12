# Template Development Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Workflow for developing, fixing, and enhancing code generation templates

---

## Overview

This protocol defines the workflow for template-based code generation systems. It applies to any template engine (Handlebars, EJS, Jinja, Mustache, etc.) used for generating code files.

---

## Core Principle

**NEVER modify generated code directly. Always modify the template that generates the code.**

Generated files are overwritten during the build process. Any direct modifications will be lost.

---

## When to Use

- Developing new code generation templates
- Fixing bugs in generated output
- Enhancing generated code patterns
- Adding new fields or features to generated files

---

## Template Locations (Example Structure)

| Template Type   | Location Pattern                         |
|-----------------|------------------------------------------|
| Schemas         | `templates/api/{Entity}Schema.{ext}.hbs` |
| Components      | `templates/components/_Entity_/`         |
| Pages           | `templates/pages/_Entity_/`              |
| Routes          | `templates/pages/Routes.{ext}.hbs`       |
| Common Helpers  | `templates/_common/`                     |

---

## Procedure

### Step 1: Understand the Request

Before making any changes:

1. **Clarify requirements** with the requester
   - What functionality is needed?
   - What is the expected output?
   - Are there existing patterns to follow?

2. **Identify affected templates**
   - Which template(s) generate the code in question?
   - What data sources feed into the template (API spec, metadata)?

3. **Review existing generated output**
   - Read the currently generated file to understand the baseline
   - Identify what needs to change

### Step 2: Analyze the Data Source

Templates typically consume data from specification files (OpenAPI, JSON Schema, etc.). Before modifying templates:

1. **Check source structure**
   ```bash
   # View model definitions in spec file
   grep -A 50 '"ModelName":' swagger.json
   ```

2. **Understand available properties**
   - Standard properties: `type`, `properties`, `required`, `format`
   - Custom extensions: `x-label`, `x-navigation-target`, etc.

3. **Document new metadata** if adding custom extensions
   - Update metadata guide documentation

### Step 3: Modify the Template

#### Template Syntax Guidelines (Handlebars Example)

```handlebars
{{!-- Safe comment syntax --}}

{{!-- String property --}}
{{#if property}}
    'property': '{{property}}',
{{/if}}

{{!-- Array property --}}
{{#if arrayProperty}}
    'arrayProperty': [{{#each arrayProperty as |item|}}'{{item}}',{{/each}}],
{{/if}}

{{!-- Conditional logic --}}
{{#if condition}}
    {{!-- true branch --}}
{{else}}
    {{!-- false branch --}}
{{/if}}

{{!-- Iteration --}}
{{#each items as |item|}}
    {{item.name}}: {{item.value}}
{{/each}}
```

#### Common Pitfalls

1. **Comment syntax**: Follow template engine conventions
2. **Special characters**: Handle quotes, newlines, special chars properly
3. **Type detection**: Templates may not easily distinguish data types
4. **Whitespace control**: Use whitespace control carefully for output formatting
5. **Context passing**: Child templates may need explicit context parameters

### Step 4: Test the Changes

1. **Generate the output**
   ```bash
   # Run your generation command
   ./generate.sh
   ```

2. **Review generated files**
   - Compare with expected output
   - Check for syntax errors
   - Verify all edge cases

3. **Build verification**
   ```bash
   # Verify generated code compiles/runs
   npm run build    # or equivalent
   ```
   - Must complete without errors
   - Check for runtime warnings

4. **Manual testing**
   - Test the feature in the running application
   - Verify expected behavior

### Step 5: Update Documentation

After successful implementation:

1. **Update relevant docs**
   - Metadata guide for new extensions
   - Feature-specific documentation
   - Architecture docs if patterns changed

2. **Add inline comments** to templates for complex logic

3. **Update changelog** with:
   - Date and description
   - Files modified
   - Before/after examples

---

## Collaboration Pattern

### When Working with Requesters

1. **Initial discussion**
   - Understand the full scope of the request
   - Identify all affected templates and generated files
   - Agree on expected output format

2. **Iterative development**
   - Make incremental changes
   - Share generated output for review
   - Incorporate feedback before finalizing

3. **Verification**
   - Confirm generated output matches expectations
   - Validate build passes
   - Test functionality in application

### Communication Checkpoints

| Stage                 | Action                                  |
|-----------------------|-----------------------------------------|
| Before starting       | Confirm understanding of requirements   |
| After template changes| Show generated output sample            |
| After testing         | Report build status and test results    |
| After completion      | Summarize changes and updated docs      |

---

## Example: Adding New Schema Metadata

### Scenario
Add support for `x-custom-property` array in schema metadata.

### Steps

1. **Update source spec** (or verify property exists)
   ```json
   {
     "QueryExampleModel": {
       "x-custom-property": ["value1", "value2"]
     }
   }
   ```

2. **Modify template**
   ```handlebars
   {{#if definition.x-custom-property}}
       'x-custom-property': [{{#each definition.x-custom-property as |val|}}'{{val}}',{{/each}}],
   {{/if}}
   ```

3. **Generate and verify**
   ```bash
   ./generate.sh
   # Check generated file shows:
   # 'x-custom-property': ['value1','value2',],
   ```

4. **Build and test**
   ```bash
   npm run build
   ```

5. **Document in metadata guide**

---

## Troubleshooting

### Template Parsing Errors

**Error**: "Reached end of template in the middle of a comment"
- **Cause**: Improper comment syntax
- **Fix**: Use correct comment delimiters

**Error**: Unexpected output format
- **Cause**: Template treating arrays as strings
- **Fix**: Use explicit iteration for array properties

### Build Errors After Generation

**Error**: Compilation fails
- **Check**: Generated syntax is valid
- **Check**: All imports are correct
- **Check**: Property names match expected interfaces

**Error**: Runtime errors
- **Check**: Generated code logic is correct
- **Check**: Null/undefined handling

---

## Registry Pattern Integration

When templates generate code that uses registries:

1. **Renderer Registry** - Template uses metadata for rendering functions
2. **Action Registry** - Template uses metadata for action arrays
3. **Component Registry** - Generates lazy-loaded component mappings

---

## Validation Checklist

Before marking template work complete:

- [ ] Template changes are in the correct file(s)
- [ ] Generated output matches expected format
- [ ] Build passes without errors
- [ ] Feature works in running application
- [ ] Documentation updated
- [ ] Changes logged in changelog
- [ ] No direct modifications to generated files

---

## Related Protocols

- [Template and Swagger Documentation](./template-swagger-documentation.md) - Documentation maintenance
- [Architectural Analysis](./architectural-analysis.md) - System analysis

---

## Change Log

- 2026-01-03 v1.0: Initial generalized version
