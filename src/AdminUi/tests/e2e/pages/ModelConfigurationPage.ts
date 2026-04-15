import type { Page, Locator } from '@playwright/test';
import { expect } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class ModelConfigurationListPage extends BasePage {
    readonly pageTitle: Locator;
    readonly addModelButton: Locator;
    readonly modelCards: Locator;
    readonly emptyStateMessage: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Model Configurations' });
        this.addModelButton = page.getByRole('button', { name: 'Add Model' });
        this.modelCards = page.getByRole('list').getByRole('heading');
        this.emptyStateMessage = page.getByText('No model configurations yet');
    }

    async navigate(projectId: string): Promise<void> {
        await super.navigate(`/projects/${projectId}/model-configurations`);
        await this.page.waitForLoadState('networkidle');
    }

    async clickAddModel(): Promise<void> {
        await this.addModelButton.click();
    }

    async waitForModelConfigurations(): Promise<void> {
        await this.modelCards.first().waitFor({ state: 'visible', timeout: 10000 });
    }

    async waitForEmptyState(): Promise<void> {
        await this.emptyStateMessage.waitFor({ state: 'visible' });
    }

    async getModelCount(): Promise<number> {
        const count = await this.modelCards.count();
        return count;
    }

    async clickModelCard(modelName: string): Promise<void> {
        const card = this.modelCards.filter({ hasText: modelName }).first();
        await card.click();
    }
}

export class CreateModelConfigurationDialog extends BasePage {
    readonly dialog: Locator;
    readonly urlInput: Locator;
    readonly modelInput: Locator;
    readonly aliasInput: Locator;
    readonly apiKeyInput: Locator;
    readonly showApiKeyButton: Locator;
    readonly complexitySelect: Locator;
    readonly addButton: Locator;
    readonly cancelButton: Locator;
    readonly urlError: Locator;
    readonly modelError: Locator;
    readonly complexityError: Locator;
    readonly errorMessage: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog', { name: 'Add Model Configuration' });
        this.urlInput = page.getByLabel('Endpoint URL');
        this.modelInput = page.getByLabel('Model Name');
        this.aliasInput = page.getByLabel('Alias');
        this.apiKeyInput = page.getByLabel('API Key');
        this.showApiKeyButton = page.getByRole('button', { name: 'Show' });
        this.complexitySelect = page.getByLabel('Max Complexity');
        this.addButton = page.getByRole('button', { name: 'Add Model' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
        this.urlError = page.getByText('URL is required').or(page.getByText('Invalid URL format'));
        this.modelError = page.getByText('Model name is required');
        this.complexityError = page.getByText('Max complexity must be between 1 and 10');
        this.errorMessage = page.getByText('Failed to create model configuration').or(page.getByText('An unexpected error occurred'));
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async fillForm(
        url: string,
        model: string,
        alias?: string,
        apiKey?: string,
        maxComplexity?: string
    ): Promise<void> {
        await this.urlInput.fill(url);
        await this.modelInput.fill(model);
        if (alias) await this.aliasInput.fill(alias);
        if (apiKey) await this.apiKeyInput.fill(apiKey);
        if (maxComplexity) {
            await this.complexitySelect.click();
            await this.page.getByRole('option', { name: maxComplexity }).click();
        }
    }

    async createModelConfiguration(
        url: string,
        model: string,
        alias?: string,
        apiKey?: string,
        maxComplexity?: string
    ): Promise<void> {
        await this.fillForm(url, model, alias, apiKey, maxComplexity);
        await this.addButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }

    async submitEmptyForm(): Promise<void> {
        await this.addButton.click();
    }

    async submitInvalidUrl(): Promise<void> {
        await this.urlInput.fill('not-a-valid-url');
        await this.modelInput.fill('Test Model');
        await this.addButton.click();
    }

    async submitMissingModel(): Promise<void> {
        await this.urlInput.fill('https://api.example.com/v1');
        await this.modelInput.fill('');
        await this.addButton.click();
    }

    async waitForUrlError(): Promise<void> {
        await this.urlError.waitFor({ state: 'visible' });
    }

    async waitForModelError(): Promise<void> {
        await this.modelError.waitFor({ state: 'visible' });
    }

    async waitForComplexityError(): Promise<void> {
        await this.complexityError.waitFor({ state: 'visible' });
    }

    async waitForErrorMessage(): Promise<void> {
        await this.errorMessage.waitFor({ state: 'visible' });
    }

    async toggleApiKeyVisibility(): Promise<void> {
        await this.showApiKeyButton.click();
    }

    async isApiKeyVisible(): Promise<boolean> {
        const type = await this.apiKeyInput.getAttribute('type');
        return type === 'text';
    }
}

export class ModelConfigurationDetailPage extends BasePage {
    readonly pageTitle: Locator;
    readonly modelBadge: Locator;
    readonly urlSection: Locator;
    readonly complexityBadge: Locator;
    readonly editButton: Locator;
    readonly deleteButton: Locator;
    readonly backButton: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: /Model Configuration/i });
        this.modelBadge = page.getByText(/gpt-|claude-|llama/i).first();
        this.urlSection = page.getByText('URL').locator('..');
        this.complexityBadge = page.getByText(/\d/).first();
        this.editButton = page.getByRole('button', { name: 'Edit' });
        this.deleteButton = page.getByRole('button', { name: 'Delete' });
        this.backButton = page.getByRole('link', { name: /Model Configurations/i });
    }

    async navigateToDetail(projectId: string, configId: string): Promise<void> {
        const path = `/projects/${projectId}/model-configurations/${configId}`;
        await super.navigate(path);
        await this.page.waitForLoadState('networkidle');
    }

    async waitForModelConfigurationDetail(): Promise<void> {
        await this.pageTitle.waitFor({ state: 'visible', timeout: 10000 });
    }

    async clickEdit(): Promise<void> {
        await this.editButton.click();
    }

    async clickDelete(): Promise<void> {
        await this.deleteButton.click();
    }

    async clickBack(): Promise<void> {
        await this.backButton.click();
    }

    async verifyModelName(modelName: string): Promise<void> {
        await expect(this.modelBadge).toContainText(modelName);
    }

    async verifyUrl(url: string): Promise<void> {
        await expect(this.urlSection).toContainText(url);
    }

    async verifyComplexity(complexity: string): Promise<void> {
        await expect(this.complexityBadge).toContainText(complexity);
    }
}

