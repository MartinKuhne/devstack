# GraphQL API Open Questions

## Phase 2

### Step 2.4 — Dashboard Query Optimization

**Question:** Should the `dashboardSummary` query return all audit events for all entities, or should it only return recent audit events for the current user's projects?

**Recommendation:** Start with all audit events and add filtering later based on user feedback and performance requirements.

### Step 2.6-2.8 — Status Transition Mutations

**Question:** Should status transitions accept only the target status and actor, or should they also accept optional metadata (e.g., comments, result details)?

**Recommendation:** Start with minimal parameters (target status + actor) and expand in phase 2.5+ if agents need to provide additional context during transitions.

### Step 2.9 — Model Configuration Encryption

**Question:** Should the encryption be transparent (auto-encrypt on set, auto-decrypt on get) or should the API layer explicitly call the secret service?

**Recommendation:** Implement transparent encryption in the entity setters to prevent accidental plaintext storage, but document this behavior clearly.

### Step 2.11 — Pagination Cursor Strategy

**Question:** Should cursors be based on IDs (simple) or on offset + timestamp for more complex sorting?

**Recommendation:** Start with ID-based cursors for simplicity, then add timestamp-based cursors if needed for time-range queries.

### Step 2.12 — OpenTelemetry Configuration

**Question:** Should trace sampling be configurable via environment variable, or use a fixed sampling rate?

**Recommendation:** Use fixed 100% sampling in development and 1% in production, configurable via environment variable for fine-tuning.

## General

### Long-Form Fields

**Question:** Should long-form text fields (Description, Memory, etc.) be lazy-loaded from a separate table or blob storage in the future?

**Recommendation:** Keep as text fields in phase 1. If performance becomes an issue, refactor to separate tables with the same schema.

### Audit Event Storage

**Question:** Should audit events have a retention policy or be archived after a certain period?

**Recommendation:** Implement in phase 3: add a `CreatedAt` index and document that audit events are append-only with no automatic deletion.

### GraphQL Schema Versioning

**Question:** Should the GraphQL schema use versioned endpoints (e.g., `/graphql/v1`) or maintain backward compatibility through deprecation?

**Recommendation:** Maintain backward compatibility using field deprecation and clear migration paths in schema documentation.
