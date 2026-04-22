import type { Page, Locator } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class AgentTaskListPage extends BasePage {
    readonly pageTitle: Locator;
    readonly taskTable: Locator;
    readonly statusFilter: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Agent Tasks', level: 2 });
        this.taskTable = page.locator('table');
        this.statusFilter =
            page.getByPlaceholder('Filter by status') ||
            page
                .locator('[role="combobox"]')
                .filter({ has: page.getByText(/status/i) })
                .first();
    }

    async navigate(): Promise<void> {
        await super.navigate('/agent-tasks');
    }

    async waitForTaskList(): Promise<void> {
        await this.pageTitle.waitFor({ state: 'visible', timeout: 10000 });
    }

    async clickTaskRow(title: string): Promise<void> {
        const row = this.taskTable.getByRole('row').filter({ hasText: title }).first();
        await row.click();
    }

    async getTaskCount(): Promise<number> {
        const rows = this.taskTable.locator('tbody tr');
        return await rows.count();
    }

    async selectStatusFilter(status: string): Promise<void> {
        if (status === 'all') {
            await this.navigate('/agent-tasks');
        } else {
            await this.navigate(`/agent-tasks?status=${status}`);
        }
    }
}

export class CreateAgentTaskDialog extends BasePage {
    readonly dialog: Locator;
    readonly titleInput: Locator;
    readonly promptTextarea: Locator;
    readonly createButton: Locator;
    readonly cancelButton: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog').filter({ hasText: /Create.*Agent Task|Add.*Task/i });
        this.titleInput = page.getByLabel(/Title/) || page.locator('[id*="title"]');
        this.promptTextarea =
            page.getByLabel(/Prompt/) || page.locator('[class*="textarea"] textarea').first();
        this.createButton = page.getByRole('button', { name: /Create|Add/ });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async fillForm(title: string, prompt?: string): Promise<void> {
        await this.titleInput.fill(title);
        if (prompt) await this.promptTextarea.fill(prompt);
    }

    async createTask(title: string, prompt?: string): Promise<void> {
        await this.fillForm(title, prompt);
        await this.createButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}

export class AgentTaskDetailPage extends BasePage {
    readonly statusBadge: Locator;
    readonly updateStatusButton: Locator;
    readonly changeStatusSelect: Locator;
    readonly backToListLink: Locator;
    readonly deleteButton: Locator;

    constructor(page: Page) {
        super(page);
        this.statusBadge = page.locator('[class*="badge"], [class*="Badge"]');
        this.updateStatusButton =
            page.getByRole('button', { name: /Update Status/ }) ||
            page.getByRole('button', { name: 'Change Status' });
        this.changeStatusSelect =
            page.getByPlaceholder('Select new status') ||
            page
                .locator('[role="combobox"]')
                .filter({ has: page.getByText(/status/i) })
                .first();
        this.backToListLink = page.getByRole('link', { name: /Back to List|Agent Tasks/ }).first();
        this.deleteButton = page.getByRole('button', { name: 'Delete' });
    }

    async navigate(taskId: string): Promise<void> {
        await super.navigate(`/agent-tasks/${taskId}`);
    }

    async waitForDetailLoaded(): Promise<void> {
        await this.statusBadge.waitFor({ state: 'visible', timeout: 10000 });
    }

    async getStatus(): Promise<string | null> {
        const badges = this.page.locator('[class*="badge"], [class*="Badge"]');
        for (let i = 0; i < (await badges.count()); i++) {
            const text = await badges.nth(i).textContent();
            if (
                text &&
                ['READY', 'IN_PROGRESS', 'NEEDS_REVIEW', 'DONE', 'FAILED', 'REJECTED'].includes(
                    text
                )
            ) {
                return text;
            }
        }
        return null;
    }

    async changeStatus(newStatus: string): Promise<void> {
        try {
            await this.changeStatusSelect.click();
            await this.page.getByText(newStatus).click();
            if (await this.updateStatusButton.isVisible()) {
                await this.updateStatusButton.click();
            } else {
                await this.page.waitForTimeout(1000);
            }
        } catch {
            const statusOption = this.page.getByRole('option', { name: newStatus }).first();
            if (await statusOption.isVisible()) {
                await statusOption.click();
            }
        }
    }

    async clickBack(): Promise<void> {
        await this.backToListLink.click();
    }

    async verifyTitle(title: string): Promise<boolean> {
        return await this.page
            .getByRole('heading', { name: title, level: 2 })
            .isVisible()
            .catch(() => false);
    }
}
