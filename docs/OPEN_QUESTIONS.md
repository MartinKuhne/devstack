# Open Questions

## Dead Letter Queue Implementation

1. **Optional Chaining Consistency**: Should we be using optional chaining more consistently in the worker.ts file when accessing job.data properties? Currently we have a mix of direct access and optional chaining with fallbacks.

2. **Error Handling Specificity**: Is the current error handling in the dead letter queue processor sufficient, or should we add more specific handling for different types of failures?

3. **Defect Creation Implementation**: The current implementation logs that it would create a defect but doesn't actually implement the defect creation. What is the expected timeline for implementing the actual defect creation functionality?

4. **Testing Coverage**: Do we have sufficient test coverage for edge cases in the dead letter queue functionality, particularly around malformed job data?

## General Observations

5. **Logging Consistency**: Are we consistent in our logging approach across different modules? Should we standardize on certain log fields or formats?

6. **Type Safety**: Are there additional TypeScript improvements we could make to increase type safety in the queues and worker implementations?

7. **Performance Impact**: What is the performance impact of the dead letter queue checking on each job failure? Is this negligible or should we consider optimizations?

---

## Epic Feature - Remaining Tasks

### Task 143: Implement Epic CRUD in Admin UI

**Status:** Blocked - Requires frontend development expertise

**Questions:**
1. What is the preferred UI framework/library for the Admin UI? (The project uses Next.js with shadcn/ui based on components.json)
2. Should Epic pages follow the same patterns as existing Feature/Project pages?
3. What specific fields should be editable in the Epic create/edit forms?
4. Should there be a dedicated Epic list page or should Epics be shown alongside Features?
5. How should Epic-Feature relationships be displayed and managed in the UI?

**Dependencies:**
- GraphQL API is ready (mutations implemented)
- MCP tools available for Epic operations

### Task 146: Update existing Feature and Task tests for Epic hierarchy

**Status:** Blocked - Requires test review and updates

**Questions:**
1. Which specific Feature tests need updates to account for optional EpicId?
2. Should new tests be added to verify the Task -> Feature -> Epic hierarchy?
3. Are there any integration tests that need updates for the Epic relationship?
4. Should the existing tests be refactored to include EpicId scenarios?

**Current State:**
- All existing tests pass (89 unit, 65 integration, 57 GraphQL integration)
- Epic entity and relationships are properly configured
- EpicId is nullable for backward compatibility

**Recommendation:**
Review test files in `src/Server/DevStack.Tests.Integration/GraphQL/` and `src/Server/DevStack.Tests.Unit/` to identify tests that create Features or Tasks and verify they work correctly with the optional EpicId field.