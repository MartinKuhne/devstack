import type { Page, Locator } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class ProjectListPage extends BasePage {
    readonly pageTitle: Locator;
    readonly newProjectButton: Locator;
    readonly projectTable: Locator;
    readonly emptyStateMessage: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Projects', level: 2 });
        this.newProjectButton = page.getByRole('button', { name: /New Project|Create Project/ });
        this.projectTable = page.locator('table');
        this.emptyStateMessage = page.getByText(/No projects yet|Create your first project/);
    }

    async navigate(): Promise<void> {
        await super.navigate('/projects');
    }

    async clickNewProject(): Promise<void> {
        await this.newProjectButton.click();
    }

    async waitForProjectList(): Promise<void> {
        await this.pageTitle.waitFor({ state: 'visible', timeout: 10000 });
    }

    async clickProjectRow(projectName: string): Promise<void> {
        const row = this.page.getByRole('row').filter({ hasText: projectName }).first();
        await row.waitFor({ state: 'visible', timeout: 10000 });
        await row.click();
    }

    async getProjectCount(): Promise<number> {
        const rows = this.projectTable.locator('tbody tr');
        return await rows.count();
    }
}

export class CreateProjectDialog extends BasePage {
    readonly dialog: Locator;
    readonly nameInput: Locator;
    readonly descriptionInput: Locator;
    readonly repositoryInput: Locator;
    readonly createButton: Locator;
    readonly cancelButton: Locator;
    readonly nameError: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog').filter({ hasText: 'Create New Project' });
        this.nameInput = page.getByLabel('Name *');
        this.descriptionInput = page.getByLabel('Description');
        this.repositoryInput = page.getByLabel('Repository URL');
        this.createButton = page.getByRole('button', { name: 'Create Project' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
        this.nameError = page
            .locator('[class*="text-destructive"]')
            .filter({ hasText: 'Name is required' })
            .first();
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async fillForm(name: string, description?: string, repository?: string): Promise<void> {
        await this.nameInput.fill(name);
        if (description) await this.descriptionInput.fill(description);
        if (repository) await this.repositoryInput.fill(repository);
    }

    async createProject(name: string, description?: string, repository?: string): Promise<void> {
        await this.fillForm(name, description, repository);
        await this.createButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}

export class ProjectDetailPage extends BasePage {
    readonly pageTitle: Locator;
    readonly repositoryLink: Locator;
    readonly editButton: Locator;
    readonly deleteButton: Locator;
    readonly backToProjectsButton: Locator;
    readonly tabsList: Locator;
    readonly deliverablesTab: Locator;
    readonly agentTasksTab: Locator;
    readonly modelsTab: Locator;
    readonly deliverablesTabContent: Locator;
    readonly agentTasksTabContent: Locator;
    readonly modelsTabContent: Locator;
    readonly newDeliverableButton: Locator;
    readonly newAgentTaskButton: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { level: 2 }).first();
        this.repositoryLink = page.locator('a[href^="http"]').first();
        this.editButton = page.getByRole('button', { name: 'Edit' });
        this.deleteButton = page.getByRole('button', { name: 'Delete' });
        this.backToProjectsButton = page.getByRole('button', { name: 'Back to Projects' });
        this.tabsList = page.getByRole('tablist').first();
        this.deliverablesTab = this.page.getByRole('tab', { name: 'Deliverables' });
        this.agentTasksTab = this.page.getByRole('tab', { name: 'Agent Tasks' });
        this.modelsTab = this.page.getByRole('tab', { name: 'Models' });
        this.deliverablesTabContent = this.page
            .getByRole('tabpanel', { name: 'Deliverables' })
            .or(this.page.locator('[data-state="active"]').first());
        this.agentTasksTabContent = this.page
            .getByRole('tabpanel', { name: 'Agent Tasks' })
            .or(this.page.locator('[data-state="active"]'));
        this.modelsTabContent = this.page
            .getByRole('tabpanel', { name: 'Models' })
            .or(this.page.locator('[data-state="active"]'));
        this.newDeliverableButton = this.page.getByRole('button', { name: 'New Deliverable' });
        this.newAgentTaskButton = this.page.getByRole('button', { name: 'New Agent Task' });
    }

    async clickEdit(): Promise<void> {
        await this.editButton.click();
    }

    async clickBack(): Promise<void> {
        await this.backToProjectsButton.click();
    }

    async clickTab(tabName: string): Promise<void> {
        const tab = this.page.getByRole('tab', { name: tabName });
        await tab.click();
        await this.page.waitForTimeout(500);
    }

    async getProjectName(): Promise<string> {
        return await this.pageTitle.textContent();
    }

    async isTabSelected(tabName: string): Promise<boolean> {
        const tab = this.page.getByRole('tab', { name: tabName });
        const state = await tab.getAttribute('data-state');
        return state === 'active';
    }

    async getTabCount(): Promise<number> {
        return await this.page.getByRole('tab').count();
    }
}

export class EditProjectDialog extends BasePage {
    readonly dialog: Locator;
    readonly nameInput: Locator;
    readonly saveButton: Locator;
    readonly cancelButton: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog').filter({ hasText: /Edit Project/i });
        this.nameInput = page.getByLabel('Name *');
        this.saveButton =
            page.getByRole('button', { name: /Save|Update/ }) ||
            page.getByRole('button', { name: 'Save Changes' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async updateProject(name?: string): Promise<void> {
        if (name) await this.nameInput.fill(name);
        await this.saveButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}
