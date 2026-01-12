# Schema Integration Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Guidelines for integrating schema validation frameworks with UI components

---

## Overview

This protocol explains how to leverage schema validation frameworks (such as Zod, Yup, Joi, or similar) to automatically configure UI components. Schema metadata can drive automatic column definitions, validation rules, and component configuration.

---

## When to Use

- Integrating schema validation with data grids
- Auto-generating form configurations
- Creating type-safe component props
- Reducing boilerplate in data-driven UIs

---

## Key Concepts

### Schema Validation Frameworks

Common schema validation frameworks include:
- **Zod** - TypeScript-first schema validation
- **Yup** - JavaScript schema validation
- **Joi** - Object schema validation
- **io-ts** - Runtime type system for TypeScript

### Schema Metadata Benefits

Rich metadata in schemas can automatically:
- Generate column definitions with proper labeling
- Determine filter, sort, and search capabilities per field
- Identify primary keys and foreign key relationships
- Configure role-based permissions and field access
- Apply default sorting and search behavior
- Handle internationalization keys
- Manage field visibility and read-only states

---

## Metadata Categories

### Navigation and Relationships
- `x-navigation-key`: Primary key on this model
- `x-navigation-target`: Class name of related model
- `x-query-set`: The entity model this query is associated with

### Display and Labeling
- `x-label`: Field label value
- `x-alt-text`: Alt text/tooltip
- `x-display-name`: Custom display name override
- `x-column-width`: Preferred column width
- `x-format`: Data format (currency, percentage, date, datetime, email, phone)
- `x-hidden`: Hide field from display
- `x-readonly`: Field is read-only

### Search, Filter, and Sort Behavior
- `x-searchable`: Field supports search
- `x-not-filterable`: Field does not support filtering
- `x-not-sortable`: Field does not support sorting
- `x-default-sort`: Default sort field

### Permissions and Access Control
- `x-permissions`: Roles allowed to access/modify

### Type and Formatting
- `x-column-type`: Specific column type
- `x-required`: Field validation requirements
- `x-min`/`x-max`: Numeric constraints
- `x-pattern`: Regex pattern for validation

---

## Implementation Pattern

### Basic Auto-Configuration

```typescript
import { schemaToGridColumns, getSchemaMetadata } from '@/utils/schemaHelper';

// Auto-generate columns from schema
const columns = schemaToGridColumns<IQueryModel>(
  QueryModelSchema,
  {
    excludeFields: ['createdById', 'updatedById'],
    customRenderers: {
      isActive: (value) => <StatusTag value={value} />
    }
  }
);

// Get metadata for configuration
const metadata = getSchemaMetadata(QueryModelSchema);
const primaryKey = metadata.primaryKey;
```

### Schema Helper Utility

```typescript
// Example utility structure
export function schemaToGridColumns<T>(
  schema: Schema<T>,
  options: {
    excludeFields?: string[];
    customRenderers?: Record<string, Function>;
    columnOverrides?: Record<string, Partial<ColumnDef>>;
  }
): ColumnDef<T>[] {
  // Extract fields from schema
  // Apply metadata for labels, widths, sorting
  // Return column definitions
}

export function getSchemaMetadata(schema: Schema): {
  primaryKey: string;
  fields: FieldMetadata[];
  relationships: RelationshipMetadata[];
} {
  // Extract schema-level metadata
}

export function getNumericColumns(schema: Schema): string[] {
  // Return fields with numeric types
}
```

### Using with Data Grid

```typescript
<DataGrid<TFilter, TOrderBy, TSearchQuery, TModel>
  client={clientInstance}
  dataKey={metadata.primaryKey}
  columns={columns}
  numericColumns={getNumericColumns(schema)}
  enableSearch={true}
  enableFilter={true}
  enableSort={true}
/>
```

### Custom Column Overrides

```typescript
const columns = schemaToGridColumns<IQueryModel>(
  QueryModelSchema,
  {
    columnOverrides: {
      entityName: {
        width: '200px',
        header: 'Display Name'
      },
      identifier: {
        width: '150px',
        header: 'ID #'
      }
    }
  }
);
```

---

## Schema Organization

### Query Models (for display)
Used for reading/displaying data:
- `QueryEntityModel` - Read operations
- Include computed fields, aggregations
- May include related data

### Save Models (for forms)
Used for create/update operations:
- `SaveEntityModel` - Write operations
- Include validation rules
- May exclude computed fields

---

## Benefits

### 1. Automatic Configuration
- Column definitions generated from schema
- Filter types determined by data types
- Sortable/filterable flags from metadata
- Primary key identification

### 2. Type Safety
- Full TypeScript integration
- Compile-time type checking
- IntelliSense support
- Runtime validation available

### 3. Maintainability
- Single source of truth for data models
- Schema changes automatically propagate
- Reduced boilerplate code
- Consistent behavior across components

### 4. Rich Metadata
- Searchability flags
- Navigation relationships
- Display formatting hints
- Validation rules

---

## Implementation Strategy

1. **Start with auto-configuration**: Use schema helper for initial setup
2. **Add custom renderers**: Define custom display logic for specific fields
3. **Override as needed**: Use column overrides for fine-tuning
4. **Leverage metadata**: Use schema metadata for advanced features

---

## Troubleshooting

### Schema Not Found
If a schema is not found, check:
1. The schema exists in expected location
2. The interface name mapping is correct
3. The import path is correct

### Missing Metadata
If metadata is missing:
1. Check the schema file for metadata definitions
2. Ensure metadata extraction is working correctly
3. Consider adding custom metadata through schema extensions

### Type Errors
If you encounter type errors:
1. Ensure generic types match between schema and usage
2. Check that interface types are properly imported
3. Verify schema structure matches expected model

---

## Validation Checklist

- [ ] Schema includes all required metadata
- [ ] Column definitions auto-generated correctly
- [ ] Type safety maintained throughout
- [ ] Custom overrides applied where needed
- [ ] Validation rules working correctly
- [ ] Performance acceptable with schema parsing

---

## Related Protocols

- [DataGrid Style Guide](./datagrid-style-guide.md) - Component standards
- [Template Development](./template-development.md) - Code generation

---

## Change Log

- 2026-01-03 v1.0: Initial generalized version (from Zod-specific guide)
