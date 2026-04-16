import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
    testDir: './',
    timeout: 60000,
    retries: process.env.CI ? 2 : 0,
    reporter: [
        ['html', { outputFolder: '../reports' }],
        ['json', { outputFile: '../reports/results.json' }],
        ['junit', { outputFile: '../reports/results.xml' }],
    ],
    use: {
        baseURL: process.env.API_URL || 'http://localhost:5173',
        GraphQLEndpoint: process.env.GRAPHQL_API_URL || 'http://localhost:5000/graphql',
        headless: !!process.env.CI,
        trace: 'retain-on-failure',
        screenshot: 'only-on-failure',
        video: 'retain-on-failure',
    },
    projects: [
        {
            name: 'chromium',
            use: { ...devices['Desktop Chrome'] },
        },
    ],
    outputDir: './test-results',
    workers: 1,
});
