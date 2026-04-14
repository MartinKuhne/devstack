# Open Questions

## Phase 1 - Open Questions

### Step 1.7 — Implement EF Core infrastructure
- **Question:** Should `Severity` on Defect be a required enum field added to the entity, or a tag-style value object?
- **Recommendation:** Add it as a nullable enum field to Defect in phase 1.
- Decision: Nullable enum, Low/Medium/High/Critical

### Step 1.7 — Implement EF Core infrastructure
- **Question:** Should `Project.Memory` be lazily loaded from a separate blob storage in the future, or kept as text?
- **Recommendation:** Text in phase 1.
- Decision: Keep as text

## Phase 2 - Open Questions

### Step 2.2 — Map domain types to GraphQL object types
- **Question:** Should long-form fields accept raw markdown strings or structured JSON arrays for acceptance criteria?
- **Recommendation:** markdown text in phase 1, structured arrays in phase 2 if multi-item editing is needed.
- Decision: Markdown

### Step 2.2 — Map domain types to GraphQL object types
- **Question:** Should audit events be queryable through GraphQL or only stored for diagnostics?
- **Recommendation:** queryable via GraphQL with pagination.
- Decision: queryable via GraphQL with pagination.

## Phase 2 - Project Settings - Open Questions

### Step 2.6 — GitHub URL and token fields
- **Question:** Should the GitHub token field be exposed in the GraphQL schema?
- **Answer:** Yes, for the update mutation only. Never expose it in queries.
- **Status:** Backend is complete. Frontend schema file needs updating.
- **Action:** Update `src/AdminUi/src/graphql/schema.graphql` to add `githubToken_Encrypted` field to `UpdateProjectInput`.
