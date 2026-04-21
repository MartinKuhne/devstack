import type { Page, Locator } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class DashboardPage extends BasePage {
    readonly pageTitle: Locator;
    readonly welcomeText: Locator;
    readonly newProjectButton: Locator;
    readonly statCards: Locator;
    readonly deliverablesPlanningCard: Locator;
    readonly deliverablesReadyCard: Locator;
    readonly deliverablesInProgressCard: Locator;
    readonly deliverablesNeedsReviewCard: Locator;
    readonly deliverablesPlanningValue: Locator;
    readonly deliverablesReadyValue: Locator;
    readonly deliverablesInProgressValue: Locator;
    readonly deliverablesNeedsReviewValue: Locator;
    readonly emptyStateMessage: Locator;
    readonly loadingSkeletons: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Dashboard', level: 2 });
        this.welcomeText = page.getByText(/Welcome to your DevStack dashboard/);
        this.newProjectButton = page.getByRole('button', { name: 'New Project' });
        this.statCards = page.locator('[class*="grid"][class*="gap-4"] > [class*="Card"]');
        this.deliverablesPlanningCard = this.statCards.filter({ hasText: 'Planning' });
        this.deliverablesReadyCard = this.statCards.filter({ hasText: 'Ready' });
        this.deliverablesInProgressCard = this.statCards.filter({ hasText: 'In Progress' });
        this.deliverablesNeedsReviewCard = this.statCards.filter({ hasText: 'Needs Review' });
        this.deliverablesPlanningValue = this.deliverablesPlanningCard.locator('div').filter({ hasText: /^[0-9]+$/ }).first();
        this.deliverablesReadyValue = this.deliverablesReadyCard.locator('div').filter({ hasText: /^[0-9]+$/ }).first();
        this.deliverablesInProgressValue = this.deliverablesInProgressCard.locator('div').filter({ hasText: /^[0-9]+$/ }).first();
        this.deliverablesNeedsReviewValue = this.deliverablesNeedsReviewCard.locator('div').filter({ hasText: /^[0-9]+$/ }).first();
        this.emptyStateMessage = page.getByText(/No data available|Create your first project/);
        this.loadingSkeletons = page.locator('[class*="skeleton"], .animate-pulse');
    }

    async navigate(): Promise<void> {
        await super.navigate('/');
        await this.page.waitForLoadState('networkidle');
    }

    async waitForDashboardLoaded(): Promise<void> {
        await this.pageTitle.waitFor({ state: 'visible', timeout: 10000 });
        await this.loadingSkeletons.first().waitFor({ state: 'detached', timeout: 10000 }).catch(() => { });
        await this.page.waitForTimeout(1000);
    }

    async clickNewProject(): Promise<void> {
        await this.newProjectButton.click();
    }

    async getDeliverablesPlanningCount(): Promise<number> {
        const text = await this.deliverablesPlanningValue.textContent();
        return parseInt(text || '0', 10);
    }

    async getDeliverablesReadyCount(): Promise<number> {
        const text = await this.deliverablesReadyValue.textContent();
        return parseInt(text || '0', 10);
    }

    async getDeliverablesInProgressCount(): Promise<number> {
        const text = await this.deliverablesInProgressValue.textContent();
        return parseInt(text || '0', 10);
    }

    async getDeliverablesNeedsReviewCount(): Promise<number> {
        const text = await this.deliverablesNeedsReviewValue.textContent();
        return parseInt(text || '0', 10);
    }
}
