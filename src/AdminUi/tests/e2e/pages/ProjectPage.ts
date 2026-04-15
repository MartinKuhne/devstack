import type { Page, Locator } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage';

export class ProjectListPage extends BasePage {
    readonly pageTitle: Locator;
    readonly newProjectButton: Locator;
    readonly projectTable: Locator;
    readonly emptyStateMessage: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Projects' });
        this.newProjectButton = page.getByRole('button', { name: 'New Project' });
        this.projectTable = page.getByRole('table');
        this.emptyStateMessage = page.getByText('No projects yet');
    }

    async navigate(): Promise<void> {
        await super.navigate('/projects');
        await this.page.waitForLoadState('networkidle');
    }

    async clickNewProject(): Promise<void> {
        await this.newProjectButton.click();
    }

    async waitForProjectList(): Promise<void> {
        await this.projectTable.waitFor({ state: 'visible', timeout: 10000 });
    }

    async waitForEmptyState(): Promise<void> {
        await this.emptyStateMessage.waitFor({ state: 'visible' });
    }

    async clickProjectRow(projectName: string): Promise<void> {
        const row = this.projectTable.getByRole('row', { name: new RegExp(projectName, 'i') });
        await row.click();
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
    readonly urlError: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog', { name: 'Create New Project' });
        this.nameInput = page.getByLabel('Name *');
        this.descriptionInput = page.getByLabel('Description');
        this.architectureInput = page.getByLabel('Architecture');
        this.memoryInput = page.getByLabel('Memory');
        this.githubUrlInput = page.getByLabel('GitHub URL');
        this.createButton = page.getByRole('button', { name: 'Create Project' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
        this.nameError = page.getByText('Name is required');
        this.urlError = page.getByText('Invalid URL');
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

    async submitEmptyForm(): Promise<void> {
        await this.createButton.click();
    }

    async submitInvalidUrl(): Promise<void> {
        await this.nameInput.fill('Test Project');
        await this.githubUrlInput.fill('not-a-valid-url');
        await this.createButton.click();
    }

    async waitForNameError(): Promise<void> {
        await this.nameError.waitFor({ state: 'visible' });
    }

    async waitForUrlError(): Promise<void> {
        await this.urlError.waitFor({ state: 'visible' });
    }
}

export class ProjectDetailPage extends BasePage {
    readonly pageTitle: Locator;
    readonly editButton: Locator;
    readonly deleteButton: Locator;
    readonly backToProjectsButton: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: /Project:/i });
        this.editButton = page.getByRole('button', { name: 'Edit' });
        this.deleteButton = page.getByRole('button', { name: 'Delete' });
        this.backToProjectsButton = page.getByRole('link', { name: /Projects/i });
    }

    async navigate(projectId: string): Promise<void> {
        await super.navigate(`/projects/${projectId}`);
        await this.page.waitForLoadState('networkidle');
    }

    async waitForProjectDetail(): Promise<void> {
        await this.pageTitle.waitFor({ state: 'visible', timeout: 10000 });
    }

    async clickEdit(): Promise<void> {
        await this.editButton.click();
    }
}

export class EditProjectDialog extends BasePage {
    readonly dialog: Locator;
    readonly nameInput: Locator;
    readonly descriptionInput: Locator;
    readonly githubUrlInput: Locator;
    readonly saveButton: Locator;
    readonly cancelButton: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog', { name: /Edit Project/i });
        this.nameInput = page.getByLabel('Name *');
        this.descriptionInput = page.getByLabel('Description');
        this.githubUrlInput = page.getByLabel('GitHub URL');
        this.saveButton = page.getByRole('button', { name: 'Save Changes' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async updateProject(name?: string, description?: string, githubUrl?: string): Promise<void> {
        if (name) await this.nameInput.fill(name);
        if (description) await this.descriptionInput.fill(description);
        if (githubUrl) await this.githubUrlInput.fill(githubUrl);
        await this.saveButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}
