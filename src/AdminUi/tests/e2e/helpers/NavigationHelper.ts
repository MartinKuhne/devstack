import type { Page } from '@playwright/test';

export class NavigationHelper {
    private page: Page;

    constructor(page: Page) {
        this.page = page;
    }

    async navigateToDashboard(): Promise<void> {
        await this.page.goto('/');
    }

    async navigateToProjects(): Promise<void> {
        await this.page.goto('/projects');
    }

    async navigateToProjectDetail(projectId: string): Promise<void> {
        await this.page.goto(`/projects/${projectId}`);
    }

    async navigateToFeatures(): Promise<void> {
        await this.page.goto('/features');
    }

    async navigateToFeatureDetail(featureId: string): Promise<void> {
        await this.page.goto(`/features/${featureId}`);
    }

    async navigateToTasks(): Promise<void> {
        await this.page.goto('/tasks');
    }

    async navigateToDefects(): Promise<void> {
        await this.page.goto('/defects');
    }

    async navigateToModelConfigurations(): Promise<void> {
        await this.page.goto('/model-configurations');
    }

    async waitForPageLoad(): Promise<void> {
        await this.page.waitForLoadState('networkidle');
        await this.page.waitForTimeout(500);
    }
}
