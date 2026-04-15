import type { Page, Locator } from '@playwright/test';
import { expect } from '@playwright/test';
import { BasePage } from '../fixtures/BasePage.js';

export class DefectListPage extends BasePage {
    readonly pageTitle: Locator;
    readonly newDefectButton: Locator;
    readonly defectTable: Locator;
    readonly emptyStateMessage: Locator;
    readonly statusFilter: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: 'Defects' });
        this.newDefectButton = page.getByRole('button', { name: 'New Defect' });
        this.defectTable = page.getByRole('table');
        this.emptyStateMessage = page.getByText('No defects found');
        this.statusFilter = page.getByRole('combobox', { name: 'Filter by status' });
    }

    async navigate(): Promise<void> {
        await super.navigate('/defects');
        await this.page.waitForLoadState('networkidle');
    }

    async clickNewDefect(): Promise<void> {
        await this.newDefectButton.click();
    }

    async waitForDefectList(): Promise<void> {
        await this.defectTable.waitFor({ state: 'visible', timeout: 10000 });
    }

    async waitForEmptyState(): Promise<void> {
        await this.emptyStateMessage.waitFor({ state: 'visible' });
    }

    async clickDefectRow(defectTitle: string): Promise<void> {
        const row = this.page.getByRole('row').filter({ hasText: defectTitle });
        await row.click();
    }

    async waitForDefectDetail(defectTitle: string): Promise<void> {
        await this.page.getByText(defectTitle).waitFor({ state: 'visible', timeout: 10000 });
    }

    async selectStatusFilter(status: string): Promise<void> {
        await this.statusFilter.click();
        await this.page.getByRole('option', { name: status }).click();
    }

    async getDefectCount(): Promise<number> {
        const rows = await this.page.getByRole('row').all();
        return rows.length - 1; // Subtract header row
    }
}

