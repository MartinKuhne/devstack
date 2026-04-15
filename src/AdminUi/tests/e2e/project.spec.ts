import { test, expect } from '@playwright/test';
import { ProjectListPage, CreateProjectDialog, ProjectDetailPage, EditProjectDialog } from '../pages/ProjectPage';

test.describe('Project Page', () => {
    let projectListPage: ProjectListPage;
    let createProjectDialog: CreateProjectDialog;
    let projectDetailPage: ProjectDetailPage;
    let editProjectDialog: EditProjectDialog;

    test.beforeEach(async ({ page }) => {
        projectListPage = new ProjectListPage(page);
        createProjectDialog = new CreateProjectDialog(page);
        projectDetailPage = new ProjectDetailPage(page);
        editProjectDialog = new EditProjectDialog(page);
    });

    test('navigates to project list page', async () => {
        await projectListPage.navigate();
        await expect(projectListPage.pageTitle).toBeVisible();
        await expect(projectListPage.newProjectButton).toBeVisible();
    });

    test('shows empty state when no projects exist', async () => {
        await projectListPage.navigate();
        await projectListPage.waitForEmptyState();
        await expect(projectListPage.emptyStateMessage).toBeVisible();
    });

    test('validates empty name on create project', async () => {
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible();
        
        await createProjectDialog.submitEmptyForm();
        await createProjectDialog.waitForNameError();
        await expect(createProjectDialog.nameError).toBeVisible();
    });

    test('validates invalid URL on create project', async () => {
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible();
        
        await createProjectDialog.submitInvalidUrl();
        await createProjectDialog.waitForUrlError();
        await expect(createProjectDialog.urlError).toBeVisible();
    });

    test('creates project with all fields', async ({ page }) => {
        const projectName = `Test Project ${Date.now()}`;
        const description = 'Test description';
        const githubUrl = 'https://github.com/test/repo';
        
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible();
        
        await createProjectDialog.createProject(projectName, description, undefined, undefined, githubUrl);
        
        await expect(createProjectDialog.dialog).not.toBeVisible();
        await projectListPage.waitForProjectList();
        await expect(page.getByText(projectName)).toBeVisible();
    });

    test('creates project with minimal fields', async ({ page }) => {
        const projectName = `Minimal Project ${Date.now()}`;
        
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible();
        
        await createProjectDialog.createProject(projectName);
        
        await expect(createProjectDialog.dialog).not.toBeVisible();
        await projectListPage.waitForProjectList();
        await expect(page.getByText(projectName)).toBeVisible();
    });

    test('navigates to project detail page', async ({ page }) => {
        const projectName = `Detail Test Project ${Date.now()}`;
        
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.createProject(projectName);
        
        await projectListPage.waitForProjectList();
        await projectListPage.clickProjectRow(projectName);
        
        await projectDetailPage.waitForProjectDetail();
        await expect(projectDetailPage.pageTitle).toBeVisible();
    });

    test('cancels create project dialog', async () => {
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await expect(createProjectDialog.dialog).toBeVisible();
        
        await createProjectDialog.cancel();
        await expect(createProjectDialog.dialog).not.toBeVisible();
    });

    test('cancels edit project dialog', async ({ page }) => {
        const projectName = `Edit Test Project ${Date.now()}`;
        
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.createProject(projectName);
        
        await projectListPage.waitForProjectList();
        await projectListPage.clickProjectRow(projectName);
        await projectDetailPage.waitForProjectDetail();
        
        await projectDetailPage.clickEdit();
        await expect(editProjectDialog.dialog).toBeVisible();
        
        await editProjectDialog.cancel();
        await expect(editProjectDialog.dialog).not.toBeVisible();
    });
});
