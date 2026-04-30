import { test, expect } from '@playwright/test';
import {
    LargeLanguageModelsPage,
    LargeLanguageModelDialog,
} from './pages/LargeLanguageModelsPage.js';
import { NavigationHelper } from './helpers/NavigationHelper.js';

test.describe('Large Language Model CRUD', () => {
    let llmPage: LargeLanguageModelsPage;
    let dialog: LargeLanguageModelDialog;
    let navigationHelper: NavigationHelper;

    test.beforeEach(async ({ page }) => {
        llmPage = new LargeLanguageModelsPage(page);
        dialog = new LargeLanguageModelDialog(page);
        navigationHelper = new NavigationHelper(page);
    });

    test('should display LLM page with correct heading', async () => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await expect(llmPage.pageTitle).toBeVisible();
        await llmPage.expectNoErrors();
    });

    test('should have Add Model button', async () => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await expect(llmPage.addModelButton).toBeVisible();
        await llmPage.expectNoErrors();
    });

    test('should open dialog when clicking Add Model', async () => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await llmPage.clickAddModel();
        await expect(dialog.dialog).toBeVisible({ timeout: 5000 });
    });

    test('should cancel dialog creation', async () => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await llmPage.clickAddModel();
        await expect(dialog.dialog).toBeVisible({ timeout: 5000 });
        await dialog.cancel();
        await expect(dialog.dialog).not.toBeVisible({ timeout: 5000 });
        await llmPage.expectNoErrors();
    });

    test('should validate required fields are empty', async () => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await llmPage.clickAddModel();
        await expect(dialog.dialog).toBeVisible({ timeout: 5000 });

        const addButton = dialog.dialog.getByRole('button', { name: /Add|Save/ }).first();
        await expect(addButton).toBeDisabled();

        await dialog.cancel();
        await llmPage.expectNoErrors();
    });

    test('should have cost input field', async ({ page }) => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await llmPage.clickAddModel();
        await expect(dialog.dialog).toBeVisible({ timeout: 5000 });

        const costInput = page.getByLabel('Cost (0-100)');
        await expect(costInput).toBeVisible();

        await dialog.cancel();
        await llmPage.expectNoErrors();
    });

    test('should validate cost field range', async ({ page }) => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await llmPage.clickAddModel();
        await expect(dialog.dialog).toBeVisible({ timeout: 5000 });

        const costInput = page.getByLabel('Cost (0-100)');
        await costInput.fill('-5');

        const addButton = dialog.addButton;
        await expect(addButton).toBeDisabled();

        await dialog.cancel();
        await llmPage.expectNoErrors();
    });

    test('should navigate back from models page', async ({ page }) => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await navigationHelper.navigateToDashboard();
        await expect(page.getByRole('heading', { name: 'Dashboard', level: 2 })).toBeVisible();
        await llmPage.expectNoErrors();
    });

    test('should show/hide API key toggle', async ({ page }) => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await llmPage.clickAddModel();
        await expect(dialog.dialog).toBeVisible({ timeout: 5000 });

        const apiKeyInput = page.locator('#apiKey');
        if (await apiKeyInput.isVisible()) {
            const hideButton = page.getByRole('button', { name: 'Hide' });
            const showButton = page.getByRole('button', { name: 'Show' });
            await expect(hideButton.or(showButton)).toBeVisible();
        }

        await dialog.cancel();
        await llmPage.expectNoErrors();
    });

    test('should have complexity dropdown with options 1-10', async ({ page }) => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await llmPage.clickAddModel();
        await expect(dialog.dialog).toBeVisible({ timeout: 5000 });

        const complexitySelect = page.getByLabel('Max Complexity (1-10)');
        if (await complexitySelect.isVisible()) {
            await complexitySelect.click();
            await expect(page.locator('[role="option"]')).toHaveCount(10);
        }

        await dialog.cancel();
        await llmPage.expectNoErrors();
    });

    test('should navigate between models and projects', async ({ page }) => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await navigationHelper.navigateToProjects();
        await expect(page.getByRole('heading', { name: 'Projects', level: 2 })).toBeVisible();

        await navigationHelper.navigateToModels();
        await expect(llmPage.pageTitle).toBeVisible();
        await llmPage.expectNoErrors();
    });
});
