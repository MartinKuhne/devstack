# Open Questions

## Phase 1

### Step 1.7 — Implement EF Core infrastructure

**Question:** Should `Severity` on Defect be a required enum field added to the entity, or a tag-style value object?

**Recommendation:** Adding it as a nullable enum field to Defect in phase 1.

**Status:** Already implemented as nullable enum field.

---

**Question:** Should `Project.Memory` be lazily loaded from a separate blob storage in the future, or kept as text?

**Recommendation:** Text in phase 1.

**Status:** Already implemented as text field.

---

## Phase 2

### Step 2.12 — Add OpenTelemetry to API

**Question:** Should long-form fields accept raw markdown strings or structured JSON arrays for acceptance criteria?

**Recommendation:** Markdown text in phase 1, structured arrays in phase 2 if multi-item editing is needed.

**Status:** Open - not yet implemented.

---

**Question:** Should audit events be queryable through GraphQL or only stored for diagnostics?

**Recommendation:** Queryable via GraphQL with pagination.

**Status:** Open - not yet implemented.

---

## General

### EF Core Version Conflicts

**Issue:** The solution had version conflicts between EF Core 8.0.10 (used in Infrastructure) and 10.0.5 (used in Api). This was causing build warnings.

**Recommendation:** Align all projects to use EF Core 8.0.10 for compatibility with Npgsql.EntityFrameworkCore.PostgreSQL 8.0.10.

**Status:** Resolved - updated DevStack.Api.csproj to use EF Core 8.0.10.

---
