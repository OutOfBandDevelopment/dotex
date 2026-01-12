# DataGrid Style Guide

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Standards for DataGrid component usage in applications

---

## Overview

This guide establishes standards for DataGrid component usage, providing consistent patterns for tabular data display.

---

## Design Principles

1. **Modern UI**: Consistent styling and behavior
2. **Type Safety**: Full TypeScript integration
3. **Feature Completeness**: Advanced filtering, bulk selection, column management
4. **Performance**: Server-side operations with optimized rendering
5. **Accessibility**: Built-in accessibility standards

---

## Required Props Configuration

### Mandatory Props

```typescript
<DataGrid<TData, TClient, TQueryModel>
  title="[Page Title]"                    // Display title
  clientClass={ClientClass}               // API client constructor
  queryModelClass={QueryModelClass}       // Query model constructor
  columns={columns}                       // Column definitions
  numericColumns={numericColumns}         // Array of numeric column IDs
  enableSearch={true}                     // Global search functionality
  enableFilter={true}                     // Column filtering
  enableSort={true}                       // Multi-column sorting
  enableColumnVisibility={true}           // Column show/hide controls
  initialPageSize={20}                    // Default page size
  className="[page-name]-grid"            // Component-specific CSS class
/>
```

### Optional Props

```typescript
// When create functionality is available
createButtonText="Create [Entity]"       // Button text
onCreateClick={handleCreate}             // Create handler

// Advanced configurations
searchPlaceholder="Search [entities]..."  // Custom search placeholder
lockedFilters={lockedFilters}            // Pre-applied non-removable filters
initialFilter={initialFilter}            // Default filter state
onDataFetch={onDataFetch}                // Data fetch callback
```

---

## Column Definition Standards

### Column Ordering

1. **ID Column** - First column (entityId, etc.)
2. **Actions Column** - Second column (edit/view buttons)
3. **Primary Name Column** - Third column (name, title)
4. **Data Columns** - Remaining columns in logical order
5. **Status Column** - Status/active indicator
6. **Audit Columns** - Created/Updated (last columns)

### Column Types

#### 1. ID Column (Always First)
```typescript
columnHelper.accessor("entityId", {
  id: "entityId",
  header: "ID",
  cell: (info) => info.getValue(),
}),
```

#### 2. Actions Column (Always Second)
```typescript
columnHelper.display({
  id: "actions",
  header: "Actions",
  enableSorting: false,
  enableColumnFilter: false,
  cell: (info) => (
    <span className="control_btn">
      <Button onClick={() => handleEdit(info.row.original.entityId!)}>
        <EditIcon />
      </Button>
    </span>
  ),
}),
```

#### 3. Standard Data Column
```typescript
columnHelper.accessor("fieldName", {
  id: "fieldName",
  header: "Display Name",
  cell: (info) => info.getValue() || "N/A",
}),
```

#### 4. Status Badge Column
```typescript
columnHelper.accessor("isActive", {
  id: "isActive",
  header: "Status",
  cell: (info) => (
    <span className={`status ${info.getValue() ? 'active' : 'inactive'}`}>
      {info.getValue() ? 'Active' : 'Inactive'}
    </span>
  ),
}),
```

#### 5. Date Column
```typescript
columnHelper.accessor("createdOn", {
  id: "createdOn",
  header: "Created On",
  cell: (info) => {
    const date = info.getValue();
    return date ? new Date(date).toLocaleDateString() : "N/A";
  },
}),
```

#### 6. Numeric Column
```typescript
columnHelper.accessor("count", {
  id: "count",
  header: "Total Count",
  cell: (info) => info.getValue() || 0,
}),
```

### Numeric Columns Array

Always define numeric columns for proper filtering:

```typescript
const numericColumns = [
  'entityId',
  'userCount',
  'total',
  // ... other numeric fields
];
```

---

## Advanced Filtering System

### Filter Types by Column Type

#### String Columns
- **Equal To**: Exact match filtering
- **Not Equal To**: Exclusion filtering
- **In Set**: Multi-value selection

#### Numeric Columns
- **Equal To**: Exact numeric match
- **Not Equal To**: Numeric exclusion
- **Greater Than**: Value comparison (>)
- **Less Than**: Value comparison (<)
- **Range**: Between two values

### Filter Configuration
```typescript
// Pre-applied filters that users cannot modify
const lockedFilters = {
  isActive: new FilterParameter({ eq: true })
};

// Initial filter state
const initialFilter = {
  status: new FilterParameter({ eq: 'active' })
};
```

---

## Component Structure

### Standard File Structure

```
/src/pages/[Section]/[PageName]/
├── index.tsx          # Main component
├── index.scss         # Component styles
└── components/        # Page-specific components
```

### Component Template

