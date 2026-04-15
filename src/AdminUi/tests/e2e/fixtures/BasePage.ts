import type { Page } from '@playwright/test';

export class BasePage {
    readonly page: Page;

    constructor(page: Page) {
        this.page = page;
    }

    async navigate(path: string): Promise<void> {
        await this.page.goto(path);
    }

    async waitForLoadingSpinner(): Promise<void> {
        await this.page.waitForSelector('loading-spinner, [data-testid="loading"]', {
            state: 'detached',
            timeout: 10000,
        }).catch(() => {
        });
    }

    async waitForElement(selector: string, timeout = 10000): Promise<void> {
        await this.page.waitForSelector(selector, { timeout });
    }

    async takeScreenshot(name: string): Promise<void> {
        await this.page.screenshot({
            path: `../reports/screenshots/${name}-${Date.now()}.png`,
        });
    }
}
