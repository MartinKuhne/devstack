# Implementation Plan: Hot Chocolate Schema-First Implementation

**Feature**: Refactor src/Server to a schema-first implementation
**Feature Branch**: `003-hotchocolate-schema-first`
**Created**: 2026-04-24

## Overview

This plan refactors the existing Hot Chocolate GraphQL implementation from code-first (implementation-first) to schema-first approach. The schema will be defined in `.graphql` files and the server will load and execute based on this schema definition.

## Technical Approach

The schema-first approach in Hot Chocolate 15 involves:
1. Defining the complete GraphQL schema in `.graphql` files
2. Using `AddGraphQLServer().AddSchemaFromFile()` or `AddSchemaFromString()`
3. Binding resolvers through code that references the schema types

## Implementation Steps

### Step 1: Create GraphQL Schema File

**Description**: Create the `.graphql` schema file with complete type definitions matching the current API functionality.

**Files to Create/Modify**:
- New file: `src/Server/DevStack.Api/GraphQL/schema.graphql`

**Complexity**: 4/10

**Acceptance Criteria**:
- Schema file contains all Query, Mutation, and type definitions
- Schema validates successfully against GraphQL specification
- All existing operations are represented in the schema

---

### Step 2: Refactor Query Resolver Binding

**Description**: Refactor the Query class to bind to schema-defined types rather than auto-generating from POCOs.

**Files to Modify**:
- `src/Server/DevStack.Api/GraphQL/Query.cs` - Update to use schema binding attributes

**Complexity**: 3/10

**Acceptance Criteria**:
- All query operations work identically to before
- Filtering, sorting, and pagination still functional

---

### Step 3: Refactor Mutation Resolver Binding

**Description**: Refactor the Mutation class to bind to schema-defined input and output types.

**Files to Modify**:
- `src/Server/DevStack.Api/GraphQL/Mutation.cs` - Update to use schema binding

**Complexity**: 3/10

**Acceptance Criteria**:
- All mutation operations work identically to before
- Input types are correctly bound to schema definitions

---

### Step 4: Update Program.cs for Schema-First Loading

**Description**: Modify the GraphQL server configuration to load schema from file.

**Files to Modify**:
- `src/Server/DevStack.Api/Program.cs`

**Complexity**: 2/10

**Acceptance Criteria**:
- Server loads schema from .graphql file on startup
- Schema validation occurs on startup

---

### Step 5: Verify Backward Compatibility

**Description**: Run existing integration tests to verify no breaking changes.

**Complexity**: 2/10

**Acceptance Criteria**:
- All GraphQL integration tests pass
- Schema introspection returns complete type information

---

## Dependencies

- Hot Chocolate 15 library (already in use)
- No external dependencies required
- No database schema changes needed

## Testing Strategy

1. **Unit Tests**: Existing unit tests should continue to pass
2. **Integration Tests**: Run existing GraphQL integration tests
3. **Manual Testing**: Verify schema introspection returns expected types

## Risk Assessment

- **Low Risk**: This is primarily a code organization change
- No breaking changes to API contract
- All existing functionality is preserved

## Files Summary

| File | Action |
|------|--------|
| `src/Server/DevStack.Api/GraphQL/schema.graphql` | Create |
| `src/Server/DevStack.Api/GraphQL/Query.cs` | Modify |
| `src/Server/DevStack.Api/GraphQL/Mutation.cs` | Modify |
| `src/Server/DevStack.Api/Program.cs` | Modify |

## Estimated Effort

- Total: ~4-6 hours
- Schema creation: 2 hours
- Resolver refactoring: 2 hours
- Testing and validation: 1-2 hours