```typescript
import React, { useMemo } from "react";
import { createColumnHelper } from "@tanstack/react-table";
import { Button } from "@mui/material";
import { useNavigate } from "react-router-dom";
import { EntityClient, QueryEntityModel } from "@/api/Client";
import DataGrid from "@/components/Reusable/DataGrid";
import "./index.scss";

const columnHelper = createColumnHelper<QueryEntityModel>();

export default function ManageEntity() {
  const navigate = useNavigate();

  const handleCreate = () => {
    navigate(`/settings/entities/edit`);
  };

  const handleEdit = (id: number) => {
    navigate(`/settings/entities/edit/${id}`);
  };

  const columns = useMemo(() => [
    // Column definitions
  ], [handleEdit]);

  const numericColumns = ['entityId'];

  return (
    <section className="manage-entity-page">
      <div className="main_wrapper">
        <div className="dis_main_box">
          <div className="title">
            <h5 className="title_text">Entity List</h5>
          </div>

          <DataGrid
            title="Entity List"
            createButtonText="Create Entity"
            onCreateClick={handleCreate}
            clientClass={EntityClient}
            queryModelClass={QueryEntityModel}
            columns={columns}
            numericColumns={numericColumns}
            enableSearch={true}
            enableFilter={true}
            enableSort={true}
            enableColumnVisibility={true}
            initialPageSize={20}
            className="entity-grid"
          />
        </div>
      </div>
    </section>
  );
}
```

---

## Styling Standards

### SCSS Structure

```scss
.manage-entity-page {
  .main_wrapper {
    padding: 1.25rem;
  }

  .dis_main_box {
    background: #ffffff;
    border-radius: 0.5rem;
    box-shadow: 0 0.125rem 0.5rem rgba(0, 0, 0, 0.1);
    padding: 1.5rem;

    .title {
      margin-bottom: 1.5rem;
      border-bottom: 0.0625rem solid #e9ecef;
      padding-bottom: 1rem;

      .title_text {
        font-size: 1.5rem;
        font-weight: 700;
      }
    }
  }
}
```

### Badge Color System

```scss
// Status badges
.status {
  padding: 0.25rem 0.5rem;
  border-radius: 0.75rem;
  font-size: 0.6875rem;
  font-weight: 600;
  text-transform: uppercase;

  &.active {
    background: #d4edda;
    color: #155724;
  }

  &.inactive {
    background: #f8d7da;
    color: #721c24;
  }
}
```

---

## TypeScript Patterns

### Generic Type Parameters

```typescript
<DataGrid<TData, TClient, TQueryModel>
  // Props...
/>

// Where:
// TData = QueryEntityModel (search result type)
// TClient = EntityClient (API client type)
// TQueryModel = QueryModel (query parameters type)
```

### Handler Function Signatures

```typescript
const handleCreate = (): void => {
  navigate(`[create-path]`);
};

const handleEdit = (id: number): void => {
  navigate(`[edit-path]/${id}`);
};
```

---

## Best Practices

### 1. Performance Optimization

```typescript
// Always wrap columns in useMemo
const columns = useMemo(
  () => [
    // Column definitions
  ],
  [handleEdit, handleView]
);

// Define numeric columns as constant
const numericColumns = ['entityId', 'count', 'total'];
```

### 2. Error Handling

```typescript
// Handle missing data gracefully
cell: (info) => info.getValue() || "N/A"

// Handle date formatting safely
cell: (info) => {
  const date = info.getValue();
  return date ? new Date(date).toLocaleDateString() : "N/A";
}
```

### 3. Accessibility

```typescript
// Always include alt text for images
<img src={edit_icon} alt="Edit" />

// Use semantic HTML in cells
<div role="cell">
  {info.getValue() || "N/A"}
</div>
```

---

## Management Patterns

### Full CRUD Pattern
Use when entity requires manual admin creation:
- Include `createButtonText` and `onCreateClick`
- Edit action in table rows
- Examples: Manufacturers, Categories, Settings

### Edit-Only Pattern
Use when entity is auto-created through other processes:
- No create button
- Edit action only
- Examples: Users (auto-created during registration)

### Pattern Decision Matrix

| Entity Type      | Pattern    | Reason                        |
|------------------|------------|-------------------------------|
| Manual entities  | Full CRUD  | Admin creates manually        |
| Auto-created     | Edit-Only  | System creates automatically  |
| Reference data   | Full CRUD  | Manual data entry required    |
| User accounts    | Edit-Only  | Registration creates users    |

---

## Migration Checklist

When updating existing DataGrid implementations:

### Required Changes
- [ ] Update to standard props pattern
- [ ] Enable search, filter, sort, column visibility
- [ ] Fix TypeScript generics
- [ ] Standardize page size to 20
- [ ] Add component-specific CSS class

### Column Updates
- [ ] Use column helper with proper typing
- [ ] Wrap columns in useMemo
- [ ] ID column first, actions second
- [ ] Use `|| "N/A"` for missing data
- [ ] Define numeric columns array

### Styling Updates
- [ ] Use standard page class pattern
- [ ] Import base styles
- [ ] Use standard badge classes
- [ ] Include responsive breakpoints

---

## Validation Checklist

- [ ] All required props configured
- [ ] Column definitions complete and ordered
- [ ] Numeric columns array defined
- [ ] Handlers properly typed
- [ ] Styles follow standards
- [ ] Accessibility requirements met
- [ ] Performance optimizations applied

---

## Related Protocols

- [Schema Integration](./schema-integration.md) - Auto-generating columns
- [Documentation Style Guide](../documentation/documentation-style-guide.md) - Standards

---

## Change Log

- 2026-01-03 v1.0: Initial generalized version
