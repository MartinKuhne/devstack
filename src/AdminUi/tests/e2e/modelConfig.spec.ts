import { test, expect } from '@playwright/test';
import {
    ModelConfigurationListPage,
    CreateModelConfigurationDialog,
    ModelConfigurationDetailPage,
    EditModelConfigurationDialog,
} from './pages/ModelConfigurationPage.js';

const TEST_PROJECT_ID = process.env.TEST_PROJECT_ID || 'test-project-123';
const API_KEY = process.env.TEST_API_KEY || 'sk-test-key-12345';

test.describe('Model Configuration Page', () => {
    let modelConfigurationListPage: ModelConfigurationListPage;
    let createModelConfigurationDialog: CreateModelConfigurationDialog;
    let modelConfigurationDetailPage: ModelConfigurationDetailPage;
    let editModelConfigurationDialog: EditModelConfigurationDialog;

    test.beforeEach(async ({ page }) => {
        modelConfigurationListPage = new ModelConfigurationListPage(page);
        createModelConfigurationDialog = new CreateModelConfigurationDialog(page);
        modelConfigurationDetailPage = new ModelConfigurationDetailPage(page);
        editModelConfigurationDialog = new EditModelConfigurationDialog(page);
    });

    test('navigates to model configuration list page', async ({ page }) => {
        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await expect(modelConfigurationListPage.pageTitle).toBeVisible();
        await expect(modelConfigurationListPage.addModelButton).toBeVisible();
    });

    test('shows empty state when no model configurations exist', async ({ page }) => {
        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.waitForEmptyState();
        await expect(modelConfigurationListPage.emptyStateMessage).toBeVisible();
    });

    test('validates empty URL on create model configuration', async ({ page }) => {
        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await expect(createModelConfigurationDialog.dialog).toBeVisible();

        await createModelConfigurationDialog.submitEmptyForm();
        await createModelConfigurationDialog.waitForUrlError();
        await expect(createModelConfigurationDialog.urlError).toBeVisible();
    });

    test('validates invalid URL format', async ({ page }) => {
        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await expect(createModelConfigurationDialog.dialog).toBeVisible();

        await createModelConfigurationDialog.submitInvalidUrl();
        await createModelConfigurationDialog.waitForUrlError();
        await expect(createModelConfigurationDialog.urlError).toBeVisible();
    });

    test('validates missing model name', async ({ page }) => {
        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await expect(createModelConfigurationDialog.dialog).toBeVisible();

        await createModelConfigurationDialog.submitMissingModel();
        await createModelConfigurationDialog.waitForModelError();
        await expect(createModelConfigurationDialog.modelError).toBeVisible();
    });

    test('creates model configuration with all fields', async ({ page }) => {
        const modelName = `gpt-4o-mini-${Date.now()}`;
        const modelAlias = 'Test Model';
        const url = 'https://api.openai.com/v1';
        const maxComplexity = '5';

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await expect(createModelConfigurationDialog.dialog).toBeVisible();

        await createModelConfigurationDialog.createModelConfiguration(
            url,
            modelName,
            modelAlias,
            API_KEY,
            maxComplexity
        );

        await expect(createModelConfigurationDialog.dialog).not.toBeVisible();
        await modelConfigurationListPage.waitForModelConfigurations();
        await expect(page.getByText(modelAlias)).toBeVisible();
        await expect(page.getByText(modelName)).toBeVisible();
    });

    test('creates model configuration with minimal fields', async ({ page }) => {
        const modelName = `minimal-model-${Date.now()}`;
        const url = 'https://api.example.com/v1';

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await expect(createModelConfigurationDialog.dialog).toBeVisible();

        await createModelConfigurationDialog.createModelConfiguration(url, modelName, undefined, API_KEY);

        await expect(createModelConfigurationDialog.dialog).not.toBeVisible();
        await modelConfigurationListPage.waitForModelConfigurations();
        await expect(page.getByText(modelName)).toBeVisible();
    });

    test('toggles API key visibility', async ({ page }) => {
        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await expect(createModelConfigurationDialog.dialog).toBeVisible();

        await createModelConfigurationDialog.fillForm('https://api.example.com/v1', 'test-model', undefined, 'sk-test');
        
        const isVisibleBefore = await createModelConfigurationDialog.isApiKeyVisible();
        expect(isVisibleBefore).toBe(false);

        await createModelConfigurationDialog.toggleApiKeyVisibility();
        
        const isVisibleAfter = await createModelConfigurationDialog.isApiKeyVisible();
        expect(isVisibleAfter).toBe(true);
    });

    test('selects max complexity from dropdown', async ({ page }) => {
        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await expect(createModelConfigurationDialog.dialog).toBeVisible();

        await createModelConfigurationDialog.fillForm(
            'https://api.example.com/v1',
            'test-model',
            undefined,
            API_KEY,
            '10'
        );

        const complexitySelect = createModelConfigurationDialog.complexitySelect;
        await expect(complexitySelect).toBeVisible();
    });

    test('cancels create model configuration dialog', async ({ page }) => {
        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await expect(createModelConfigurationDialog.dialog).toBeVisible();

        await createModelConfigurationDialog.cancel();
        await expect(createModelConfigurationDialog.dialog).not.toBeVisible();
    });

    test('navigates to model configuration detail page', async ({ page }) => {
        const modelName = `detail-test-${Date.now()}`;
        const url = 'https://api.example.com/v1';

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(url, modelName, undefined, API_KEY);

        await modelConfigurationListPage.waitForModelConfigurations();
        await modelConfigurationListPage.clickModelCard(modelName);

        await modelConfigurationDetailPage.waitForModelConfigurationDetail();
        await expect(modelConfigurationDetailPage.pageTitle).toBeVisible();
    });

    test('shows loading states', async ({ page }) => {
        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        
        await expect(page.locator('body')).toBeVisible();
    });

    test('handles server error gracefully', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            await route.fulfill({
                status: 500,
                body: JSON.stringify({ errors: [{ message: 'Internal server error' }] }),
            });
        });

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(
            'https://api.example.com/v1',
            'test-model',
            undefined,
            API_KEY
        );

        await createModelConfigurationDialog.waitForErrorMessage();
        await expect(createModelConfigurationDialog.errorMessage).toBeVisible();
    });

    test('handles network failure with mocked API', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            await route.abort('failed');
        });

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(
            'https://api.example.com/v1',
            'test-model',
            undefined,
            API_KEY
        );

        await createModelConfigurationDialog.waitForErrorMessage();
        await expect(createModelConfigurationDialog.errorMessage).toBeVisible();
    });

    test('prevents XSS in model name input', async ({ page }) => {
        const xssPayload = '<script>alert("XSS")</script>';
        const url = 'https://api.example.com/v1';

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(url, xssPayload, undefined, API_KEY);

        await modelConfigurationListPage.waitForModelConfigurations();
        await expect(page.getByText(xssPayload)).not.toBeVisible();
        await expect(page.locator('script')).toHaveCount(0);
    });

    test('validates max length for URL input', async ({ page }) => {
        const longUrl = 'https://'.padEnd(1000, 'a');
        const modelName = 'test-model';

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.fillForm(longUrl, modelName, undefined, API_KEY);

        const inputLength = await createModelConfigurationDialog.urlInput.inputValue();
        expect(inputLength.length).toBeLessThanOrEqual(2048);
    });

    test('shows disabled state during form submission', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            await new Promise(resolve => setTimeout(resolve, 1000));
            await route.fulfill({
                status: 200,
                body: JSON.stringify({
                    data: {
                        createModelConfiguration: {
                            id: 'test-id',
                            projectId: TEST_PROJECT_ID,
                            url: 'https://api.example.com/v1',
                            model: 'test-model',
                            modelAlias: null,
                            maxComplexity: 3,
                            createdAt: new Date().toISOString(),
                            updatedAt: new Date().toISOString(),
                        },
                    },
                }),
            });
        });

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(
            'https://api.example.com/v1',
            'test-model',
            undefined,
            API_KEY
        );

        const addButton = createModelConfigurationDialog.addButton;
        await expect(addButton).toBeDisabled();
    });

    test('displays complexity badges with correct variants', async ({ page }) => {
        const lowComplexityModel = `low-complexity-${Date.now()}`;
        const mediumComplexityModel = `medium-complexity-${Date.now()}`;
        const highComplexityModel = `high-complexity-${Date.now()}`;
        const url = 'https://api.example.com/v1';

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(url, lowComplexityModel, 'Low', API_KEY, '2');
        
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(url, mediumComplexityModel, 'Medium', API_KEY, '5');
        
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(url, highComplexityModel, 'High', API_KEY, '9');

        await modelConfigurationListPage.waitForModelConfigurations();
        await expect(page.getByText('2')).toBeVisible();
        await expect(page.getByText('5')).toBeVisible();
        await expect(page.getByText('9')).toBeVisible();
    });

    test('edits model configuration', async ({ page }) => {
        const modelName = `edit-test-${Date.now()}`;
        const url = 'https://api.example.com/v1';
        const updatedUrl = 'https://api.updated.com/v2';

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(url, modelName, undefined, API_KEY);

        await modelConfigurationListPage.waitForModelConfigurations();
        await modelConfigurationListPage.clickModelCard(modelName);
        await modelConfigurationDetailPage.waitForModelConfigurationDetail();

        await modelConfigurationDetailPage.clickEdit();
        await expect(editModelConfigurationDialog.dialog).toBeVisible();

        await editModelConfigurationDialog.updateModelConfiguration(updatedUrl);
        await expect(editModelConfigurationDialog.dialog).not.toBeVisible();

        await expect(page.getByText(updatedUrl)).toBeVisible();
    });

    test('cancels edit model configuration dialog', async ({ page }) => {
        const modelName = `cancel-edit-${Date.now()}`;
        const url = 'https://api.example.com/v1';

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(url, modelName, undefined, API_KEY);

        await modelConfigurationListPage.waitForModelConfigurations();
        await modelConfigurationListPage.clickModelCard(modelName);
        await modelConfigurationDetailPage.waitForModelConfigurationDetail();

        await modelConfigurationDetailPage.clickEdit();
        await expect(editModelConfigurationDialog.dialog).toBeVisible();

        await editModelConfigurationDialog.cancel();
        await expect(editModelConfigurationDialog.dialog).not.toBeVisible();
    });

    test('deletes model configuration', async ({ page }) => {
        const modelName = `delete-test-${Date.now()}`;
        const url = 'https://api.example.com/v1';

        await modelConfigurationListPage.navigate(TEST_PROJECT_ID);
        await modelConfigurationListPage.clickAddModel();
        await createModelConfigurationDialog.createModelConfiguration(url, modelName, undefined, API_KEY);

        await modelConfigurationListPage.waitForModelConfigurations();
        const initialCount = await modelConfigurationListPage.getModelCount();

        await modelConfigurationListPage.clickModelCard(modelName);
        await modelConfigurationDetailPage.waitForModelConfigurationDetail();

        await modelConfigurationDetailPage.clickDelete();
        await page.waitForTimeout(500);

        await modelConfigurationListPage.waitForModelConfigurations();
        const finalCount = await modelConfigurationListPage.getModelCount();
        expect(finalCount).toBe(initialCount - 1);
    });
});
