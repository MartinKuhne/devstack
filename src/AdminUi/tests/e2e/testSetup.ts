import { type FullConfig } from '@playwright/test';

async function globalSetup(config: FullConfig): Promise<void> {
    // Cleanup test data before running tests
    void config;
    try {
        const graphqlUrl =
            process.env.GRAPHQL_API_URL || 'http://localhost:8087/graphql';
        const response = await fetch(graphqlUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                query: 'mutation CleanupTestData { cleanupTestData { success message } }',
            }),
        });
        const result = await response.json();
        if (result?.data?.cleanupTestData?.success) {
            console.log('Test data cleaned up before test suite');
        }
    } catch {
        // Backend may not be available during setup
        console.log('Backend not available for test data cleanup before suite');
    }
}

async function globalTeardown(config: FullConfig): Promise<void> {
    // Cleanup test data after running tests
    void config;
    try {
        const graphqlUrl =
            process.env.GRAPHQL_API_URL || 'http://localhost:8087/graphql';
        const response = await fetch(graphqlUrl, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                query: 'mutation CleanupTestData { cleanupTestData { success message } }',
            }),
        });
        const result = await response.json();
        if (result?.data?.cleanupTestData?.success) {
            console.log('Test data cleaned up after test suite');
        }
    } catch {
        // Backend may not be available during teardown
        console.log('Backend not available for test data cleanup after suite');
    }
}

export default { globalSetup, globalTeardown };
