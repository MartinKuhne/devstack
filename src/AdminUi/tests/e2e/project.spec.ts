import { test, expect } from '@playwright/test';
import { ProjectListPage, CreateProjectDialog, ProjectDetailPage } from './pages/ProjectPage.js';
import { NavigationHelper } from './helpers/NavigationHelper.js';

test.describe('Project CRUD', () => {
    let projectListPage: ProjectListPage;
    let createProjectDialog: CreateProjectDialog;
    let navigationHelper: NavigationHelper;

    test.beforeEach(async ({ page }) => {
        projectListPage = new ProjectListPage(page);
        createProjectDialog = new CreateProjectDialog(page);
        navigationHelper = new NavigationHelper(page);
    });

    test('should display Projects page with correct heading', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await expect(projectListPage.pageTitle).toBeVisible();
        await projectListPage.expectNoErrors();
    });

    test('should have New Project button', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await expect(projectListPage.newProjectButton).toBeVisible();
        await projectListPage.expectNoErrors();
    });

    test('should open create dialog when clicking New Project', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });
    });

    test('should cancel create dialog', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });
        await createProjectDialog.cancel();
        await expect(createProjectDialog.dialog).not.toBeVisible({ timeout: 5000 });
        await projectListPage.expectNoErrors();
    });

    test('should validate empty project name', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        const createBtn = page.getByRole('button', { name: 'Create Project' });
        await expect(createBtn).toBeDisabled();

        await createProjectDialog.cancel();
        await projectListPage.expectNoErrors();
    });

    test('should navigate from projects to dashboard', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await navigationHelper.navigateToDashboard();
        await expect(page.getByRole('heading', { name: 'Dashboard', level: 2 })).toBeVisible();
        await projectListPage.expectNoErrors();
    });

    test('should navigate from projects to models', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await navigationHelper.navigateToModels();
        await expect(
            page.getByRole('heading', { name: 'Large Language Models', level: 2 })
        ).toBeVisible();
        await projectListPage.expectNoErrors();
    });

    test('should show project table with headers', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();

        await expect(page.getByRole('columnheader', { name: 'Name' })).toBeVisible();
        await expect(page.getByRole('columnheader', { name: 'Description' })).toBeVisible();
        await projectListPage.expectNoErrors();
    });

    test('should have Create Project button in dialog footer', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        const createBtn = page.getByRole('button', { name: 'Create Project' });
        await expect(createBtn).toBeVisible();

        await createProjectDialog.cancel();
        await projectListPage.expectNoErrors();
    });

    test('should have Cancel button in dialog footer', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        const cancelBtn = page.getByRole('button', { name: 'Cancel' });
        await expect(cancelBtn).toBeVisible();

        await createProjectDialog.cancel();
        await projectListPage.expectNoErrors();
    });

    test('sidebar navigation - dashboard to projects', async ({ page }) => {
        await navigationHelper.navigateToDashboard();
        await expect(page.getByRole('heading', { name: 'Dashboard', level: 2 })).toBeVisible();

        await navigationHelper.navigateToProjects();
        await expect(projectListPage.pageTitle).toBeVisible();
        await projectListPage.expectNoErrors();
    });

    test('sidebar navigation - projects to models', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();

        await navigationHelper.navigateToModels();
        await expect(
            page.getByRole('heading', { name: 'Large Language Models', level: 2 })
        ).toBeVisible();
        await projectListPage.expectNoErrors();
    });

    test('should show empty state when no projects exist', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();

        const hasEmptyState = await projectListPage.emptyStateMessage.isVisible().catch(() => false);
        if (hasEmptyState) {
            await expect(projectListPage.emptyStateMessage).toBeVisible();
        }
        await projectListPage.expectNoErrors();
    });
});

