import type { Page, Locator } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class NavigationHelper extends BasePage {
    readonly dashboardLink: Locator;
    readonly projectsLink: Locator;
    readonly modelsLink: Locator;
    readonly projectDropdown: Locator;

    constructor(page: Page) {
        super(page);
        this.dashboardLink = page.getByRole('link', { name: /Dashboard/ }).first();
        this.projectsLink = page.getByRole('link', { name: /Projects/ }).first();
        this.modelsLink = page.getByRole('link', { name: /Large Language Models/ });
        this.projectDropdown = page.locator('[role="combobox"]').filter({ has: page.getByText(/Select Project/) }).first();
    }

    async navigateToDashboard(): Promise<void> {
        await super.navigate('/');
    }

    async navigateToProjects(): Promise<void> {
        await super.navigate('/projects');
    }

    async navigateToModels(): Promise<void> {
        await super.navigate('/models');
    }

    async selectProject(projectName: string): Promise<void> {
        try {
            if (await this.projectDropdown.isVisible()) {
                await this.projectDropdown.click();
                await page.getByText(projectName).click();
                await page.waitForTimeout(500);
            }
        } catch {
            // Project dropdown may not be available on all pages
        }
    }
}
