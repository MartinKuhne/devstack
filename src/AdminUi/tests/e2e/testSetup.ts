import { type FullConfig } from '@playwright/test';
import { deleteTestData } from './helpers/testDataOperations.js';

/**
 * Global Playwright setup: removes `[DeleteAfterTest]` rows left behind by earlier
 * runs so the suite starts from a clean database.
 */
async function globalSetup(config: FullConfig): Promise<void> {
    void config;
    const graphqlUrl =
        process.env.GRAPHQL_API_URL || 'http://localhost:8087/graphql';

    const outcome = await deleteTestData(graphqlUrl);

    if (!outcome.attempted) {
        console.log(`Backend not available for test data cleanup before suite: ${outcome.message}`);
        return;
    }

    if (outcome.success) {
        console.log(`Test data cleaned up before test suite: ${outcome.message ?? 'ok'}`);
    } else {
        console.warn(
            `Test data cleanup before test suite failed: ${
                outcome.errors?.join('; ') ?? outcome.message ?? 'unknown error'
            }`
        );
    }
}

export default globalSetup;
