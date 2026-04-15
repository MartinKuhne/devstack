import { test, expect } from '@playwright/test';
import { DefectListPage, CreateDefectDialog, DefectDetailPage, EditDefectDialog } from './pages/DefectPage.js';

test.describe('Defect Page', () => {
    let defectListPage: DefectListPage;
    let createDefectDialog: CreateDefectDialog;
    let defectDetailPage: DefectDetailPage;
    let editDefectDialog: EditDefectDialog;
    let projectId: string;

    test.beforeAll(async () => {
        const uniqueId = Date.now().toString();
        projectId = `project-${uniqueId}`;
    });

    test.beforeEach(async ({ page }) => {
        defectListPage = new DefectListPage(page);
        createDefectDialog = new CreateDefectDialog(page);
        defectDetailPage = new DefectDetailPage(page);
        editDefectDialog = new EditDefectDialog(page);
    });

    test('navigates to defect list page', async ({ page }) => {
        await defectListPage.navigate();
        await expect(defectListPage.pageTitle).toBeVisible();
        await expect(defectListPage.newDefectButton).toBeVisible();
    });

    test('shows empty state when no defects exist', async ({ page }) => {
        await defectListPage.navigate();
        await defectListPage.waitForEmptyState();
        await expect(defectListPage.emptyStateMessage).toBeVisible();
    });

    test('validates empty title on create defect', async ({ page }) => {
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await expect(createDefectDialog.dialog).toBeVisible();
        
        await createDefectDialog.submitEmptyForm();
        await createDefectDialog.waitForTitleError();
        await expect(createDefectDialog.titleError).toBeVisible();
    });

    test('creates defect with all fields', async ({ page }) => {
        const defectTitle = `Test Defect ${Date.now()}`;
        const description = 'Test description';
        const acceptanceCriteria = 'Test criteria';
        const plan = 'Test plan';
        const securityImpact = 'Test security impact';
        const performanceImpact = 'Test performance impact';
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await expect(createDefectDialog.dialog).toBeVisible();
        
        await createDefectDialog.createDefect(defectTitle, 'High', description, acceptanceCriteria, plan, securityImpact, performanceImpact);
        
        await expect(createDefectDialog.dialog).not.toBeVisible();
        await defectListPage.waitForDefectList();
        await expect(page.getByText(defectTitle)).toBeVisible();
    });

    test('creates defect with minimal fields', async ({ page }) => {
        const defectTitle = `Minimal Defect ${Date.now()}`;
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await expect(createDefectDialog.dialog).toBeVisible();
        
        await createDefectDialog.createDefect(defectTitle, 'Medium');
        
        await expect(createDefectDialog.dialog).not.toBeVisible();
        await defectListPage.waitForDefectList();
        await expect(page.getByText(defectTitle)).toBeVisible();
    });

    test('creates defect with parent feature', async ({ page }) => {
        const defectTitle = `Linked Defect ${Date.now()}`;
        const featureTitle = `Test Feature ${Date.now()}`;
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await expect(createDefectDialog.dialog).toBeVisible();
        
        await createDefectDialog.createDefect(defectTitle, 'Medium', 'Description', undefined, undefined, undefined, undefined);
        
        await expect(createDefectDialog.dialog).not.toBeVisible();
        await defectListPage.waitForDefectList();
        await expect(page.getByText(defectTitle)).toBeVisible();
    });

    test('cancels create defect dialog', async ({ page }) => {
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await expect(createDefectDialog.dialog).toBeVisible();
        
        await createDefectDialog.cancel();
        await expect(createDefectDialog.dialog).not.toBeVisible();
    });

    test('navigates to defect detail page', async ({ page }) => {
        const defectTitle = `Detail Test Defect ${Date.now()}`;
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await createDefectDialog.createDefect(defectTitle, 'Medium');
        
        await defectListPage.waitForDefectList();
        await defectListPage.clickDefectRow(defectTitle);
        
        await defectDetailPage.waitForDefectDetail(defectTitle);
        await expect(defectDetailPage.pageTitle).toBeVisible();
    });

    test('displays severity badge on defect detail', async ({ page }) => {
        const defectTitle = `Severity Test ${Date.now()}`;
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await createDefectDialog.createDefect(defectTitle, 'Critical');
        
        await defectListPage.waitForDefectList();
        await defectListPage.clickDefectRow(defectTitle);
        
        await defectDetailPage.waitForDefectDetail(defectTitle);
        await defectDetailPage.verifySeverity('Critical');
    });

    test('displays status badge on defect detail', async ({ page }) => {
        const defectTitle = `Status Test ${Date.now()}`;
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await createDefectDialog.createDefect(defectTitle, 'Medium');
        
        await defectListPage.waitForDefectList();
        await defectListPage.clickDefectRow(defectTitle);
        
        await defectDetailPage.waitForDefectDetail(defectTitle);
        await defectDetailPage.verifyStatus('Reported');
    });

    test('cancels edit defect dialog', async ({ page }) => {
        const defectTitle = `Edit Cancel Test ${Date.now()}`;
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await createDefectDialog.createDefect(defectTitle, 'Medium');
        
        await defectListPage.waitForDefectList();
        await defectListPage.clickDefectRow(defectTitle);
        await defectDetailPage.waitForDefectDetail(defectTitle);
        
        await defectDetailPage.clickEdit();
        await expect(editDefectDialog.dialog).toBeVisible();
        
        await editDefectDialog.cancel();
        await expect(editDefectDialog.dialog).not.toBeVisible();
    });

    test('edits defect via edit dialog', async ({ page }) => {
        const defectTitle = `Edit Test Defect ${Date.now()}`;
        const updatedTitle = `Updated Defect ${Date.now()}`;
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await createDefectDialog.createDefect(defectTitle, 'Medium');
        
        await defectListPage.waitForDefectList();
        await defectListPage.clickDefectRow(defectTitle);
        await defectDetailPage.waitForDefectDetail(defectTitle);
        
        await defectDetailPage.clickEdit();
        await expect(editDefectDialog.dialog).toBeVisible();
        
        await editDefectDialog.updateDefect(updatedTitle, 'High');
        await expect(editDefectDialog.dialog).not.toBeVisible();
        
        await defectDetailPage.waitForDefectDetail(updatedTitle);
        await expect(page.getByText(updatedTitle)).toBeVisible();
    });

    test('filters defects by status', async ({ page }) => {
        const defectTitle1 = `Filter Test 1 ${Date.now()}`;
        const defectTitle2 = `Filter Test 2 ${Date.now()}`;
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await createDefectDialog.createDefect(defectTitle1, 'Medium');
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await createDefectDialog.createDefect(defectTitle2, 'High');
        
        await defectListPage.navigate();
        await defectListPage.waitForDefectList();
        
        await defectListPage.selectStatusFilter('Reported');
        await expect(page.getByText(defectTitle1)).toBeVisible();
    });

    test('shows defect description on detail page', async ({ page }) => {
        const defectTitle = `Description Test ${Date.now()}`;
        const description = 'This is a detailed description of the defect';
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await createDefectDialog.createDefect(defectTitle, 'Medium', description);
        
        await defectListPage.waitForDefectList();
        await defectListPage.clickDefectRow(defectTitle);
        
        await defectDetailPage.waitForDefectDetail(defectTitle);
        await expect(defectDetailPage.descriptionSection).toBeVisible();
    });

    test('shows empty state after all defects deleted', async ({ page }) => {
        const defectTitle = `Delete Test ${Date.now()}`;
        
        await defectListPage.navigate();
        await defectListPage.clickNewDefect();
        await createDefectDialog.createDefect(defectTitle, 'Medium');
        
        await defectListPage.waitForDefectList();
        await defectListPage.clickDefectRow(defectTitle);
        await defectDetailPage.waitForDefectDetail(defectTitle);
        
        await defectDetailPage.clickEdit();
        await editDefectDialog.updateDefect(defectTitle, 'Medium');
        
        await defectDetailPage.clickBack();
        await defectListPage.waitForDefectList();
        
        await defectListPage.clickDefectRow(defectTitle);
        await defectDetailPage.waitForDefectDetail(defectTitle);
        
        await defectDetailPage.clickBack();
        await defectListPage.waitForEmptyState();
        await expect(defectListPage.emptyStateMessage).toBeVisible();
    });
});
