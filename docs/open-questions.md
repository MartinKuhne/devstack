# Open Questions for E2E Test Development

## E2E Playwright Tests

The following tasks are blocked pending environment setup:

1. **Create Model Configuration tests** (Task #127)
   - Requires running Admin UI with GraphQL endpoint
   - Needs test data setup for ModelConfiguration entities

2. **Create Dashboard tests** (Task #128)
   - Requires running Admin UI with dashboard data
   - Needs test data for audit events and summary cards

3. **Create error handling and validation tests** (Task #130)
   - Requires running Admin UI with error scenarios
   - Needs API mocking infrastructure

4. **Configure CI/CD for Playwright tests** (Task #132)
   - Requires deployed environment URL for test execution
   - Needs GitHub Actions configuration

5. **Execute Playwright E2E tests and fix issues** (Task #134)
   - Blocked by all above tasks
   - Requires full test suite implementation first

### Prerequisites

To unblock these tasks, the following needs to be completed:

1. **Start Admin UI Application**
   - Ensure Docker Compose services are running
   - Verify GraphQL endpoint is accessible at http://localhost:5000/graphql
   - Confirm database migrations are applied

2. **Configure Playwright Environment**
   - Set up `.env` file with correct base URL
   - Configure test fixtures and helpers
   - Verify test container setup

3. **Test Data Strategy**
   - Decide on test data cleanup approach
   - Implement TestDataRegistry if needed
   - Configure afterEach/afterAll hooks

### Questions

1. Should E2E tests run against a dedicated test database or the development database?
2. What is the preferred approach for test data cleanup - automatic or manual?
3. Should we use Docker Compose to spin up test environment or run against existing services?
4. What is the target deployment URL for CI/CD test execution?
