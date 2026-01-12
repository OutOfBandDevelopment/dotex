# Security Audit Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Systematic security audit of API endpoints and services

---

## Overview

This protocol defines the steps for conducting a security audit of API applications. The audit ensures that all endpoints and services are properly secured according to the application's security model.

---

## Key Security Principles

1. **Secure by Default** - All endpoints require authentication unless explicitly made public
2. **Explicit Opt-Out** - Only specific attributes can make endpoints public
3. **Role-Based Access** - Role restrictions limit access to specific user types
4. **Data Scoping** - Users only see data within their organizational context
5. **Service-Layer Authorization** - Complex authorization logic lives in services

---

## Pre-Audit Checklist

Before starting the audit, verify these foundational elements:

### 1. Verify Secure-by-Default Configuration

Check that global authentication is required:

```bash
# Check for global authorization filter/middleware
grep -r "Authorize" --include="*.cs" src/
grep -r "authMiddleware" --include="*.ts" src/
```

**Expected patterns:**
- Global authorization filter registered
- Authentication middleware applied to all routes
- Default policy requires authenticated user

### 2. Verify Authorization Handler is Registered

```bash
# Find authorization handler registration
grep -r "AuthorizationHandler\|authHandler" --include="*.{cs,ts}" src/
```

---

## Audit Steps

### Step 1: Controller/Route Security Attributes

Run these searches to identify security status:

```bash
# Find all controllers/route handlers
find src -name "*Controller.*" -type f
find src -name "*routes.*" -type f

# Find handlers with public/anonymous access
grep -rn "AllowAnonymous\|isPublic\|public: true" --include="*.{cs,ts}" src/

# Find role-restricted endpoints
grep -rn "Authorize.*Role\|roles:\|requiredRoles" --include="*.{cs,ts}" src/
```

#### Security Attribute Matrix

For each controller/route, document:

| Controller          | Auth Required | Has Public Endpoints | Role-Restricted Actions | Notes   |
|---------------------|---------------|----------------------|-------------------------|---------|
| ProductController   | Yes           | No                   | Save, Delete            | Correct |
| UserController      | Yes           | No                   | Save (service-layer)    | OK      |
| AuthController      | Yes           | Yes (login only)     | -                       | Correct |

#### Verification Rules

| Scenario                             | Expected                  | Action if Missing            |
|--------------------------------------|---------------------------|------------------------------|
| Handler has no attributes            | Protected (global filter) | Add explicit attribute       |
| Handler has `[Authorize]`            | Protected                 | None - correct               |
| Handler has `[AllowAnonymous]`       | Public                    | Verify intentional, document |
| Action has role restriction          | Role-restricted           | Verify roles appropriate     |

---

### Step 2: Service-Layer Authorization

Some authorization must occur at the service layer. Audit these patterns:

```bash
# Find service methods that check user context
grep -rn "currentUser\|userContext\|GetSession" --include="*Service.*" src/

# Find permission checks in services
grep -rn "hasPermission\|isAuthorized\|canAccess" --include="*Service.*" src/

# Find data scoping patterns
grep -rn "OrganizationId\|TenantId\|OwnerId" --include="*Service.*" src/
```

#### Service Authorization Checklist

| Service          | Has User Context | Data Scoping | Role Checks     | Notes   |
|------------------|------------------|--------------|-----------------|---------|
| ProductService   | Yes              | By Org       | Admin for Save  | Correct |
| UserService      | Yes              | Self + Admin | Edit own/admin  | OK      |

---

### Step 3: Data Scoping Audit

Verify that queries respect organizational boundaries:

```bash
# Find repository query methods
grep -rn "findBy\|query\|select" --include="*Repository.*" src/

# Check for user context in queries
grep -rn "organizationId\|tenantId" --include="*Repository.*" src/
```

#### Data Scoping Rules

| User Role      | Should See              | Scoping Field    |
|----------------|-------------------------|------------------|
| SuperAdmin     | All data                | None (unfiltered)|
| OrgAdmin       | Organization data       | OrganizationId   |
| User           | Own data only           | UserId           |

---

### Step 4: Endpoint Security Classification

Classify all endpoints by security level:

#### Public Endpoints (Should be minimal)

