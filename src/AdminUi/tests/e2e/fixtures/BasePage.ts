import type { Page, Locator } from '@playwright/test';
import { expect } from '@playwright/test';

export class BasePage {
    readonly page: Page;

    constructor(page: Page) {
        this.page = page;
    }

    async navigate(path: string): Promise<void> {
        await this.page.goto(path);
        await this.page.waitForLoadState('networkidle').catch(() => {});
    }

    async waitForLoadingSpinner(): Promise<void> {
        await this.page.locator('.animate-pulse, [class*="skeleton"]').first().waitFor({ state: 'detached', timeout: 10000 }).catch(() => {});
    }

    async waitForElement(selector: string, timeout = 10000): Promise<void> {
        await this.page.waitForSelector(selector, { timeout });
    }

    getErrorBoundaryErrors(): Locator {
        return this.page.getByRole('heading', { name: 'Something went wrong', level: 2 });
    }

    getErrorStateErrors(): Locator {
        return this.page.locator('h3.font-semibold.text-destructive, h3:has-text("Error"), h3:has-text("Error loading")');
    }

    getErrorMessages(): Locator {
        return this.page.locator('[class*="destructive"], [class*="error"], [class*="alert"]');
    }

    async getAnyErrorMessageText(): Promise<string> {
        const errorBoundary = await this.getErrorBoundaryErrors().count();
        if (errorBoundary > 0) {
            return await this.getErrorBoundaryErrors().textContent();
        }

        const errorStates = await this.getErrorStateErrors().allTextContents();
        if (errorStates.length > 0) {
            return errorStates.join('; ');
        }

        const errorMessages = await this.getErrorMessages().allTextContents();
        const filtered = errorMessages.filter(text =>
            text.toLowerCase().includes('error') ||
            text.toLowerCase().includes('failed') ||
            text.toLowerCase().includes('not found') ||
            text.toLowerCase().includes('unexpected')
        );
        if (filtered.length > 0) {
            return filtered.join('; ');
        }

        return '';
    }

    async expectNoErrors(): Promise<void> {
        await expect(this.getErrorBoundaryErrors()).not.toBeVisible();
        await expect(this.getErrorStateErrors()).not.toBeVisible();
    }
}
