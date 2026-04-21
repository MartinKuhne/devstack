import { defineConfig, devices } from '@playwright/test';
import dotenv from 'dotenv';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

dotenv.config({ path: path.resolve(__dirname, '.env') });

export default defineConfig({
    testDir: './tests/e2e',
    testMatch: '**/*.spec.ts',
    timeout: 60000,
    retries: process.env.CI ? 2 : 0,
    reporter: [
        ['html', { outputFolder: './tests/reports' }],
        ['json', { outputFile: './tests/reports/results.json' }],
        ['junit', { outputFile: './tests/reports/results.xml' }],
        ['list'],
    ],
    use: {
        baseURL: process.env.BASE_URL || 'http://localhost:5173',
        GraphQLEndpoint: process.env.GRAPHQL_API_URL || 'http://localhost:8087/graphql',
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
    outputDir: './tests/e2e/test-results',
    workers: 1,
});
