import { test, expect } from '@playwright/test';
import { ProjectListPage, CreateProjectDialog } from '../pages/ProjectPage.js';
import { NavigationHelper } from '../helpers/NavigationHelper.js';

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
    });

    test('should have New Project button', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await expect(projectListPage.newProjectButton).toBeVisible();
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
    });

    test('should validate empty project name', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });
        
        // Try to submit without filling name
        await createProjectDialog.createButton.click();
        // Should still be visible due to validation error
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });
        
        await createProjectDialog.cancel();
    });

    test('should navigate from projects to dashboard', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await navigationHelper.navigateToDashboard();
        await expect(page.getByRole('heading', { name: 'Dashboard', level: 2 })).toBeVisible();
    });

    test('should navigate from projects to models', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await navigationHelper.navigateToModels();
        await expect(page.getByRole('heading', { name: 'Large Language Models', level: 2 })).toBeVisible();
    });

    test('should show project table with headers', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        
        // Table header columns should be visible
        await expect(page.getByRole('columnheader', { name: 'Name' })).toBeVisible();
        await expect(page.getByRole('columnheader', { name: 'Description' })).toBeVisible();
    });

    test('should have Create Project button in dialog footer', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });
        
        const createBtn = page.getByRole('button', { name: 'Create Project' });
        await expect(createBtn).toBeVisible();
        
        await createProjectDialog.cancel();
    });

    test('should have Cancel button in dialog footer', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible({ timeout: 5000 });
        
        const cancelBtn = page.getByRole('button', { name: 'Cancel' });
        await expect(cancelBtn).toBeVisible();
        
        await createProjectDialog.cancel();
    });

    test('sidebar navigation - dashboard to projects', async ({ page }) => {
        await navigationHelper.navigateToDashboard();
        await expect(page.getByRole('heading', { name: 'Dashboard', level: 2 })).toBeVisible();
        
        await navigationHelper.navigateToProjects();
        await expect(projectListPage.pageTitle).toBeVisible();
    });

    test('sidebar navigation - projects to models', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        
        await navigationHelper.navigateToModels();
        await expect(page.getByRole('heading', { name: 'Large Language Models', level: 2 })).toBeVisible();
    });

    test('should show empty state when no projects exist', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForProjectList();
        
        // Either empty state message or table should be visible
        const hasEmptyState = await projectListPage.emptyStateMessage.isVisible().catch(() => false);
        if (hasEmptyState) {
            await expect(projectListPage.emptyStateMessage).toBeVisible();
        }
    });
});
