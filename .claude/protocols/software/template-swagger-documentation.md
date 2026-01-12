# Template and Swagger Documentation Maintenance Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Procedures for maintaining documentation related to code generation templates and OpenAPI extensions

---

## Overview

This protocol defines procedures for reviewing and maintaining documentation related to code generation templates, OpenAPI/Swagger extensions, and template engine features.

---

## When to Use

- Reviewing template documentation accuracy
- Adding new metadata attributes
- Updating template files
- Synchronizing documentation with implementation

---

## Key Locations

### Source Files

| Component             | Location Pattern                          | Description                      |
|-----------------------|-------------------------------------------|----------------------------------|
| Template Source       | `templates/`                              | Template files (e.g., .hbs)      |
| Template Output       | `generated/` or `publish/`                | Generated files                  |
| API Spec              | `docs/swagger.json` or `openapi.yaml`     | OpenAPI spec with extensions     |
| Entity Models         | `src/Models/`                             | Models with metadata attributes  |
| Metadata Attributes   | `src/Common/Metadata/`                    | Attribute definitions            |
| Schema Filters        | `src/Common/SwaggerGen/`                  | Swagger extension processors     |
| Template Helpers      | `src/Common/Helpers/`                     | Custom template helpers          |
| Template Engine CLI   | `src/Tools/TemplateEngine/`               | Code generation tool             |

### Documentation Files

| Document              | Location                           | Purpose                          |
|-----------------------|------------------------------------|----------------------------------|
| Project Memory        | `/CLAUDE.md`                       | Project guidelines               |
| Swagger Extensions    | `docs/swagger-extensions.md`       | OpenAPI extension reference      |
| Template Helpers      | `docs/template-helpers.md`         | Template helper documentation    |
| Template README       | `templates/README.md`              | Template usage guide             |

---

## Review Procedures

### 1. Template Review Protocol

When reviewing or updating templates:

```bash
# Step 1: Verify current template structure
find templates -name "*.hbs" | wc -l

# Step 2: Run template generation to verify output
./generate.sh

# Step 3: Compare generated output
diff -r generated/ <previous_output>
```

**Key Template Files to Review:**
- Common header templates
- Type mapping templates
- Entity definition templates
- Component templates

**Template Variable Inheritance:**
- Child templates may NOT automatically inherit parent context
- Must explicitly pass variables to child templates
- Root context provides access to full spec document

### 2. Swagger Extension Review Protocol

When reviewing or updating swagger extensions:

```bash
# Step 1: Count metadata attributes
find src -name "*Attribute.cs" -path "*/Metadata/*" | wc -l

# Step 2: Extract extension names from swagger.json
cat swagger.json | grep -o '"x-[^"]*"' | sort | uniq

# Step 3: Verify schema filter processes all attributes
cat src/Common/SwaggerGen/MetadataSchemaFilter.cs
```

**Extension Categories:**
1. **Display & UI**: x-label, x-alt-text, x-hidden-column, x-column-priority
2. **Navigation**: x-navigation-key, x-navigation-target, x-navigation-description
3. **Form Fields**: x-placeholder, x-help-text, x-default-value, x-validation-pattern
4. **Data Operations**: x-quick-filter, x-export-exclude, x-bulk-actions
5. **Entity Config**: x-query-model, x-save-model, x-not-creatable
6. **Search**: x-default-sort, x-filterable, x-searchable, x-not-sortable
7. **Operation**: x-query-set, x-query-action, x-permissions

### 3. Entity Model Review Protocol

When reviewing entity models:

```bash
# Step 1: List all Query models
find src -name "Query*.cs" -path "*/Models/*"

# Step 2: Check model attributes
grep -l "NavigationTarget\|Label" src/Models/*.cs

# Step 3: Verify swagger output for specific model
cat swagger.json | jq '.components.schemas["ModelName"]'
```

**Model Attribute Checklist:**
- [ ] `[Label("...")]` - Entity display name
- [ ] `[SaveModel(typeof(...))]` - Save model mapping
- [ ] `[NavigationKey]` - Primary key property
- [ ] `[DisplayValue]` - Display text property
- [ ] `[NavigationTarget(...)]` - Foreign key relationships

### 4. Template Engine Review Protocol

When reviewing template engine:

```bash
# Step 1: Check CLI options
cat src/Tools/TemplateEngine/Options.cs

# Step 2: Review available helpers
find src/Common/Helpers -name "*Descriptor.cs"

# Step 3: Run template with verbose output
./template-engine --input swagger.json --output test.md --Template Test.md
```

---

## Documentation Update Checklist

When updating documentation, ensure:

### Project Documentation Updates
- [ ] Update metadata attribute count if changed
- [ ] Document new OpenAPI extensions
- [ ] Add new template files to structure
- [ ] Record implementation dates
- [ ] Update version history

### swagger-extensions.md Updates
- [ ] Add new attribute documentation sections
- [ ] Update Quick Reference Table
- [ ] Add usage examples for new extensions
- [ ] Update Integration Notes
- [ ] Add version history entry

### Template README Updates
- [ ] Document new template files
- [ ] Update OpenAPI Extension Support table
- [ ] Add troubleshooting entries
- [ ] Update version history

### Template Helpers Updates
- [ ] Document new helpers
- [ ] Add usage examples
- [ ] Update architecture section if needed

---

## Validation Commands

### Verify Template Generation

```bash
# Clean and regenerate
rm -rf generated/
./generate.sh

# Verify output
find generated -name "*.ts" | wc -l
```

### Verify Swagger Extensions

```bash
# Regenerate swagger.json
dotnet build src/API
dotnet run --project src/API
curl http://localhost:5000/swagger/all/swagger.json > swagger.json

# Verify extension count
cat swagger.json | grep -o '"x-[^"]*"' | sort | uniq -c | sort -rn
```

---

## Common Issues

### Issue: Template variable not available

**Symptom:** `@def.x-some-extension` returns empty/undefined

**Cause:** Parent template didn't pass context to child template

**Fix:** Add context parameter to template call:
```handlebars
{{~call template="components/Entity/Component.tsx"
        name=(concat "components/" @entityName "/Component")
        def=@def
        }}
```

### Issue: New attribute not appearing in swagger

**Symptom:** Attribute on model not generating x-extension

**Cause:** Attribute not processed by schema filter

**Fix:**
1. Ensure attribute implements required interface
2. Verify schema filter processes the attribute type
3. Check attribute target (class vs property level)

### Issue: Generated code has type errors

**Symptom:** Generated files have compilation errors

**Cause:** Template schema mapping issue

**Fix:** Review type mapping template for correct mappings

---

## Sync Procedures

### Sync Templates to Project

```bash
./sync-templates.sh
```

### Sync Generated Output

```bash
./sync-generated.sh
```

---

## Validation Checklist

- [ ] All templates documented
- [ ] All extensions documented
- [ ] Schema filter processes all attributes
- [ ] Generated output compiles
- [ ] Documentation matches implementation

---

## Related Protocols

- [Template Development](./template-development.md) - Template workflow
- [Architectural Analysis](./architectural-analysis.md) - System analysis
- [Documentation Standards](../documentation/documentation-standards.md) - File organization

---

## Change Log

- 2026-01-03 v1.0: Initial generalized version
