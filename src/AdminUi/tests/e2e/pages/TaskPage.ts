import type { Page, Locator } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class TaskBoardPage extends BasePage {
    readonly pageTitle: Locator;
    readonly newTaskButton: Locator;
    readonly taskBoard: Locator;
    readonly todoColumn: Locator;
    readonly inProgressColumn: Locator;
    readonly reviewColumn: Locator;
    readonly doneColumn: Locator;
    readonly emptyStateMessage: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Task Board' });
        this.newTaskButton = page.getByRole('button', { name: 'New Task' });
        this.taskBoard = page.getByText('To Do').locator('..').locator('..');
        this.todoColumn = page.getByText('To Do').locator('..').locator('..');
        this.inProgressColumn = page.getByText('In Progress').locator('..').locator('..');
        this.reviewColumn = page.getByText('Review').locator('..').locator('..');
        this.doneColumn = page.getByText('Done').locator('..').locator('..');
        this.emptyStateMessage = page.getByText('No tasks');
    }

    async navigate(featureId: string): Promise<void> {
        await super.navigate(`/features/${featureId}`);
        await this.page.waitForLoadState('networkidle');
        await this.page.getByRole('tab', { name: 'Tasks' }).click();
        await this.page.waitForLoadState('networkidle');
    }

    async clickNewTask(): Promise<void> {
        await this.newTaskButton.click();
    }

    async waitForTaskBoard(): Promise<void> {
        await this.taskBoard.waitFor({ state: 'visible', timeout: 10000 });
    }

    async waitForEmptyState(): Promise<void> {
        await this.emptyStateMessage.waitFor({ state: 'visible' });
    }

    async clickTaskCard(taskTitle: string): Promise<void> {
        const card = this.page.getByRole('heading', { name: taskTitle }).locator('..').locator('..');
        await card.click();
    }

    async waitForTaskCard(taskTitle: string): Promise<void> {
        await this.page.getByText(taskTitle).waitFor({ state: 'visible', timeout: 10000 });
    }

    async getTaskCountInColumn(columnName: string): Promise<number> {
        const column = this.page.getByText(columnName).locator('..').locator('..');
        const badge = column.getByRole('status');
        const text = await badge.textContent();
        return text ? parseInt(text, 10) : 0;
    }
}

