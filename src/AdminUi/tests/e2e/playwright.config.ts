import { defineConfig, devices } from '@playwright/test';
import dotenv from 'dotenv';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

dotenv.config({ path: path.resolve(__dirname, '.env') });

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
        baseURL: process.env.BASE_URL || 'http://localhost:5173',
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
    globalSetup: './testSetup.ts',
    globalTeardown: './testSetup.ts',
});
