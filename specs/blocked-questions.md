# Open Questions for Blocked Tasks

## Admin UI E2E Playwright Tests - Blocked Items

### Task #129: Create end-to-end workflow tests
**Status:** Blocked  
**Priority:** High

**Questions:**
1. What is the preferred authentication mechanism for the E2E tests?
   - Should tests use a test API key?
   - Should tests create temporary users?
   - Should tests run against a public endpoint?

2. What is the expected test data strategy?
   - Create fresh data for each test?
   - Share data across tests?
   - Use existing test data?

3. What is the deployment environment URL for running tests?
   - Development: ?
   - Staging: ?
   - Production: ?

4. How should test failures be reported?
   - GitHub Actions artifacts?
   - Allure reports?
   - Playwright trace viewer?

---

### Task #131: Add test data cleanup mechanism
**Status:** Blocked  
**Priority:** High

**Questions:**
1. Should cleanup be automatic (afterEach/afterAll) or manual?
2. Should tests use soft deletes or hard deletes?
3. How to handle cleanup failures?
   - Log and continue?
   - Fail the test?
   - Retry cleanup?
4. Should cleanup use GraphQL mutations or direct database access?

---

### Task #134: Execute Playwright E2E tests and fix issues
**Status:** Blocked  
**Priority:** High

**Questions:**
1. What is the baseline environment state for tests?
2. Should tests run in parallel or sequentially?
3. What is the timeout configuration?
   - Navigation timeout?
   - Action timeout?
   - Expect timeout?
4. Should tests run in headed or headless mode?

---

## Recommendations

### Immediate Next Steps
1. **Define authentication strategy** - Use a test API key with elevated permissions for E2E tests
2. **Set up test environment** - Deploy to a dedicated test environment
3. **Implement cleanup strategy** - Use GraphQL mutations with automatic afterEach cleanup
4. **Configure Playwright** - Headless mode, 30s timeout, sequential execution for E2E flows

### Dependencies
- GraphQL integration tests must be fully operational
- Test environment must be accessible
- CI/CD pipeline must support Playwright test execution

---

*Generated: 2026-04-15*  
*Review and resolve before unblocking tasks*
