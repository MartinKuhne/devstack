import { test, expect } from '@playwright/test';
import { DashboardPage } from './pages/DashboardPage.js';

test.describe('Dashboard Page', () => {
    let dashboardPage: DashboardPage;

    test.beforeEach(async ({ page }) => {
        dashboardPage = new DashboardPage(page);
    });

    test('navigates to dashboard page', async ({ page }) => {
        await dashboardPage.navigate();
        await expect(dashboardPage.pageTitle).toBeVisible();
        await expect(dashboardPage.welcomeText).toBeVisible();
    });

    test('renders summary cards', async ({ page }) => {
        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        await expect(dashboardPage.projectsInFlightCard).toBeVisible();
        await expect(dashboardPage.featuresInReviewCard).toBeVisible();
        await expect(dashboardPage.featuresFailedCard).toBeVisible();
        await expect(dashboardPage.tasksInProgressCard).toBeVisible();
        await expect(dashboardPage.tasksFailedCard).toBeVisible();
    });

    test('displays correct values in summary cards', async ({ page }) => {
        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        const projectsValue = await dashboardPage.projectsInFlightValue.textContent();
        const featuresInReviewValue = await dashboardPage.featuresInReviewValue.textContent();
        const featuresFailedValue = await dashboardPage.featuresFailedValue.textContent();
        const tasksInProgressValue = await dashboardPage.tasksInProgressValue.textContent();
        const tasksFailedValue = await dashboardPage.tasksFailedValue.textContent();

        expect(projectsValue).toBeTruthy();
        expect(featuresInReviewValue).toBeTruthy();
        expect(featuresFailedValue).toBeTruthy();
        expect(tasksInProgressValue).toBeTruthy();
        expect(tasksFailedValue).toBeTruthy();
    });

    test('shows empty state when no data exists', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('dashboardSummary')) {
                await route.fulfill({
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
            } else {
                await route.continue();
            }
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();
        await expect(dashboardPage.emptyStateMessage).toBeVisible();
    });

    test('displays recent audit events table', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('dashboardSummary')) {
                await route.fulfill({
                    status: 200,
                    body: JSON.stringify({
                        data: {
                            dashboardSummary: {
                                projectsInFlight: 1,
                                featuresInReview: 0,
                                featuresFailed: 0,
                                tasksInProgress: 0,
                                tasksFailed: 0,
                                recentAuditEvents: [
                                    {
                                        id: '1',
                                        entityType: 'Project',
                                        eventType: 'Created',
                                        actor: 'Test User',
                                        occurredAt: new Date().toISOString(),
                                    },
                                    {
                                        id: '2',
                                        entityType: 'Task',
                                        eventType: 'Updated',
                                        actor: 'Test User',
                                        occurredAt: new Date().toISOString(),
                                    },
                                ],
                            },
                        },
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        await expect(dashboardPage.auditEventsTable).toBeVisible();
        await expect(dashboardPage.page.getByText('Project')).toBeVisible();
        await expect(dashboardPage.page.getByText('Task')).toBeVisible();
    });

    test('shows loading states during initial load', async ({ page }) => {
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
        
        await expect(dashboardPage.loadingSkeletons.first()).toBeVisible();
        
        resolveRequest({});
        await dashboardPage.waitForDashboardLoaded();
        
        await expect(dashboardPage.loadingSkeletons.first()).not.toBeVisible();
    });

    test('refreshes dashboard data', async ({ page }) => {
        let callCount = 0;
        
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('dashboardSummary')) {
                callCount++;
                await route.fulfill({
                    status: 200,
                    body: JSON.stringify({
                        data: {
                            dashboardSummary: {
                                projectsInFlight: callCount,
                                featuresInReview: 0,
                                featuresFailed: 0,
                                tasksInProgress: 0,
                                tasksFailed: 0,
                                recentAuditEvents: [],
                            },
                        },
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        const initialProjectsValue = await dashboardPage.projectsInFlightValue.textContent();
        expect(initialProjectsValue).toBe('1');

        await dashboardPage.clickRefresh();
        await dashboardPage.waitForDashboardLoaded();

        const updatedProjectsValue = await dashboardPage.projectsInFlightValue.textContent();
        expect(updatedProjectsValue).toBe('2');
    });

    test('shows refresh button disabled during loading', async ({ page }) => {
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

        await dashboardPage.navigate();
        
        await expect(dashboardPage.refreshButton).toBeDisabled();
        
        resolveRequest({});
        await dashboardPage.waitForDashboardLoaded();
        
        await expect(dashboardPage.refreshButton).toBeEnabled();
    });

    test('navigates to failed features from dashboard link', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('dashboardSummary')) {
                await route.fulfill({
                    status: 200,
                    body: JSON.stringify({
                        data: {
                            dashboardSummary: {
                                projectsInFlight: 0,
                                featuresInReview: 0,
                                featuresFailed: 5,
                                tasksInProgress: 0,
                                tasksFailed: 0,
                                recentAuditEvents: [],
                            },
                        },
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        await expect(dashboardPage.viewFailedFeaturesButton).toBeVisible();
        await dashboardPage.clickViewFailedFeatures();
        
        await expect(page).toHaveURL(/\/features.*status=Failed/);
    });

    test('navigates to failed tasks from dashboard link', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('dashboardSummary')) {
                await route.fulfill({
                    status: 200,
                    body: JSON.stringify({
                        data: {
                            dashboardSummary: {
                                projectsInFlight: 0,
                                featuresInReview: 0,
                                featuresFailed: 0,
                                tasksInProgress: 0,
                                tasksFailed: 3,
                                recentAuditEvents: [],
                            },
                        },
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        await expect(dashboardPage.viewFailedTasksButton).toBeVisible();
        await dashboardPage.clickViewFailedTasks();
        
        await expect(page).toHaveURL(/\/tasks.*status=Failed/);
    });

    test('opens create project dialog from dashboard', async ({ page }) => {
        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        await dashboardPage.clickNewProject();
        await expect(dashboardPage.createProjectDialog).toBeVisible();
    });

    test('handles server error gracefully', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('dashboardSummary')) {
                await route.fulfill({
                    status: 500,
                    body: JSON.stringify({ errors: [{ message: 'Internal server error' }] }),
                });
            } else {
                await route.continue();
            }
        });

        await dashboardPage.navigate();
        
        await expect(dashboardPage.errorCard).toBeVisible();
        await expect(dashboardPage.errorMessage).toBeVisible();
    });

    test('shows background refresh indicator', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('dashboardSummary')) {
                await route.fulfill({
                    status: 200,
                    body: JSON.stringify({
                        data: {
                            dashboardSummary: {
                                projectsInFlight: 1,
                                featuresInReview: 0,
                                featuresFailed: 0,
                                tasksInProgress: 0,
                                tasksFailed: 0,
                                recentAuditEvents: [],
                            },
                        },
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        await dashboardPage.clickRefresh();
        
        await expect(dashboardPage.refreshingIndicator).toBeVisible();
    });

    test('displays audit event details in table', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('dashboardSummary')) {
                await route.fulfill({
                    status: 200,
                    body: JSON.stringify({
                        data: {
                            dashboardSummary: {
                                projectsInFlight: 1,
                                featuresInReview: 0,
                                featuresFailed: 0,
                                tasksInProgress: 0,
                                tasksFailed: 0,
                                recentAuditEvents: [
                                    {
                                        id: '1',
                                        entityType: 'Feature',
                                        eventType: 'Created',
                                        actor: 'John Doe',
                                        occurredAt: '2026-04-15T10:00:00Z',
                                    },
                                ],
                            },
                        },
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        await expect(dashboardPage.page.getByText('Feature')).toBeVisible();
        await expect(dashboardPage.page.getByText('Created')).toBeVisible();
        await expect(dashboardPage.page.getByText('John Doe')).toBeVisible();
    });

    test('shows "No recent activity" when audit events are empty', async ({ page }) => {
        await page.route('**/api/graphql', async (route) => {
            const request = route.request();
            const postData = request.postDataJSON();
            
            if (postData.query?.includes('dashboardSummary')) {
                await route.fulfill({
                    status: 200,
                    body: JSON.stringify({
                        data: {
                            dashboardSummary: {
                                projectsInFlight: 1,
                                featuresInReview: 0,
                                featuresFailed: 0,
                                tasksInProgress: 0,
                                tasksFailed: 0,
                                recentAuditEvents: [],
                            },
                        },
                    }),
                });
            } else {
                await route.continue();
            }
        });

        await dashboardPage.navigate();
        await dashboardPage.waitForDashboardLoaded();

        await expect(dashboardPage.noActivityMessage).toBeVisible();
    });
});