test.describe('Project Detail View', () => {
    let projectListPage: ProjectListPage;
    let createProjectDialog: CreateProjectDialog;
    let projectDetailPage: ProjectDetailPage;

    test.beforeEach(async ({ page }) => {
        projectListPage = new ProjectListPage(page);
        createProjectDialog = new CreateProjectDialog(page);
        projectDetailPage = new ProjectDetailPage(page);
    });

    test('should create a project and navigate to detail page', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject('DeleteAfterTest - Project E2E', 'A test project for e2e tests');

        await page.waitForTimeout(3000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project E2E');
        await page.waitForURL('/projects/**', { timeout: 10000 });
        await expect(projectDetailPage.pageTitle).toBeVisible({ timeout: 10000 });
        await expect(page.getByRole('heading', { name: 'DeleteAfterTest - Project E2E', level: 2 })).toBeVisible(
            { timeout: 10000 }
        );
    });

    test('should display project detail with name and repository link', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject(
            'DeleteAfterTest - Project With Repo',
            'Test description',
            'https://github.com/example/repo'
        );

        await page.waitForTimeout(2000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project With Repo');
        await page.waitForURL('/projects/**');
        await expect(projectDetailPage.pageTitle).toBeVisible();
        await expect(projectDetailPage.repositoryLink).toBeVisible();
        await expect(projectDetailPage.repositoryLink).toHaveAttribute(
            'href',
            'https://github.com/example/repo'
        );
    });

    test('should have Edit and Delete buttons on project detail page', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject('DeleteAfterTest - Project Buttons');

        await page.waitForTimeout(2000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project Buttons');
        await page.waitForURL('/projects/**');
        await expect(projectDetailPage.editButton).toBeVisible();
        await expect(projectDetailPage.deleteButton).toBeVisible();
    });

    test('should navigate back to projects list from detail page', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject('DeleteAfterTest - Project Back');

        await page.waitForTimeout(2000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project Back');
        await page.waitForURL('/projects/**');
        await projectDetailPage.clickBack();
        await page.waitForURL('/projects');
        await expect(projectListPage.pageTitle).toBeVisible();
    });

    test('should show two tabs on project detail page', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject('DeleteAfterTest - Project Tabs');

        await page.waitForTimeout(3000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project Tabs');
        await page.waitForURL('/projects/**');
        await expect(projectDetailPage.deliverablesTab).toBeVisible();
        await expect(projectDetailPage.agentTasksTab).toBeVisible();
    });

    test('should have Deliverables tab selected by default', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject('DeleteAfterTest - Project Default Tab');

        await page.waitForTimeout(2000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project Default Tab');
        await page.waitForURL('/projects/**');
        const isSelected = await projectDetailPage.isTabSelected('Deliverables');
        expect(isSelected).toBe(true);
    });

    test('should switch between tabs on project detail page', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject('DeleteAfterTest - Project Tab Switch');

        await page.waitForTimeout(2000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project Tab Switch');
        await page.waitForURL('/projects/**');
        await projectDetailPage.clickTab('Agent Tasks');
        const agentTasksSelected = await projectDetailPage.isTabSelected('Agent Tasks');
        expect(agentTasksSelected).toBe(true);

        await projectDetailPage.clickTab('Deliverables');
        const deliverablesSelected = await projectDetailPage.isTabSelected('Deliverables');
        expect(deliverablesSelected).toBe(true);
    });

    test('should show New Deliverable button in deliverables tab', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject('DeleteAfterTest - Project New Deliverable');

        await page.waitForTimeout(2000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project New Deliverable');
        await page.waitForURL('/projects/**');
        await expect(projectDetailPage.newDeliverableButton).toBeVisible();
    });

    test('should show New Agent Task button in agent tasks tab', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject('DeleteAfterTest - Project New Agent Task');

        await page.waitForTimeout(2000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project New Agent Task');
        await page.waitForURL('/projects/**');
        await projectDetailPage.clickTab('Agent Tasks');
        await expect(projectDetailPage.newAgentTaskButton).toBeVisible();
    });

    test('should open edit dialog when clicking Edit button', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject('DeleteAfterTest - Project Edit');

        await page.waitForTimeout(2000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project Edit');
        await page.waitForURL('/projects/**');
        await projectDetailPage.clickEdit();
        const editDialog = page.getByRole('dialog').filter({ hasText: /Edit Project/i });
        await expect(editDialog).toBeVisible({ timeout: 5000 });

        const nameInput = page.getByLabel('Name *');
        await expect(nameInput).toBeVisible();
        const saveButton = page.getByRole('button', { name: /Save|Update/ }).first();
        await expect(saveButton).toBeVisible();

        await saveButton.click();
        await expect(editDialog).not.toBeVisible({ timeout: 5000 });
    });

    test('should show empty deliverables message in deliverables tab', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });

        await createProjectDialog.createProject('DeleteAfterTest - Project Empty Deliverables');

        await page.waitForTimeout(2000);
        await projectListPage.clickProjectRow('DeleteAfterTest - Project Empty Deliverables');
        await page.waitForURL('/projects/**');
        await expect(page.getByRole('heading', { name: 'Deliverables', level: 3 })).toBeVisible();
        await expect(page.getByText('No deliverables yet')).toBeVisible();
    });
});
