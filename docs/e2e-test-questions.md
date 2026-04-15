# Open Questions for E2E Testing

## Test Data Cleanup (Task #131)

### Status: Blocked
- No delete mutations found in GraphQL schema
- Need to implement delete mutations for: Project, Feature, Task, Defect, ModelConfiguration

### Implementation Questions
1. Should cleanup use GraphQL mutations or direct database access?
2. Should we track all created entities or use naming conventions?
3. Should cleanup run after each test or after all tests?
4. How to handle cleanup failures without breaking test results?

### Technical Questions
1. Are there GraphQL mutations for deleting projects, features, tasks, and defects?
2. What is the cascade delete behavior in the GraphQL API?
3. Should we implement soft delete or hard delete for test data?

---

## End-to-end workflow tests (Task #129)

### Missing Page Objects
- **FeaturePage.ts**: Need to create page objects for FeatureListPage and FeatureDetailPage
  - What fields are required for feature creation?
  - What status transitions are available?
  - How are tasks and defects linked to features?

### Implementation Questions
1. Should workflow tests create their own test data or use existing fixtures?
2. What is the expected cleanup strategy for workflow test data?
3. Should workflow tests run sequentially or in parallel?
4. What is the timeout for full workflow execution?

### Dependencies
- Requires FeaturePage.ts page objects to be created
- Requires test data cleanup mechanism (Task #131)
- Requires running application server

---

## Test data cleanup mechanism (Task #131)

### Implementation Questions
1. Should cleanup use GraphQL mutations or direct database access?
2. Should we track all created entities or use naming conventions?
3. Should cleanup run after each test or after all tests?
4. How to handle cleanup failures without breaking test results?

### Technical Questions
1. Are there GraphQL mutations for deleting projects, features, tasks, and defects?
2. What is the cascade delete behavior in the GraphQL API?
3. Should we implement soft delete or hard delete for test data?

---

## Execute Playwright E2E tests (Task #134)

### Environment Setup
1. How to start the Admin UI server for E2E tests?
2. What is the base URL for test execution?
3. How to seed test data before running tests?
4. Should tests run against local or deployed environment?

### CI/CD Integration
1. What is the CI pipeline configuration?
2. How to capture test artifacts (screenshots, videos, traces)?
3. What is the expected test execution time?
4. How to handle flaky tests?
