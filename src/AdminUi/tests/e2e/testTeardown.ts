import { type FullConfig } from '@playwright/test';

async function cleanupTestData(graphqlUrl: string): Promise<boolean> {
    try {
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
            return true;
        }
    } catch {
        console.log('Backend not available for test data cleanup after suite');
    }
    return false;
}

async function globalTeardown(config: FullConfig): Promise<void> {
    void config;
    const graphqlUrl =
        process.env.GRAPHQL_API_URL || 'http://localhost:8087/graphql';
    await cleanupTestData(graphqlUrl);
}

export default globalTeardown;
