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
        const row = this.projectTable.getByRole('row').filter({ hasText: projectName }).first();
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
    readonly architectureInput: Locator;
    readonly memoryInput: Locator;
    readonly githubUrlInput: Locator;
    readonly createButton: Locator;
    readonly cancelButton: Locator;
    readonly nameError: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog').filter({ hasText: 'Create New Project' });
        this.nameInput = page.getByLabel('Name *');
        this.descriptionInput = page.getByLabel('Description');
        this.architectureInput = page.getByLabel('Architecture');
        this.memoryInput = page.getByLabel('Memory');
        this.githubUrlInput = page.getByLabel('GitHub URL');
        this.createButton = page.getByRole('button', { name: 'Create Project' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
        this.nameError = page.locator('[class*="text-destructive"]').filter({ hasText: 'Name is required' }).first();
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async fillForm(name: string, description?: string, architecture?: string, memory?: string, githubUrl?: string): Promise<void> {
        await this.nameInput.fill(name);
        if (description) await this.descriptionInput.fill(description);
        if (architecture) await this.architectureInput.fill(architecture);
        if (memory) await this.memoryInput.fill(memory);
        if (githubUrl) await this.githubUrlInput.fill(githubUrl);
    }

    async createProject(name: string, description?: string, architecture?: string, memory?: string, githubUrl?: string): Promise<void> {
        await this.fillForm(name, description, architecture, memory, githubUrl);
        await this.createButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}

export class ProjectDetailPage extends BasePage {
    readonly editButton: Locator;
    readonly deleteButton: Locator;
    readonly backToProjectsLink: Locator;

    constructor(page: Page) {
        super(page);
        this.editButton = page.getByRole('button', { name: 'Edit' });
        this.deleteButton = page.getByRole('button', { name: 'Delete' });
        this.backToProjectsLink = page.getByRole('link', { name: /Projects/ }).first();
    }

    async clickEdit(): Promise<void> {
        await this.editButton.click();
    }

    async clickBack(): Promise<void> {
        await this.backToProjectsLink.click();
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
        this.saveButton = page.getByRole('button', { name: /Save|Update/ }) || page.getByRole('button', { name: 'Save Changes' });
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
