import type { Page } from '@playwright/test';

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
}
