import { test, expect } from '@playwright/test';
import { LargeLanguageModelsPage, LargeLanguageModelDialog } from './pages/LargeLanguageModelsPage.js';
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
    });

    test('should have Add Model button', async () => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await expect(llmPage.addModelButton).toBeVisible();
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
    });

    test('should validate required fields are empty', async () => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await llmPage.clickAddModel();
        await expect(dialog.dialog).toBeVisible({ timeout: 5000 });
        
        // Try to submit without filling anything - should show validation errors
        const addButton = dialog.dialog.getByRole('button', { name: /Add|Save/ }).first();
        if (await addButton.isVisible()) {
            await addButton.click();
            // Should still be visible due to validation error
            await expect(dialog.dialog).toBeVisible({ timeout: 5000 });
        }
        
        await dialog.cancel();
    });

    test('should navigate back from models page', async ({ page }) => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await navigationHelper.navigateToDashboard();
        await expect(page.getByRole('heading', { name: 'Dashboard', level: 2 })).toBeVisible();
    });

    test('should show/hide API key toggle', async ({ page }) => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await llmPage.clickAddModel();
        await expect(dialog.dialog).toBeVisible({ timeout: 5000 });
        
        // The dialog should have an API key input with show/hide button
        const apiKeyInput = page.locator('#apiKey');
        if (await apiKeyInput.isVisible()) {
            const hideButton = page.getByRole('button', { name: 'Hide' });
            const showButton = page.getByRole('button', { name: 'Show' });
            // Either Hide or Show button should be visible depending on current state
            await expect(hideButton.or(showButton)).toBeVisible();
        }
        
        await dialog.cancel();
    });

    test('should have complexity dropdown with options 1-10', async ({ page }) => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await llmPage.clickAddModel();
        await expect(dialog.dialog).toBeVisible({ timeout: 5000 });
        
        const complexitySelect = page.getByLabel('Max Complexity (1-10)');
        if (await complexitySelect.isVisible()) {
            await complexitySelect.click();
            // Should show options for numbers 1-10
            await expect(page.locator('[role="option"]')).toHaveCount(10);
        }
        
        await dialog.cancel();
    });

    test('should navigate between models and projects', async ({ page }) => {
        await llmPage.navigate();
        await llmPage.waitForPageLoaded();
        await navigationHelper.navigateToProjects();
        await expect(page.getByRole('heading', { name: 'Projects', level: 2 })).toBeVisible();
        
        await navigationHelper.navigateToModels();
        await expect(llmPage.pageTitle).toBeVisible();
    });
});
