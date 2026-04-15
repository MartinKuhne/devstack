import type { Page, Locator } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class DashboardPage extends BasePage {
    readonly pageTitle: Locator;
    readonly welcomeText: Locator;
    readonly refreshButton: Locator;
    readonly newProjectButton: Locator;
    readonly createProjectDialog: Locator;
    readonly projectsInFlightCard: Locator;
    readonly featuresInReviewCard: Locator;
    readonly featuresFailedCard: Locator;
    readonly tasksInProgressCard: Locator;
    readonly tasksFailedCard: Locator;
    readonly projectsInFlightValue: Locator;
    readonly featuresInReviewValue: Locator;
    readonly featuresFailedValue: Locator;
    readonly tasksInProgressValue: Locator;
    readonly tasksFailedValue: Locator;
    readonly auditEventsTable: Locator;
    readonly emptyStateMessage: Locator;
    readonly noActivityMessage: Locator;
    readonly errorCard: Locator;
    readonly errorMessage: Locator;
    readonly viewFailedFeaturesButton: Locator;
    readonly viewFailedTasksButton: Locator;
    readonly loadingSkeletons: Locator;
    readonly refreshingIndicator: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Dashboard' });
        this.welcomeText = page.getByText('Welcome to your DevStack dashboard');
        this.refreshButton = page.getByRole('button', { name: 'Refresh' });
        this.newProjectButton = page.getByRole('button', { name: 'New Project' });
        this.createProjectDialog = page.getByRole('dialog', { name: 'Create New Project' });
        this.projectsInFlightCard = page.getByText('Projects In Flight').locator('..').locator('..');
        this.featuresInReviewCard = page.getByText('Features In Review').locator('..').locator('..');
        this.featuresFailedCard = page.getByText('Features Failed').locator('..').locator('..');
        this.tasksInProgressCard = page.getByText('Tasks In Progress').locator('..').locator('..');
        this.tasksFailedCard = page.getByText('Tasks Failed').locator('..').locator('..');
        this.projectsInFlightValue = this.projectsInFlightCard.getByRole('heading', { level: 3 }).last();
        this.featuresInReviewValue = this.featuresInReviewCard.getByRole('heading', { level: 3 }).last();
        this.featuresFailedValue = this.featuresFailedCard.getByRole('heading', { level: 3 }).last();
        this.tasksInProgressValue = this.tasksInProgressCard.getByRole('heading', { level: 3 }).last();
        this.tasksFailedValue = this.tasksFailedCard.getByRole('heading', { level: 3 }).last();
        this.auditEventsTable = page.getByRole('table');
        this.emptyStateMessage = page.getByText('No data available yet');
        this.noActivityMessage = page.getByText('No recent activity');
        this.errorCard = page.getByRole('heading', { name: 'Error loading dashboard' }).locator('..');
        this.errorMessage = this.errorCard.getByText('Internal server error');
        this.viewFailedFeaturesButton = page.getByRole('button', { name: /View Failed Features/ });
        this.viewFailedTasksButton = page.getByRole('button', { name: /View Failed Tasks/ });
        this.loadingSkeletons = page.locator('.animate-pulse');
        this.refreshingIndicator = page.getByText('Refreshing...');
    }

    async navigate(): Promise<void> {
        await super.navigate('/dashboard');
        await this.page.waitForLoadState('networkidle');
    }

    async waitForDashboardLoaded(): Promise<void> {
        await this.pageTitle.waitFor({ state: 'visible', timeout: 10000 });
        await this.loadingSkeletons.first().waitFor({ state: 'detached', timeout: 10000 }).catch(() => { });
    }

    async clickRefresh(): Promise<void> {
        await this.refreshButton.click();
    }

    async clickNewProject(): Promise<void> {
        await this.newProjectButton.click();
    }

    async clickViewFailedFeatures(): Promise<void> {
        await this.viewFailedFeaturesButton.click();
    }

    async clickViewFailedTasks(): Promise<void> {
        await this.viewFailedTasksButton.click();
    }

    async getProjectsInFlightCount(): Promise<number> {
        const text = await this.projectsInFlightValue.textContent();
        return parseInt(text || '0', 10);
    }

    async getFeaturesInReviewCount(): Promise<number> {
        const text = await this.featuresInReviewValue.textContent();
        return parseInt(text || '0', 10);
    }

    async getFeaturesFailedCount(): Promise<number> {
        const text = await this.featuresFailedValue.textContent();
        return parseInt(text || '0', 10);
    }

    async getTasksInProgressCount(): Promise<number> {
        const text = await this.tasksInProgressValue.textContent();
        return parseInt(text || '0', 10);
    }

    async getTasksFailedCount(): Promise<number> {
        const text = await this.tasksFailedValue.textContent();
        return parseInt(text || '0', 10);
    }

    async getAuditEventCount(): Promise<number> {
        const rows = await this.page.getByRole('row').all();
        return rows.length - 1;
    }
}
