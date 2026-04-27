import type { Page, Locator } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class DeliverableListPage extends BasePage {
    readonly pageTitle: Locator;
    readonly newDeliverableButton: Locator;
    readonly deliverableTable: Locator;
    readonly statusFilter: Locator;
    readonly typeFilter: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Deliverables', level: 2 });
        this.newDeliverableButton = page.getByRole('button', { name: /New Deliverable/ });
        this.deliverableTable = page.locator('table');
        this.statusFilter =
            page.getByPlaceholder('Filter by status') || page.locator('[class*="select"] select');
        this.typeFilter =
            page.getByPlaceholder('Filter by type') || page.locator('[class*="select"]').first();
    }

    async navigate(): Promise<void> {
        await super.navigate('/deliverables');
    }

    async clickNewDeliverable(): Promise<void> {
        await this.newDeliverableButton.click();
    }

    async waitForDeliverableList(): Promise<void> {
        await this.pageTitle.waitFor({ state: 'visible', timeout: 10000 });
    }

    async clickDeliverableRow(title: string): Promise<void> {
        const row = this.deliverableTable.getByRole('row').filter({ hasText: title }).first();
        await row.click();
    }

    async getDeliverableCount(): Promise<number> {
        const rows = this.deliverableTable.locator('tbody tr');
        return await rows.count();
    }

    async selectStatusFilter(status: string): Promise<void> {
        // Use URL params for status filter since the Select component may be hard to interact with
        if (status === 'all') {
            await this.page.goto('/deliverables');
        } else {
            await this.page.goto(`/deliverables?status=${status}`);
        }
    }
}

export class CreateDeliverableDialog extends BasePage {
    readonly dialog: Locator;
    readonly typeSelect: Locator;
    readonly titleInput: Locator;
    readonly descriptionInput: Locator;
    readonly acceptanceCriteriaInput: Locator;
    readonly initialStatusSelect: Locator;
    readonly createButton: Locator;
    readonly cancelButton: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog').filter({ hasText: 'Create New Deliverable' });
        this.typeSelect = page.locator('[id="type"]');
        this.titleInput = page.getByLabel('Title *');
        this.descriptionInput = page.getByLabel('Description');
        this.acceptanceCriteriaInput = page.getByLabel('Acceptance Criteria');
        this.initialStatusSelect = page.locator('[id="initialStatus"]');
        this.createButton = page.getByRole('button', { name: 'Create Deliverable' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async selectType(type: string): Promise<void> {
        await this.typeSelect.click();
        await this.page.getByRole('option', { name: type }).click();
    }

    async fillForm(
        title: string,
        description?: string,
        acceptanceCriteria?: string
    ): Promise<void> {
        await this.titleInput.fill(title);
        if (description) await this.descriptionInput.fill(description);
        if (acceptanceCriteria) await this.acceptanceCriteriaInput.fill(acceptanceCriteria);
    }

    async createDeliverable(
        title: string,
        description?: string,
        acceptanceCriteria?: string
    ): Promise<void> {
        await this.fillForm(title, description, acceptanceCriteria);
        await this.createButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}

export class DeliverableDetailPage extends BasePage {
    readonly editButton: Locator;
    readonly deleteButton: Locator;
    readonly backToListLink: Locator;
    readonly statusBadge: Locator;
    readonly changeStatusSelect: Locator;
    readonly updateStatusButton: Locator;
    readonly descriptionBlock: Locator;
    readonly acceptanceCriteriaBlock: Locator;
    readonly executionPlanBlock: Locator;
    readonly agentTasksSection: Locator;

    constructor(page: Page) {
        super(page);
        this.editButton = page.getByRole('button', { name: 'Edit' });
        this.deleteButton = page.getByRole('button', { name: 'Delete' });
        this.backToListLink = page.getByRole('link', { name: /Back to List|Deliverables/ }).first();
        this.statusBadge = page.locator('[class*="badge"], [class*="Badge"]');
        this.changeStatusSelect =
            page.getByPlaceholder('Select new status') ||
            page.locator('[role="combobox"]').filter({ has: page.getByText(/status/i) });
        this.updateStatusButton =
            page.getByRole('button', { name: /Update Status|Change Status/ }) ||
            page.getByRole('button', { name: 'Update Status' });
        this.descriptionBlock = page.getByRole('heading', { name: 'Description' }).locator('..');
        this.acceptanceCriteriaBlock = page
            .getByRole('heading', { name: 'Acceptance Criteria' })
            .locator('..');
        this.executionPlanBlock = page
            .getByRole('heading', { name: 'Execution Plan' })
            .locator('..');
        this.agentTasksSection = page.getByRole('heading', { name: /Agent Tasks/ });
    }

    async navigate(deliverableId?: string): Promise<void> {
        if (deliverableId) {
            await super.navigate(`/deliverables/${deliverableId}`);
        } else {
            await super.navigate('/deliverables');
        }
    }

    async waitForDetailLoaded(): Promise<void> {
        await this.editButton.waitFor({ state: 'visible', timeout: 10000 });
    }

    async clickEdit(): Promise<void> {
        await this.editButton.click();
    }

    async clickBack(): Promise<void> {
        await this.backToListLink.click();
    }

    async getStatus(): Promise<string | null> {
        const badges = this.page.locator('[class*="badge"], [class*="Badge"]');
        for (let i = 0; i < (await badges.count()); i++) {
            const text = await badges.nth(i).textContent();
                if (
                    text &&
                    [
                        'DRAFT',
                        'DESIGN',
                        'PLAN',
                        'IMPLEMENT',
                        'MERGE',
                        'DEPLOY',
                        'TEST',
                        'DONE',
                        'NEEDS_REVIEW',
                        'FAILED',
                        'REJECTED',
                    ].includes(text)
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
                // Status might auto-update on select
                await this.page.waitForTimeout(1000);
            }
        } catch {
            // Try alternative approach using the status dropdown in sidebar
            const statusOption = this.page.getByRole('option', { name: newStatus }).first();
            if (await statusOption.isVisible()) {
                await statusOption.click();
            }
        }
    }

    async verifyFieldVisible(fieldName: string): Promise<boolean> {
        return await this.page
            .getByRole('heading', { name: fieldName, level: 3 })
            .isVisible()
            .catch(() => false);
    }
}
