import { type FullConfig } from '@playwright/test';
import { deleteTestData } from './helpers/testDataOperations.js';

/**
 * Global Playwright teardown: removes every `[DeleteAfterTest]` row created by the
 * suite so no test data leaks into the database.
 */
async function globalTeardown(config: FullConfig): Promise<void> {
    void config;
    const graphqlUrl =
        process.env.GRAPHQL_API_URL || 'http://localhost:8087/graphql';

    const outcome = await deleteTestData(graphqlUrl);

    if (!outcome.attempted) {
        console.log(`Backend not available for test data cleanup after suite: ${outcome.message}`);
        return;
    }

    if (outcome.success) {
        console.log(`Test data cleaned up after test suite: ${outcome.message ?? 'ok'}`);
    } else {
        console.warn(
            `Test data cleanup after test suite failed: ${
                outcome.errors?.join('; ') ?? outcome.message ?? 'unknown error'
            }`
        );
    }
}

export default globalTeardown;
