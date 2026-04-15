import { test, expect } from '@playwright/test';
import { TaskBoardPage, CreateTaskDialog, TaskDetailDrawer, EditTaskDialog } from './pages/TaskPage.js';

test.describe('Task Page', () => {
    let taskBoardPage: TaskBoardPage;
    let createTaskDialog: CreateTaskDialog;
    let taskDetailDrawer: TaskDetailDrawer;
    let editTaskDialog: EditTaskDialog;
    let featureId: string;

    test.beforeAll(async () => {
        const uniqueId = Date.now().toString();
        featureId = `feature-${uniqueId}`;
    });

    test.beforeEach(async ({ page }) => {
        taskBoardPage = new TaskBoardPage(page);
        createTaskDialog = new CreateTaskDialog(page);
        taskDetailDrawer = new TaskDetailDrawer(page);
        editTaskDialog = new EditTaskDialog(page);
    });

    test('navigates to task board page', async ({ page }) => {
        await taskBoardPage.navigate(featureId);
        await expect(taskBoardPage.pageTitle).toBeVisible();
        await expect(taskBoardPage.newTaskButton).toBeVisible();
    });

    test('shows empty state when no tasks exist', async ({ page }) => {
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.waitForEmptyState();
        await expect(taskBoardPage.emptyStateMessage).toBeVisible();
    });

    test('validates empty title on create task', async ({ page }) => {
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await expect(createTaskDialog.dialog).toBeVisible();
        
        await createTaskDialog.submitEmptyForm();
        await createTaskDialog.waitForTitleError();
        await expect(createTaskDialog.titleError).toBeVisible();
    });

    test('validates invalid complexity on create task', async ({ page }) => {
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await expect(createTaskDialog.dialog).toBeVisible();
        
        await createTaskDialog.fillForm('Test Task', undefined, undefined, undefined, undefined, '');
        await createTaskDialog.submitEmptyForm();
        await createTaskDialog.waitForComplexityError();
        await expect(createTaskDialog.complexityError).toBeVisible();
    });

    test('creates task with all fields', async ({ page }) => {
        const taskTitle = `Test Task ${Date.now()}`;
        const deliverable = 'Test deliverable';
        const acceptanceCriteria = 'Test criteria';
        const risks = 'Test risks';
        const followUps = 'Test follow-ups';
        
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await expect(createTaskDialog.dialog).toBeVisible();
        
        await createTaskDialog.createTask(taskTitle, deliverable, acceptanceCriteria, risks, followUps, 'Moderate');
        
        await expect(createTaskDialog.dialog).not.toBeVisible();
        await taskBoardPage.waitForTaskCard(taskTitle);
        await expect(page.getByText(taskTitle)).toBeVisible();
    });

    test('creates task with minimal fields', async ({ page }) => {
        const taskTitle = `Minimal Task ${Date.now()}`;
        
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await expect(createTaskDialog.dialog).toBeVisible();
        
        await createTaskDialog.createTask(taskTitle, undefined, undefined, undefined, undefined, 'Simple');
        
        await expect(createTaskDialog.dialog).not.toBeVisible();
        await taskBoardPage.waitForTaskCard(taskTitle);
        await expect(page.getByText(taskTitle)).toBeVisible();
    });

    test('cancels create task dialog', async ({ page }) => {
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await expect(createTaskDialog.dialog).toBeVisible();
        
        await createTaskDialog.cancel();
        await expect(createTaskDialog.dialog).not.toBeVisible();
    });

    test('opens task detail drawer', async ({ page }) => {
        const taskTitle = `Detail Test Task ${Date.now()}`;
        
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await createTaskDialog.createTask(taskTitle, undefined, undefined, undefined, undefined, 'Simple');
        
        await taskBoardPage.waitForTaskCard(taskTitle);
        await taskBoardPage.clickTaskCard(taskTitle);
        
        await expect(taskDetailDrawer.drawer).toBeVisible();
        await expect(taskDetailDrawer.taskTitle).toBeVisible();
    });

    test('closes task detail drawer', async ({ page }) => {
        const taskTitle = `Close Test Task ${Date.now()}`;
        
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await createTaskDialog.createTask(taskTitle, undefined, undefined, undefined, undefined, 'Simple');
        
        await taskBoardPage.waitForTaskCard(taskTitle);
        await taskBoardPage.clickTaskCard(taskTitle);
        await expect(taskDetailDrawer.drawer).toBeVisible();
        
        await taskDetailDrawer.close();
        await expect(taskDetailDrawer.drawer).not.toBeVisible();
    });

    test('edits task via edit dialog', async ({ page }) => {
        const taskTitle = `Edit Test Task ${Date.now()}`;
        const updatedTitle = `Updated Task ${Date.now()}`;
        
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await createTaskDialog.createTask(taskTitle, undefined, undefined, undefined, undefined, 'Simple');
        
        await taskBoardPage.waitForTaskCard(taskTitle);
        await taskBoardPage.clickTaskCard(taskTitle);
        await taskDetailDrawer.waitForTaskDetail(taskTitle);
        
        await taskDetailDrawer.clickEdit();
        await expect(editTaskDialog.dialog).toBeVisible();
        
        await editTaskDialog.updateTask(updatedTitle, 'Moderate', 'In Progress');
        await expect(editTaskDialog.dialog).not.toBeVisible();
        
        await taskBoardPage.waitForTaskCard(updatedTitle);
        await expect(page.getByText(updatedTitle)).toBeVisible();
    });

    test('cancels edit task dialog', async ({ page }) => {
        const taskTitle = `Edit Cancel Test ${Date.now()}`;
        
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await createTaskDialog.createTask(taskTitle, undefined, undefined, undefined, undefined, 'Simple');
        
        await taskBoardPage.waitForTaskCard(taskTitle);
        await taskBoardPage.clickTaskCard(taskTitle);
        await taskDetailDrawer.waitForTaskDetail(taskTitle);
        
        await taskDetailDrawer.clickEdit();
        await expect(editTaskDialog.dialog).toBeVisible();
        
        await editTaskDialog.cancel();
        await expect(editTaskDialog.dialog).not.toBeVisible();
    });

    test('performs status transition from task detail', async ({ page }) => {
        const taskTitle = `Transition Test ${Date.now()}`;
        
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await createTaskDialog.createTask(taskTitle, undefined, undefined, undefined, undefined, 'Simple');
        
        await taskBoardPage.waitForTaskCard(taskTitle);
        await taskBoardPage.clickTaskCard(taskTitle);
        await taskDetailDrawer.waitForTaskDetail(taskTitle);
        
        await taskDetailDrawer.selectStatusTransition('Done');
        await taskDetailDrawer.clickTransition();
        
        await taskDetailDrawer.waitForStatusChange('Done');
    });

    test('shows pagination in task list', async ({ page }) => {
        const taskCount = 15;
        const tasks: string[] = [];
        
        for (let i = 0; i < taskCount; i++) {
            const taskTitle = `Pagination Task ${i} ${Date.now()}`;
            tasks.push(taskTitle);
            
            await taskBoardPage.navigate(featureId);
            await taskBoardPage.clickNewTask();
            await createTaskDialog.createTask(taskTitle, undefined, undefined, undefined, undefined, 'Simple');
        }
        
        await taskBoardPage.navigate(featureId);
        await expect(page.getByRole('navigation')).toBeVisible();
    });

    test('shows empty state after all tasks deleted', async ({ page }) => {
        const taskTitle = `Delete Test Task ${Date.now()}`;
        
        await taskBoardPage.navigate(featureId);
        await taskBoardPage.clickNewTask();
        await createTaskDialog.createTask(taskTitle, undefined, undefined, undefined, undefined, 'Simple');
        
        await taskBoardPage.waitForTaskCard(taskTitle);
        await taskBoardPage.clickTaskCard(taskTitle);
        await taskDetailDrawer.waitForTaskDetail(taskTitle);
        
        await taskDetailDrawer.clickEdit();
        await editTaskDialog.updateTask(taskTitle, 'Simple', 'Done');
        
        await taskDetailDrawer.close();
        await taskBoardPage.waitForTaskCard(taskTitle);
        
        await taskBoardPage.clickTaskCard(taskTitle);
        await taskDetailDrawer.waitForTaskDetail(taskTitle);
        
        await taskDetailDrawer.clickEdit();
        await editTaskDialog.updateTask(taskTitle, 'Simple', 'Done');
        
        await taskDetailDrawer.close();
        await taskBoardPage.waitForEmptyState();
        await expect(taskBoardPage.emptyStateMessage).toBeVisible();
    });
});