```bash
grep -rn "AllowAnonymous\|isPublic" --include="*Controller.*" src/
```

**Valid public endpoints:**
- Health checks
- OpenAPI/Swagger documentation
- Login/authentication endpoints
- Public asset endpoints

**Flag for review:** Any business data endpoints marked public

#### Role-Restricted Endpoints

| Operation Type     | Expected Roles              |
|--------------------|----------------------------|
| Query/Get          | Any authenticated (or specific) |
| Create/Update      | Admin, or owner context    |
| Delete             | Admin                      |
| Bulk operations    | Admin or elevated roles    |
| Admin functions    | Admin only                 |

---

### Step 5: Bulk Action Security

Verify bulk actions have appropriate protection:

```bash
# Find all bulk action endpoints
grep -rn "bulk\|batch" --include="*Controller.*" src/

# Verify they have role restrictions
grep -B5 "bulk\|batch" --include="*Controller.*" src/ | grep -E "Role\|Authorize"
```

**Required for bulk actions:**
- Role restriction with appropriate roles
- Confirmation required for destructive actions
- Audit logging

---

### Step 6: Sensitive Data Protection

Check for proper handling of sensitive fields:

```bash
# Find password/secret handling
grep -rn "password\|secret\|token\|apiKey" --include="*.{cs,ts}" src/

# Check for exclusion from exports/responses
grep -rn "JsonIgnore\|exclude\|hidden" --include="*.{cs,ts}" src/
```

---

## Audit Report Template

```markdown
# Security Audit Report

**Date:** YYYY-MM-DD
**Auditor:** {Name}
**Scope:** {Full/Partial - specify areas}

## Executive Summary

- Total Controllers Audited: X
- Total Services Audited: X
- Issues Found: X (Critical: X, High: X, Medium: X, Low: X)

## Secure-by-Default Status

- [ ] Global authorization configured
- [ ] Authorization handler registered
- [ ] Default policy requires authentication

## Findings

### Critical Issues
{List any critical security issues}

### High Priority Issues
{List high priority issues}

### Medium Priority Issues
{List medium priority issues}

### Low Priority/Recommendations
{List low priority items and recommendations}

## Controller Audit Results

| Controller | Status | Issues | Notes |
|------------|--------|--------|-------|
| ...        | ...    | ...    | ...   |

## Service Audit Results

| Service | Data Scoping | Role Checks | Issues |
|---------|--------------|-------------|--------|
| ...     | ...          | ...         | ...    |

## Recommendations

1. {Recommendation 1}
2. {Recommendation 2}

## Sign-off

- [ ] All critical issues addressed
- [ ] High priority issues have remediation plan
- [ ] Documentation updated
```

---

## Common Security Anti-Patterns

### DO NOT

1. **Skip authorization on new endpoints** - Always add explicit auth attributes
2. **Use public access on data endpoints** - Business data should be protected
3. **Forget service-layer authorization** - Controller attributes aren't enough
4. **Ignore data scoping** - Always filter by user context
5. **Return sensitive data in responses** - Use exclusion attributes
6. **Allow bulk operations without confirmation** - Require confirmation for destructive actions

### DO

1. **Add explicit authorization to all endpoints** - Documents intent clearly
2. **Use role restrictions for elevated operations** - Save, Delete, Admin functions
3. **Implement data scoping in repositories** - Filter by organization/user
4. **Validate user context in services** - Check ownership before modifications
5. **Log security-relevant operations** - Audit trail for admin actions
6. **Review public endpoint usage** - Should be rare and documented

---

## Validation Checklist

- [ ] Secure-by-default configuration verified
- [ ] All controllers/routes audited
- [ ] Service-layer authorization reviewed
- [ ] Data scoping patterns verified
- [ ] Public endpoints justified
- [ ] Role restrictions appropriate
- [ ] Bulk actions secured
- [ ] Sensitive data protected
- [ ] Audit report generated

---

## Related Protocols

- [Architectural Analysis](./architectural-analysis.md) - Overall system analysis
- [Comprehensive Analysis Checklist](./comprehensive-analysis-checklist.md) - Completeness

---

## Change Log

- 2026-01-03 v1.0: Generalized for any project
