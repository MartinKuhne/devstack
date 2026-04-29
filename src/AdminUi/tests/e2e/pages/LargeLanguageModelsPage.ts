import type { Page, Locator } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class LargeLanguageModelsPage extends BasePage {
    readonly pageTitle: Locator;
    readonly addModelButton: Locator;
    readonly modelListCard: Locator;
    readonly emptyStateMessage: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Large Language Models', level: 2 });
        this.addModelButton = page.getByRole('button', { name: 'Add Model' }).first();
        this.modelListCard = page
            .getByRole('heading', { name: 'Model Configurations' })
            .locator('..');
        this.emptyStateMessage = page.getByText(/No model configurations|Configure/);
    }

    async navigate(): Promise<void> {
        await super.navigate('/models');
    }

    async waitForPageLoaded(): Promise<void> {
        await this.pageTitle.waitFor({ state: 'visible', timeout: 10000 });
    }

    async clickAddModel(): Promise<void> {
        await this.addModelButton.click();
    }
}

export class LargeLanguageModelDialog extends BasePage {
    readonly dialog: Locator;
    readonly urlInput: Locator;
    readonly modelInput: Locator;
    readonly aliasInput: Locator;
    readonly apiKeyInput: Locator;
    readonly costInput: Locator;
    readonly complexitySelect: Locator;
    readonly addButton: Locator;
    readonly saveButton: Locator;
    readonly cancelButton: Locator;
    readonly errorMessage: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog');
        this.urlInput = page.getByLabel('Endpoint URL');
        this.modelInput = page.getByLabel('Model Name');
        this.aliasInput = page.getByLabel('Alias (optional)') || page.locator('#alias');
        this.apiKeyInput = page.getByLabel('API Key');
        this.costInput = page.getByLabel('Cost (0-100)');
        this.complexitySelect = page.getByLabel('Max Complexity (1-10)');
        this.addButton = page.getByRole('button', { name: 'Add Model' });
        this.saveButton =
            page.getByRole('button', { name: 'Save' }) ||
            page.getByRole('button', { name: 'Save Changes' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
        this.errorMessage = page.locator('[class*="text-destructive"], [class*="bg-destructive"]');
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async fillForm(
        url: string,
        modelName: string,
        alias?: string,
        apiKey?: string,
        cost?: string,
        complexity?: string
    ): Promise<void> {
        await this.urlInput.fill(url);
        await this.modelInput.fill(modelName);
        if (alias) await this.aliasInput.fill(alias);
        if (apiKey) await this.apiKeyInput.fill(apiKey);
        if (cost) await this.costInput.fill(cost);
        if (complexity) await this.complexitySelect.selectOption(complexity);
    }

    async createModel(
        url: string,
        modelName: string,
        alias?: string,
        apiKey?: string,
        cost?: string,
        complexity?: string
    ): Promise<void> {
        await this.fillForm(url, modelName, alias, apiKey, cost, complexity);
        if (await this.addButton.isVisible()) {
            await this.addButton.click();
        } else if (await this.saveButton.isVisible()) {
            await this.saveButton.click();
        } else {
            await this.dialog.getByRole('button', { name: /Add|Save/ }).click();
        }
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }

    async getErrorMessage(): Promise<string | null> {
        if (await this.errorMessage.isVisible()) {
            return await this.errorMessage.textContent();
        }
        return null;
    }
}