export class CreateTaskDialog extends BasePage {
    readonly dialog: Locator;
    readonly titleInput: Locator;
    readonly deliverableInput: Locator;
    readonly acceptanceCriteriaInput: Locator;
    readonly risksInput: Locator;
    readonly followUpsInput: Locator;
    readonly complexitySelect: Locator;
    readonly createButton: Locator;
    readonly cancelButton: Locator;
    readonly titleError: Locator;
    readonly complexityError: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog', { name: 'Create New Task' });
        this.titleInput = page.getByLabel('Title *');
        this.deliverableInput = page.getByLabel('Deliverable');
        this.acceptanceCriteriaInput = page.getByLabel('Acceptance Criteria');
        this.risksInput = page.getByLabel('Risks');
        this.followUpsInput = page.getByLabel('Required Follow-ups');
        this.complexitySelect = page.getByLabel('Complexity *');
        this.createButton = page.getByRole('button', { name: 'Create Task' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
        this.titleError = page.getByText('Title is required');
        this.complexityError = page.getByText('Feature is required');
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async fillForm(title: string, deliverable?: string, acceptanceCriteria?: string, risks?: string, followUps?: string, complexity?: string): Promise<void> {
        await this.titleInput.fill(title);
        if (deliverable) await this.deliverableInput.fill(deliverable);
        if (acceptanceCriteria) await this.acceptanceCriteriaInput.fill(acceptanceCriteria);
        if (risks) await this.risksInput.fill(risks);
        if (followUps) await this.followUpsInput.fill(followUps);
        if (complexity) await this.complexitySelect.selectOption(complexity);
    }

    async createTask(title: string, deliverable?: string, acceptanceCriteria?: string, risks?: string, followUps?: string, complexity?: string): Promise<void> {
        await this.fillForm(title, deliverable, acceptanceCriteria, risks, followUps, complexity);
        await this.createButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }

    async submitEmptyForm(): Promise<void> {
        await this.createButton.click();
    }

    async submitInvalidForm(): Promise<void> {
        await this.titleInput.fill('');
        await this.createButton.click();
    }

    async waitForTitleError(): Promise<void> {
        await this.titleError.waitFor({ state: 'visible' });
    }

    async waitForComplexityError(): Promise<void> {
        await this.complexityError.waitFor({ state: 'visible' });
    }
}

export class TaskDetailDrawer extends BasePage {
    readonly drawer: Locator;
    readonly taskTitle: Locator;
    readonly taskComplexity: Locator;
    readonly taskStatus: Locator;
    readonly editButton: Locator;
    readonly closeButton: Locator;
    readonly statusTransitionSelect: Locator;
    readonly transitionButton: Locator;

    constructor(page: Page) {
        super(page);
        this.drawer = page.getByRole('dialog');
        this.taskTitle = page.getByRole('heading', { name: /Task/i }).first();
        this.taskComplexity = page.getByText(/Simple|Moderate|Complex|Major/i);
        this.taskStatus = page.getByText(/Todo|InProgress|Review|Done/i);
        this.editButton = page.getByRole('button', { name: 'Edit' });
        this.closeButton = page.getByRole('button', { name: 'Close' });
        this.statusTransitionSelect = page.getByLabel('Target').first();
        this.transitionButton = page.getByRole('button', { name: 'Transition' });
    }

    async isOpen(): Promise<boolean> {
        return await this.drawer.isVisible();
    }

    async waitForTaskDetail(taskTitle: string): Promise<void> {
        await this.page.getByText(taskTitle).waitFor({ state: 'visible', timeout: 10000 });
    }

    async clickEdit(): Promise<void> {
        await this.editButton.click();
    }

    async close(): Promise<void> {
        await this.closeButton.click();
    }

    async selectStatusTransition(status: string): Promise<void> {
        await this.statusTransitionSelect.selectOption(status);
    }

    async clickTransition(): Promise<void> {
        await this.transitionButton.click();
    }

    async waitForStatusChange(expectedStatus: string): Promise<void> {
        await this.page.getByText(expectedStatus).waitFor({ state: 'visible', timeout: 10000 });
    }
}

export class EditTaskDialog extends BasePage {
    readonly dialog: Locator;
    readonly titleInput: Locator;
    readonly complexitySelect: Locator;
    readonly statusSelect: Locator;
    readonly deliverableInput: Locator;
    readonly acceptanceCriteriaInput: Locator;
    readonly risksInput: Locator;
    readonly followUpsInput: Locator;
    readonly saveButton: Locator;
    readonly cancelButton: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog', { name: /Edit Task/i });
        this.titleInput = page.getByLabel('Title *');
        this.complexitySelect = page.getByLabel('Complexity *');
        this.statusSelect = page.getByLabel('Status *');
        this.deliverableInput = page.getByLabel('Deliverable');
        this.acceptanceCriteriaInput = page.getByLabel('Acceptance Criteria');
        this.risksInput = page.getByLabel('Risks');
        this.followUpsInput = page.getByLabel('Required Follow-ups');
        this.saveButton = page.getByRole('button', { name: 'Save Changes' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async fillForm(title?: string, complexity?: string, status?: string, deliverable?: string, acceptanceCriteria?: string, risks?: string, followUps?: string): Promise<void> {
        if (title) await this.titleInput.fill(title);
        if (complexity) await this.complexitySelect.selectOption(complexity);
        if (status) await this.statusSelect.selectOption(status);
        if (deliverable) await this.deliverableInput.fill(deliverable);
        if (acceptanceCriteria) await this.acceptanceCriteriaInput.fill(acceptanceCriteria);
        if (risks) await this.risksInput.fill(risks);
        if (followUps) await this.followUpsInput.fill(followUps);
    }

    async updateTask(title?: string, complexity?: string, status?: string, deliverable?: string, acceptanceCriteria?: string, risks?: string, followUps?: string): Promise<void> {
        await this.fillForm(title, complexity, status, deliverable, acceptanceCriteria, risks, followUps);
        await this.saveButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}
