import { test, expect } from '@playwright/test';
import { ProjectListPage, CreateProjectDialog, ProjectDetailPage } from './pages/ProjectPage.js';
import { TaskBoardPage, CreateTaskDialog, TaskDetailDrawer } from './pages/TaskPage.js';
import { DefectListPage, CreateDefectDialog, DefectDetailPage, EditDefectDialog } from './pages/DefectPage.js';
import { NavigationHelper } from './helpers/NavigationHelper.js';

test.describe('End-to-End Workflows', () => {
    let projectListPage: ProjectListPage;
    let createProjectDialog: CreateProjectDialog;
    let projectDetailPage: ProjectDetailPage;
    let taskBoardPage: TaskBoardPage;
    let createTaskDialog: CreateTaskDialog;
    let taskDetailDrawer: TaskDetailDrawer;
    let defectListPage: DefectListPage;
    let createDefectDialog: CreateDefectDialog;
    let defectDetailPage: DefectDetailPage;
    let editDefectDialog: EditDefectDialog;
    let navigationHelper: NavigationHelper;

    test.beforeEach(async ({ page }) => {
        projectListPage = new ProjectListPage(page);
        createProjectDialog = new CreateProjectDialog(page);
        projectDetailPage = new ProjectDetailPage(page);
        taskBoardPage = new TaskBoardPage(page);
        createTaskDialog = new CreateTaskDialog(page);
        taskDetailDrawer = new TaskDetailDrawer(page);
        defectListPage = new DefectListPage(page);
        createDefectDialog = new CreateDefectDialog(page);
        defectDetailPage = new DefectDetailPage(page);
        editDefectDialog = new EditDefectDialog(page);
        navigationHelper = new NavigationHelper(page);
    });

    test.describe('Complete Feature Lifecycle', () => {
        test('creates task, transitions status, marks done', async ({ page }) => {
            const taskTitle = `E2E Task ${Date.now()}`;
            const featureId = 'feature-test';
            
            await taskBoardPage.navigate(featureId);
            await taskBoardPage.clickNewTask();
            
            await expect(createTaskDialog.dialog).toBeVisible();
            await createTaskDialog.createTask(taskTitle, undefined, undefined, undefined, undefined, 'Simple');
            
            await expect(createTaskDialog.dialog).not.toBeVisible();
            await taskBoardPage.waitForTaskCard(taskTitle);
            
            await taskBoardPage.clickTaskCard(taskTitle);
            await taskDetailDrawer.waitForTaskDetail(taskTitle);
            
            await taskDetailDrawer.selectStatusTransition('In Progress');
            await taskDetailDrawer.clickTransition();
            
            await taskDetailDrawer.waitForStatusChange('In Progress');
            
            await taskDetailDrawer.selectStatusTransition('Done');
            await taskDetailDrawer.clickTransition();
            
            await taskDetailDrawer.waitForStatusChange('Done');
        });
    });

    test.describe('Complete Defect Lifecycle', () => {
        test('creates defect, transitions status to resolved', async ({ page }) => {
            const defectTitle = `E2E Defect ${Date.now()}`;
            const description = 'Test defect description';
            const acceptanceCriteria = 'Defect is fixed';
            const plan = 'Investigate root cause and fix';
            
            await defectListPage.navigate();
            await defectListPage.clickNewDefect();
            
            await createDefectDialog.createDefect(defectTitle, 'High', description, acceptanceCriteria, plan);
            
            await expect(createDefectDialog.dialog).not.toBeVisible();
            await defectListPage.waitForDefectList();
            await expect(page.getByText(defectTitle)).toBeVisible();
            
            await defectListPage.clickDefectRow(defectTitle);
            await defectDetailPage.waitForDefectDetail(defectTitle);
            
            await defectDetailPage.verifyStatus('Reported');
            
            await defectDetailPage.clickEdit();
            await expect(editDefectDialog.dialog).toBeVisible();
            await editDefectDialog.cancel();
            await expect(editDefectDialog.dialog).not.toBeVisible();
            
            await defectDetailPage.clickBack();
            await defectListPage.waitForDefectList();
            
            await defectListPage.selectStatusFilter('Resolved');
            await expect(page.getByText(defectTitle)).toBeVisible();
        });
    });

    test.describe('Cross-Entity Data Consistency', () => {
        test('project persists across navigation', async ({ page }) => {
            const projectName = `E2E Consistency ${Date.now()}`;
            
            await projectListPage.navigate();
            await projectListPage.clickNewProject();
            await createProjectDialog.createProject(projectName);
            
            await expect(createProjectDialog.dialog).not.toBeVisible();
            await projectListPage.waitForProjectList();
            
            await navigationHelper.navigateToDashboard();
            await page.waitForLoadState('networkidle');
            
            await navigationHelper.navigateToProjects();
            await projectListPage.waitForProjectList();
            
            await expect(page.getByText(projectName)).toBeVisible();
        });

        test('task shows in feature detail', async ({ page }) => {
            const taskTitle = `E2E Task Detail ${Date.now()}`;
            const featureId = 'feature-test';
            
            await taskBoardPage.navigate(featureId);
            await taskBoardPage.clickNewTask();
            await createTaskDialog.createTask(taskTitle, undefined, undefined, undefined, undefined, 'Simple');
            
            await expect(createTaskDialog.dialog).not.toBeVisible();
            await taskBoardPage.waitForTaskCard(taskTitle);
            
            await taskBoardPage.clickTaskCard(taskTitle);
            await taskDetailDrawer.waitForTaskDetail(taskTitle);
            await expect(taskDetailDrawer.drawer).toBeVisible();
        });
    });
});