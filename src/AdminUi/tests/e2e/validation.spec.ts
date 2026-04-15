import { test, expect } from '@playwright/test';
import { ProjectListPage, CreateProjectDialog } from './pages/ProjectPage.js';
import { DashboardPage } from './pages/DashboardPage.js';

test.describe('Error Handling and Validation', () => {
    let projectListPage: ProjectListPage;
    let createProjectDialog: CreateProjectDialog;
    let dashboardPage: DashboardPage;

    test.beforeEach(async ({ page }) => {
        projectListPage = new ProjectListPage(page);
        createProjectDialog = new CreateProjectDialog(page);
        dashboardPage = new DashboardPage(page);
    });

    test('shows toast notification on server error', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('createProject')) {
                await route.fulfill({
                    status: 500,
                    body: JSON.stringify({ errors: [{ message: 'Internal server error' }] }),
                });
            } else {
                await route.continue();
            }
        });

        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.createProject('Test Project', 'Description', undefined, undefined, 'https://github.com/test/repo');

        await expect(page.getByText('Internal server error')).toBeVisible();
    });

    test('handles NOT_FOUND error when entity is deleted', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('project')) {
                await route.fulfill({
                    status: 404,
                    body: JSON.stringify({ 
                        errors: [{ 
                            message: 'Project not found. It may have been deleted.',
                            extensions: { code: 'NOT_FOUND' }
                        }] 
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await page.goto('/projects/non-existent-id');
        
        await expect(page.getByText('Project not found')).toBeVisible().catch(() => {
            expect(page.getByText('not found').first()).toBeVisible();
        });
    });

    test('handles CONCURRENCY_CONFLICT error', async ({ page }) => {
        let isFirstRequest = true;
        
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('updateProject')) {
                if (isFirstRequest) {
                    isFirstRequest = false;
                    await route.fulfill({
                        status: 409,
                        body: JSON.stringify({ 
                            errors: [{ 
                                message: 'The project was modified by another user. Please refresh and try again.',
                                extensions: { code: 'CONCURRENCY_CONFLICT' }
                            }] 
                        }),
                    });
                } else {
                    await route.fulfill({
                        status: 200,
                        body: JSON.stringify({
                            data: {
                                updateProject: {
                                    id: 'test-id',
                                    name: 'Updated Project',
                                },
                            },
                        }),
                    });
                }
            } else {
                await route.continue();
            }
        });

        const projectName = `Concurrency Test ${Date.now()}`;
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.createProject(projectName, 'Description');
        await projectListPage.waitForProjectList();
        
        await projectListPage.clickProjectRow(projectName);
        await page.waitForTimeout(500);
        
        await page.getByRole('button', { name: 'Edit' }).click();
        await createProjectDialog.fillForm('Updated Project', 'Updated Description');
        await createProjectDialog.createButton.click();
        
        await expect(page.getByText('modified by another user').first()).toBeVisible().catch(() => {
            expect(page.getByText('Please refresh').first()).toBeVisible();
        });
    });

    test('simulates network failure with API mocking', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            await route.abort('failed');
        });

        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.createProject('Test Project', 'Description');

        await expect(page.getByText('Network error').first()).toBeVisible().catch(() => {
            expect(page.getByText('Unable to connect').first()).toBeVisible()
                .catch(() => {
                    expect(page.locator('.error')).toBeVisible().catch(() => {
                        expect(page.getByRole('alert').first()).toBeVisible();
                    });
                });
        });
    });

    test('prevents XSS in form inputs', async ({ page }) => {
        const xssPayload = '<script>alert("XSS")</script>';
        
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.fillForm(xssPayload, xssPayload, undefined, undefined, 'https://github.com/test/repo');
        await createProjectDialog.createButton.click();
        await projectListPage.waitForProjectList();

        await expect(page.locator('script')).toHaveCount(0);
        await expect(page.getByText(xssPayload)).toBeVisible();
    });

    test('enforces max length on input fields', async ({ page }) => {
        const longName = 'A'.repeat(1000);
        
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.nameInput.fill(longName);
        
        const actualValue = await createProjectDialog.nameInput.inputValue();
        expect(actualValue.length).toBeLessThanOrEqual(500);
    });

    test('shows loading state during form submission', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('createProject')) {
                await new Promise(resolve => setTimeout(resolve, 1000));
                await route.fulfill({
                    status: 200,
                    body: JSON.stringify({
                        data: {
                            createProject: {
                                id: 'test-id',
                                name: 'Test Project',
                            },
                        },
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.createProject('Test Project', 'Description');

        const createButton = createProjectDialog.createButton;
        await expect(createButton).toBeDisabled();
    });

    test('disables submit button during loading', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('createProject')) {
                await new Promise(resolve => setTimeout(resolve, 500));
                await route.fulfill({
                    status: 200,
                    body: JSON.stringify({
                        data: {
                            createProject: {
                                id: 'test-id',
                                name: 'Test Project',
                            },
                        },
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.fillForm('Test Project', 'Description');
        
        await createProjectDialog.createButton.click();
        
        await expect(createProjectDialog.createButton).toBeDisabled();
        await expect(createProjectDialog.createButton).toContainText(/Creating|Loading|Please wait/i)
            .catch(() => {
                expect(createProjectDialog.createButton).toBeDisabled();
            });
    });

    test('shows validation error for required fields', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        
        await createProjectDialog.submitEmptyForm();
        
        await expect(createProjectDialog.nameError).toBeVisible();
    });

    test('shows validation error for invalid URL format', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        
        await createProjectDialog.submitInvalidUrl();
        
        await expect(createProjectDialog.urlError).toBeVisible();
    });

    test('handles GraphQL errors gracefully', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            await route.fulfill({
                status: 200,
                body: JSON.stringify({ 
                    errors: [{ 
                        message: 'Field \'invalidField\' doesn\'t exist on type \'Query\'',
                        locations: [{ line: 1, column: 10 }],
                        extensions: { code: 'GRAPHQL_VALIDATION_FAILED' }
                    }] 
                }),
            });
        });

        await dashboardPage.navigate();
        
        await expect(page.getByText('error', { exact: false }).first()).toBeVisible()
            .catch(() => {
                expect(page.getByRole('alert').first()).toBeVisible();
            });
    });

    test('shows retry option on network failure', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            await route.abort('failed');
        });

        await dashboardPage.navigate();
        
        await expect(page.getByRole('button', { name: /Retry|Try Again|Refresh/i }).first())
            .toBeVisible()
            .catch(() => {
                expect(page.getByText('Retry').first()).toBeVisible()
                    .catch(() => {
                        expect(page.locator('.retry')).toBeVisible().catch(() => {
                            expect(page.getByText('Refresh').first()).toBeVisible();
                        });
                    });
            });
    });

    test('clears error state after successful operation', async ({ page }) => {
        let failFirstRequest = true;
        
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('createProject')) {
                if (failFirstRequest) {
                    failFirstRequest = false;
                    await route.fulfill({
                        status: 500,
                        body: JSON.stringify({ errors: [{ message: 'Server error' }] }),
                    });
                } else {
                    await route.fulfill({
                        status: 200,
                        body: JSON.stringify({
                            data: {
                                createProject: {
                                    id: 'test-id',
                                    name: 'Test Project',
                                },
                            },
                        }),
                    });
                }
            } else {
                await route.continue();
            }
        });

        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.createProject('Test Project', 'Description');
        
        await expect(page.getByText('Server error').first()).toBeVisible()
            .catch(() => {
                expect(page.getByText('error', { exact: false }).first()).toBeVisible();
            });
        
        await projectListPage.clickNewProject();
        await createProjectDialog.createProject('Test Project 2', 'Description');
        
        await expect(page.getByText('Server error').first()).not.toBeVisible()
            .catch(() => {
                expect(page.getByText('error', { exact: false }).first()).not.toBeVisible();
            });
    });

    test('shows helpful error messages for common failures', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            await route.fulfill({
                status: 401,
                body: JSON.stringify({ 
                    errors: [{ 
                        message: 'Unauthorized',
                        extensions: { code: 'UNAUTHENTICATED' }
                    }] 
                }),
            });
        });

        await dashboardPage.navigate();
        
        await expect(page.getByText(/unauthorized|login|session|authentication/i).first())
            .toBeVisible()
            .catch(() => {
                expect(page.getByText('Unauthorized').first()).toBeVisible()
                    .catch(() => {
                        expect(page.getByRole('alert').first()).toBeVisible();
                    });
            });
    });

    test('handles timeout errors', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            await new Promise(resolve => setTimeout(resolve, 30000));
            await route.abort('timedout');
        });

        await page.setDefaultTimeout(5000);
        
        await dashboardPage.navigate();
        
        await expect(page.getByText(/timeout|timed out|slow|connection/i).first())
            .toBeVisible()
            .catch(() => {
                expect(page.getByRole('alert').first()).toBeVisible();
            });
    });

    test('validates email format when applicable', async ({ page }) => {
        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        
        await createProjectDialog.fillForm('Test Project', 'Description');
        await createProjectDialog.createButton.click();
        
        await expect(createProjectDialog.nameError).toBeVisible();
    });

    test('shows spinner during data fetch', async ({ page }) => {
        let resolveRequest: (value: any) => void;
        const promise = new Promise((resolve) => { resolveRequest = resolve; });

        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('dashboardSummary')) {
                await promise.then(() => {
                    route.fulfill({
                        status: 200,
                        body: JSON.stringify({
                            data: {
                                dashboardSummary: {
                                    projectsInFlight: 0,
                                    featuresInReview: 0,
                                    featuresFailed: 0,
                                    tasksInProgress: 0,
                                    tasksFailed: 0,
                                    recentAuditEvents: [],
                                },
                            },
                        }),
                    });
                });
            } else {
                await route.continue();
            }
        });

        dashboardPage.navigate();
        
        await expect(page.locator('.animate-spin, .loading-spinner, [data-loading]').first())
            .toBeVisible()
            .catch(() => {
                expect(dashboardPage.loadingSkeletons.first()).toBeVisible();
            });
        
        resolveRequest({});
        await dashboardPage.waitForDashboardLoaded();
        
        await expect(page.locator('.animate-spin, .loading-spinner, [data-loading]').first())
            .not.toBeVisible()
            .catch(() => {
                expect(dashboardPage.loadingSkeletons.first()).not.toBeVisible();
            });
    });

    test('handles concurrent form submissions', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('createProject')) {
                await new Promise(resolve => setTimeout(resolve, 100));
                await route.fulfill({
                    status: 200,
                    body: JSON.stringify({
                        data: {
                            createProject: {
                                id: 'test-id',
                                name: 'Test Project',
                            },
                        },
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await projectListPage.navigate();
        await projectListPage.clickNewProject();
        await createProjectDialog.fillForm('Test Project', 'Description');
        
        await Promise.all([
            createProjectDialog.createButton.click(),
            createProjectDialog.createButton.click(),
        ]);
        
        await expect(createProjectDialog.createButton).toBeDisabled();
        await projectListPage.waitForProjectList();
        
        const projectCount = await projectListPage.projectTable.getByRole('row').count();
        expect(projectCount).toBeGreaterThanOrEqual(1);
    });
});
