import type { Page, Locator } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class DashboardPage extends BasePage {
    readonly pageTitle: Locator;
    readonly welcomeText: Locator;
    readonly newProjectButton: Locator;
    readonly statCards: Locator;
    readonly planningCard: Locator;
    readonly readyCard: Locator;
    readonly inProgressCard: Locator;
    readonly needsReviewCard: Locator;
    readonly emptyStateMessage: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Dashboard', level: 2 });
        this.welcomeText = page.getByText(/Welcome to your DevStack dashboard/);
        this.newProjectButton = page.getByRole('button', { name: 'New Project' });
        this.statCards = page.locator(
            '[class*="grid"] > div[class*="Card"], [class*="space-y-6"] > div'
        );
        this.planningCard = page.getByText('Planning').locator('..').locator('..').first();
        this.readyCard = page.getByText('Ready').locator('..').locator('..').nth(0);
        this.inProgressCard = page.getByText('In Progress').locator('..').locator('..');
        this.needsReviewCard = page.getByText('Needs Review').locator('..').locator('..');
        this.emptyStateMessage = page.getByText(/No data available|Create your first project/);
    }

    async navigate(): Promise<void> {
        await super.navigate('/');
    }

    async waitForDashboardLoaded(): Promise<void> {
        await this.pageTitle.waitFor({ state: 'visible', timeout: 10000 });
    }

    async getStatValue(statName: string): Promise<number> {
        const card = this.page.getByText(statName).locator('..').locator('..');
        const badge = card.locator('[class*="badge"], [class*="Badge"]');
        const text = await badge.textContent();
        return parseInt(text?.replace(/\D/g, '') || '0', 10);
    }
}