export class EditModelConfigurationDialog extends BasePage {
    readonly dialog: Locator;
    readonly urlInput: Locator;
    readonly modelInput: Locator;
    readonly aliasInput: Locator;
    readonly apiKeyInput: Locator;
    readonly showApiKeyButton: Locator;
    readonly complexitySelect: Locator;
    readonly saveButton: Locator;
    readonly cancelButton: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog', { name: /Edit Model Configuration/i });
        this.urlInput = page.getByLabel('Endpoint URL');
        this.modelInput = page.getByLabel('Model Name');
        this.aliasInput = page.getByLabel('Alias');
        this.apiKeyInput = page.getByLabel('API Key');
        this.showApiKeyButton = page.getByRole('button', { name: 'Show' });
        this.complexitySelect = page.getByLabel('Max Complexity');
        this.saveButton = page.getByRole('button', { name: 'Save Changes' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async fillForm(
        url?: string,
        model?: string,
        alias?: string,
        apiKey?: string,
        maxComplexity?: string
    ): Promise<void> {
        if (url) await this.urlInput.fill(url);
        if (model) await this.modelInput.fill(model);
        if (alias) await this.aliasInput.fill(alias);
        if (apiKey) await this.apiKeyInput.fill(apiKey);
        if (maxComplexity) {
            await this.complexitySelect.click();
            await this.page.getByRole('option', { name: maxComplexity }).click();
        }
    }

    async updateModelConfiguration(
        url?: string,
        model?: string,
        alias?: string,
        apiKey?: string,
        maxComplexity?: string
    ): Promise<void> {
        await this.fillForm(url, model, alias, apiKey, maxComplexity);
        await this.saveButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}