export class CreateDefectDialog extends BasePage {
    readonly dialog: Locator;
    readonly titleInput: Locator;
    readonly severitySelect: Locator;
    readonly parentFeatureSelect: Locator;
    readonly descriptionInput: Locator;
    readonly acceptanceCriteriaInput: Locator;
    readonly planInput: Locator;
    readonly securityImpactInput: Locator;
    readonly performanceImpactInput: Locator;
    readonly createButton: Locator;
    readonly cancelButton: Locator;
    readonly titleError: Locator;
    readonly severityError: Locator;
    readonly projectError: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog', { name: 'Create New Defect' });
        this.titleInput = page.getByLabel('Title *');
        this.severitySelect = page.getByLabel('Severity *');
        this.parentFeatureSelect = page.getByRole('button', { name: 'Select a feature' });
        this.descriptionInput = page.getByLabel('Description');
        this.acceptanceCriteriaInput = page.getByLabel('Acceptance Criteria');
        this.planInput = page.getByLabel('Plan');
        this.securityImpactInput = page.getByLabel('Security Impact');
        this.performanceImpactInput = page.getByLabel('Performance Impact');
        this.createButton = page.getByRole('button', { name: 'Create Defect' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
        this.titleError = page.getByText('Title is required');
        this.severityError = page.getByText('Severity is required');
        this.projectError = page.getByText('Project is required');
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async fillForm(title: string, severity: string, description?: string, acceptanceCriteria?: string, plan?: string, securityImpact?: string, performanceImpact?: string, parentFeatureId?: string): Promise<void> {
        await this.titleInput.fill(title);
        await this.severitySelect.click();
        await this.page.getByRole('option', { name: severity }).click();
        
        if (description) await this.descriptionInput.fill(description);
        if (acceptanceCriteria) await this.acceptanceCriteriaInput.fill(acceptanceCriteria);
        if (plan) await this.planInput.fill(plan);
        if (securityImpact) await this.securityImpactInput.fill(securityImpact);
        if (performanceImpact) await this.performanceImpactInput.fill(performanceImpact);
        
        if (parentFeatureId) {
            await this.parentFeatureSelect.click();
            await this.page.getByRole('option', { name: parentFeatureId }).click();
        }
    }

    async createDefect(title: string, severity: string, description?: string, acceptanceCriteria?: string, plan?: string, securityImpact?: string, performanceImpact?: string): Promise<void> {
        await this.fillForm(title, severity, description, acceptanceCriteria, plan, securityImpact, performanceImpact);
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

    async waitForSeverityError(): Promise<void> {
        await this.severityError.waitFor({ state: 'visible' });
    }

    async selectParentFeature(featureTitle: string): Promise<void> {
        await this.parentFeatureSelect.click();
        await this.page.getByRole('option', { name: featureTitle }).click();
    }
}

export class DefectDetailPage extends BasePage {
    readonly pageTitle: Locator;
    readonly severityBadge: Locator;
    readonly statusBadge: Locator;
    readonly editButton: Locator;
    readonly backButton: Locator;
    readonly closeButton: Locator;
    readonly descriptionSection: Locator;
    readonly acceptanceCriteriaSection: Locator;
    readonly planSection: Locator;
    readonly parentFeatureLink: Locator;

    constructor(page: Page) {
        super(page);
        this.pageTitle = page.getByRole('heading', { name: /Defect/i }).first();
        this.severityBadge = page.getByText(/Critical|High|Medium|Low/i);
        this.statusBadge = page.getByText(/Reported|Triaged|InProgress|Resolved|Closed/i);
        this.editButton = page.getByRole('button', { name: 'Edit' });
        this.backButton = page.getByRole('button', { name: 'Back' });
        this.closeButton = page.getByRole('button', { name: 'Close' });
        this.descriptionSection = page.getByText('Description').locator('..');
        this.acceptanceCriteriaSection = page.getByText('Acceptance Criteria').locator('..');
        this.planSection = page.getByText('Plan').locator('..');
        this.parentFeatureLink = page.getByText(/Parent Feature/i).locator('..').getByRole('link').first();
    }

    async isOpen(): Promise<boolean> {
        return await this.pageTitle.isVisible();
    }

    async waitForDefectDetail(defectTitle: string): Promise<void> {
        await this.page.getByText(defectTitle).waitFor({ state: 'visible', timeout: 10000 });
    }

    async clickEdit(): Promise<void> {
        await this.editButton.click();
    }

    async clickBack(): Promise<void> {
        await this.backButton.click();
    }

    async verifySeverity(severity: string): Promise<void> {
        await expect(this.severityBadge).toContainText(severity);
    }

    async verifyStatus(status: string): Promise<void> {
        await expect(this.statusBadge).toContainText(status);
    }
}

export class EditDefectDialog extends BasePage {
    readonly dialog: Locator;
    readonly titleInput: Locator;
    readonly severitySelect: Locator;
    readonly descriptionInput: Locator;
    readonly acceptanceCriteriaInput: Locator;
    readonly planInput: Locator;
    readonly securityImpactInput: Locator;
    readonly performanceImpactInput: Locator;
    readonly saveButton: Locator;
    readonly cancelButton: Locator;

    constructor(page: Page) {
        super(page);
        this.dialog = page.getByRole('dialog', { name: /Edit Defect/i });
        this.titleInput = page.getByLabel('Title *');
        this.severitySelect = page.getByLabel('Severity *');
        this.descriptionInput = page.getByLabel('Description');
        this.acceptanceCriteriaInput = page.getByLabel('Acceptance Criteria');
        this.planInput = page.getByLabel('Plan');
        this.securityImpactInput = page.getByLabel('Security Impact');
        this.performanceImpactInput = page.getByLabel('Performance Impact');
        this.saveButton = page.getByRole('button', { name: 'Save Changes' });
        this.cancelButton = page.getByRole('button', { name: 'Cancel' });
    }

    async isOpen(): Promise<boolean> {
        return await this.dialog.isVisible();
    }

    async fillForm(title?: string, severity?: string, description?: string, acceptanceCriteria?: string, plan?: string, securityImpact?: string, performanceImpact?: string): Promise<void> {
        if (title) await this.titleInput.fill(title);
        if (severity) {
            await this.severitySelect.click();
            await this.page.getByRole('option', { name: severity }).click();
        }
        if (description) await this.descriptionInput.fill(description);
        if (acceptanceCriteria) await this.acceptanceCriteriaInput.fill(acceptanceCriteria);
        if (plan) await this.planInput.fill(plan);
        if (securityImpact) await this.securityImpactInput.fill(securityImpact);
        if (performanceImpact) await this.performanceImpactInput.fill(performanceImpact);
    }

    async updateDefect(title?: string, severity?: string, description?: string, acceptanceCriteria?: string, plan?: string, securityImpact?: string, performanceImpact?: string): Promise<void> {
        await this.fillForm(title, severity, description, acceptanceCriteria, plan, securityImpact, performanceImpact);
        await this.saveButton.click();
    }

    async cancel(): Promise<void> {
        await this.cancelButton.click();
    }
}
